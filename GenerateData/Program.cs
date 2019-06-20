using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ManagerQueue;
using Newtonsoft.Json;
using RabbitMQ.Client.Framing.Impl;

namespace GenerateData
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var command = Console.ReadLine();
                if (command == "start")
                {
                    Start();
                }
            }
        }

        public static void Start()
        {
            var queue = new Queue("Config.json");
            while (true)
            {
                Console.WriteLine("Set data from queue #1: ");
                queue.SetData(Console.ReadLine());
                if (queue.ServiceOperability() == false) break;
            }
        }
    }
}