using Sample.Common;
using System;

namespace Sample.Client.Consoles
{
    class Program
    {
        static void Main(string[] args)
        {
           var proxy = Message.Me.Client.GetServiceProxy<IHello>(Constants.CHANNEL_NAME, timeoutInSeconds: 1);
            Console.WriteLine("Press enter to send message...");
            Console.ReadLine();
            Console.WriteLine($"Response: {proxy.SayHello("Gaurav")}");
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
