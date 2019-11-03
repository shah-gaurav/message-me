using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Message.Me
{
    public class Server
    {
        private Dictionary<string, FileSystemWatcher> watcherList = new Dictionary<string, FileSystemWatcher>(StringComparer.InvariantCultureIgnoreCase);
        readonly Dictionary<string, object> services = new Dictionary<string, object>();
        readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        public void RegisterService<T>(T instance, string channelName)
        {
            if (!(instance is T))
                throw new InvalidOperationException("Instance must implement service interface");

            services[typeof(T).Name] = instance;
            types[typeof(T).Name] = typeof(T);

            FileSystemWatcher watcher;

            var folder = Utilities.GetChannelDirectory(channelName);
            watcher = new FileSystemWatcher(folder, "*.json")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite
            };

            watcher.Changed += OnMessageReceived;
            watcherList.Add(channelName, watcher);
        }

        public void DeregisterService<T>(string channelName)
        {
            services.Remove(typeof(T).Name);
            types.Remove(typeof(T).Name);
            watcherList.Remove(channelName);
        }

        private BlockingCollection<String> workQueue = new BlockingCollection<String>();
        private Thread workerThread;

        public void Start()
        {
            workerThread = new Thread(() =>
            {
                foreach (string value in workQueue.GetConsumingEnumerable())
                {
                    ProcessMessage(value);
                }
                Console.WriteLine("Exiting thread");
            });
            workerThread.Start();

            foreach (var watcher in watcherList)
            {
                EmptyDirectory(watcher.Value.Path);
                watcher.Value.EnableRaisingEvents = true;
            }
        }

        private void EmptyDirectory(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.EnumerateFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch { }
            }
        }

        public virtual void Stop()
        {
            workQueue.CompleteAdding();
            foreach (var watcher in watcherList)
            {
                watcher.Value.EnableRaisingEvents = false;
            }
        }

        private void OnMessageReceived(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            workQueue.Add(e.FullPath);
        }

        private void ProcessMessage(string fullPath)
        {
            var message = Serializer.DeserializeFromFile<Request>(fullPath);

            object returnValue = null;
            string errorMessage = string.Empty;
            Type returnType = typeof(object);
            // find the service
            if (services.TryGetValue(message.Service, out object instance) && instance != null)
            {
                // get the method
                System.Reflection.MethodInfo method = instance.GetType().GetMethod(message.Method);

                // double check method existence against type-list for security
                // typelist will contain interfaces instead of instances
                if (types[message.Service].GetMethod(message.Method) != null && method != null)
                {
                    try
                    {
                        var parameters = new List<Object>();
                        for (int i = 0; i < message.Parameters.Length; i++)
                        {
                            parameters.Add(message.Parameters[i] == null ? null : Convert.ChangeType(message.Parameters[i], message.ParameterTypes[i]));
                        }
                        // invoke method
                        returnValue = method.Invoke(instance, parameters.ToArray());
                        returnType = method.ReturnType;
                        Console.WriteLine($"Return Value: {returnValue}");
                    }
                    catch (Exception ex) { errorMessage = ex.ToString(); }

                }
                else
                {
                    errorMessage = "Could not find method";
                }
            }
            else
            {
                errorMessage = "Could not find service";
            }


            try
            {
                var returnMessage = new Response()
                {
                    IsSuccessful = errorMessage == string.Empty,
                    Error = errorMessage,
                    ReturnValue = returnValue,
                    ReturnType = returnType
                };

                string responseFilePath = Path.Combine(Path.GetDirectoryName(fullPath), $"{Path.GetFileName(fullPath)}.response");
                Serializer.SerializeToFile<Response>(returnMessage, responseFilePath);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(fullPath), $"{Path.GetFileName(fullPath)}.response"), ex.ToString());
            }

        }

    }

}
