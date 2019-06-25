using System;
using System.Collections.Generic;
using ManagerQueue;

namespace ViewData
{
    class Program
    {
        static void Main(string[] args)
        {
            const string configForFirstQueue = "Config.json";
            const string configForSecondQueue = "ConfigTransite.json";
            Console.WriteLine("Service start");
            var queue = new QueueTransit(configForFirstQueue, configForSecondQueue);
            while (true)
            {
                queue.TransitData();
                queue.GetData();
                if (queue.ServiceOperability() == false)
                {
                    Console.WriteLine("Service stop");
                    break;
                }
            }
        }
    }
}
