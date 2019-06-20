using System;
using Newtonsoft.Json;
using System.IO;

namespace ManagerQueue
{
    public class Config
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string HostName { get; set; }
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }

        public Config GetConfig(string path)
        {
            try
            {
                string json;
                using (var jsonString = new StreamReader(path))
                {
                    json = jsonString.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }
    }
}
