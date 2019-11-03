using Sample.Common;
using System;

namespace Sample.Server
{
    internal class HelloService : IHello
    { 
        public string SayHello(string name)
        {
            return $"Hello, {name}!";
        }
    }
}