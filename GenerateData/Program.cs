using System;
using ManagerQueue;

namespace GenerateData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Service start");
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