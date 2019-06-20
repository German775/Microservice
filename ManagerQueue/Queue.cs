using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;

namespace ManagerQueue
{
    public class Queue
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public bool ServiceWork = true;

        public ConnectionFactory Factory;
        public IConnection Connection;
        public IModel Model;

        public Queue(string configFile)
        {
            var settings = new Config().GetConfig(configFile);
            this.ExchangeName = settings.ExchangeName;
            this.QueueName = settings.QueueName;
            this.RoutingKey = settings.RoutingKey;

            this.Factory = new ConnectionFactory
            {
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                HostName = settings.HostName

            };

            this.Connection = Factory.CreateConnection();
            this.Model = Connection.CreateModel();
            Model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            Model.QueueDeclare(QueueName, false, false, false, null);
            Model.QueueBind(QueueName, ExchangeName, RoutingKey, null);
        }

        public void SetData(string data)
        {
            try
            {
                if (data == "stop")
                {
                    byte[] messageStop = Encoding.UTF8.GetBytes(data);
                    Model.BasicPublish(ExchangeName, RoutingKey, null, messageStop);
                    ServiceWork = false;
                    return;
                }

                byte[] messageBodyBytes = Encoding.UTF8.GetBytes(data);
                Model.BasicPublish(ExchangeName, RoutingKey, null, messageBodyBytes);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public bool ServiceOperability()
        {
            if (ServiceWork == true) return true;
            else
            {
                Model.Close();
                Connection.Close();
                Console.WriteLine("Service stop");
                return false;
            }
        }
    }

    public class QueueTransit
    {
        public Subscription SubscriptionForFirstQueue;
        public Subscription SubscriptionForSecondQueue;
        public Queue FirstQueue;
        public Queue SecondQueue;
        public bool ServiceWork = true;

        public QueueTransit(string configForFirstQueue, string configForSecondQueue)
        {
            this.FirstQueue = new Queue(configForFirstQueue);
            this.SubscriptionForFirstQueue = new Subscription(FirstQueue.Model, FirstQueue.QueueName, false);
            this.SecondQueue = new Queue(configForSecondQueue);
            this.SubscriptionForSecondQueue = new Subscription(SecondQueue.Model, SecondQueue.QueueName, false);
        }

        public void TransitData()
        {
            try
            {
                var basicDeliveryEventArgs = SubscriptionForFirstQueue.Next();
                var messageContent = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);
                if (messageContent == "stop")
                {
                    SubscriptionForFirstQueue.Ack(basicDeliveryEventArgs);
                    FirstQueue.Model.Close();
                    FirstQueue.Connection.Close();
                    SubscriptionForFirstQueue.Close();
                    SendToTwoQueue(messageContent);
                    return;
                }

                Console.WriteLine($"Data: '{messageContent}' send to Queues #2");
                SubscriptionForFirstQueue.Ack(basicDeliveryEventArgs);
                SendToTwoQueue(messageContent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private void SendToTwoQueue(string data)
        {
            SecondQueue.SetData(data);
        }

        public void GetData()
        {
            try
            {
                var basicDeliveryEventArgs = SubscriptionForSecondQueue.Next();
                var messageContent = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);
                if (messageContent == "stop")
                {
                    SubscriptionForSecondQueue.Ack(basicDeliveryEventArgs);
                    SubscriptionForSecondQueue.Close();
                    FirstQueue.Model.Close();
                    FirstQueue.Connection.Close();
                    ServiceWork = false;
                    return;
                }
                Console.WriteLine("Get data from queue #2: " + messageContent);
                SubscriptionForSecondQueue.Ack(basicDeliveryEventArgs);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public bool ServiceOperability()
        {
            if (ServiceWork == true) return true;
            else
            {
                Console.WriteLine("Service stop");
                return false;
            }
        }
    }
}
