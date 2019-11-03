using Sample.Common;
using System;

namespace Sample.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Message.Me.Server();
            server.RegisterService<IHello>(new HelloService(), Constants.CHANNEL_NAME);            
            server.Start();
            Console.WriteLine("Server Started. Press enter to exit...");
            Console.ReadLine();
            server.Stop();
            server.DeregisterService<IHello>(Constants.CHANNEL_NAME);
        }
    }
}
