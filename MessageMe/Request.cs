using System;
using System.Collections.Generic;
using System.Text;

namespace Message.Me
{
    public class Request
    {
        public string Service { get; set; }
        public string Method { get; set; }
        public object[] Parameters { get; set; }
        public Type[] ParameterTypes { get; set; }
    }
}
