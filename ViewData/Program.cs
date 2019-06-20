using System;
using System.Collections.Generic;
using ManagerQueue;

namespace ViewData
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
            var queue = new QueueTransit("Config.json", "ConfigTransite.json");
            while (true)
            {
                queue.TransitData();
                queue.GetData();
                if (queue.ServiceOperability() == false) break;
            }
        }
    }
}
