using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Message.Me
{
    public class Utilities
    {         

        public static string GetChannelDirectory(string channelName)
        {
            var sanitizedChannelName = channelName;
            Path.GetInvalidPathChars().ToList().ForEach(c => sanitizedChannelName = sanitizedChannelName.Replace(c, '_'));
            var channelDirectoryPath = Path.Combine(Path.GetTempPath(), sanitizedChannelName);
            if (!Directory.Exists(channelDirectoryPath))
            {
                Directory.CreateDirectory(channelDirectoryPath);
            }
            
            return channelDirectoryPath;
        }
    }
}
