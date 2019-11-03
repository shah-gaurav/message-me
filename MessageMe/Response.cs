using System;
using System.Collections.Generic;
using System.Text;

namespace Message.Me
{
    public class Response
    {
        public bool IsSuccessful { get; set; }
        public object ReturnValue { get; set; }
        public Type ReturnType { get; set; }
        public string Error { get; set; }
    }
}
