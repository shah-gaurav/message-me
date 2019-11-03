using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Message.Me
{
    public class Client
    {
        class Proxy<T> : IInterceptor
        {
            public string ChannelName { get; private set; }
            public int TimeoutInSeconds { get; set; }

            public Proxy(string channelName, int timeout)
            {
                ChannelName = channelName;
                TimeoutInSeconds = timeout;
            }

            public void Intercept(IInvocation invocation)
            {
                var parameterTypes = new List<Type>();
                foreach(var paramInfo in invocation.Method.GetParameters().OrderBy(p => p.Position))
                {
                    parameterTypes.Add(paramInfo.ParameterType);
                }
                // build message for intercepted call
                Request msg = new Request()
                {
                    Service = typeof(T).Name,
                    Method = invocation.Method.Name,
                    Parameters = invocation.Arguments,
                    ParameterTypes = parameterTypes.ToArray()
                };

                // send message
                invocation.ReturnValue = SendMessage(msg);
            }

            protected object SendMessage(Request message)
            {
                var channelDirectory = Utilities.GetChannelDirectory(ChannelName);

                var messageId = Guid.NewGuid().ToString();

                var messageFilePath = Path.Combine(channelDirectory, $"{messageId}.json");
                Serializer.SerializeToFile<Request>(message, messageFilePath);

                var returnMessageFilePath = messageFilePath + ".response";

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                while (true)
                {

                    if (stopWatch.Elapsed > TimeSpan.FromSeconds(TimeoutInSeconds))
                    {
                        stopWatch.Stop();
                        throw new TimeoutException("No response in specified time");
                    }

                    if (File.Exists(returnMessageFilePath) && DateTime.Now.Subtract(File.GetLastWriteTime(returnMessageFilePath)) > TimeSpan.FromMilliseconds(100))
                    {
                        stopWatch.Stop();
                        var returnMessage = Serializer.DeserializeFromFile<Response>(returnMessageFilePath);
                        if (returnMessage.IsSuccessful)
                        {
                            return returnMessage.ReturnValue == null ? null : Convert.ChangeType(returnMessage.ReturnValue, returnMessage.ReturnType);
                        }
                        else
                        {
                            throw new ApplicationException(returnMessage.Error);
                        }
                    }

                    Thread.Sleep(100);
                }
            }
        }

        public static T GetServiceProxy<T>(string channelName, int timeoutInSeconds = 5)
        {
            return (T)new ProxyGenerator().CreateInterfaceProxyWithoutTarget(typeof(T), new Proxy<T>(channelName, timeoutInSeconds));
        }

    }

}
