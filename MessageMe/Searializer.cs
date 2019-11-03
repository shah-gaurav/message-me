using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Message.Me
{
    public static class Serializer
    {

        public static T DeserializeFromFile<T>(string filePath) where T : class
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), jsonSerializerSettings);
        }

        public static void SerializeToFile<T>(T obj, string filePath) where T : class
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            File.WriteAllText(filePath, JsonConvert.SerializeObject(obj, jsonSerializerSettings));
        }
    }
}
