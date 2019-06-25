using System;
using System.Diagnostics;
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
        public Config Config;
        public ConnectionFactory Factory;
        public IConnection Connection;
        public IModel Model;
        private const string _CommandStop = "stop"; 

        public Queue(string configFile)
        {
            try
            {
                this.Config = new Config().GetConfig(configFile);
                this.QueueName = Config.QueueName;
                
                this.Factory = new ConnectionFactory
                {
                    UserName = Config.UserName,
                    Password = Config.Password,
                    VirtualHost = Config.VirtualHost,
                    HostName = Config.HostName

                };

                this.Connection = Factory.CreateConnection();
                this.Model = Connection.CreateModel();
                Model.ExchangeDeclare(Config.ExchangeName, ExchangeType.Direct);
                Model.QueueDeclare(Config.QueueName, false, false, false, null);
                Model.QueueBind(Config.QueueName, Config.ExchangeName, Config.RoutingKey, null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Process.GetCurrentProcess().Kill();
            }
        }

        public void SetData(string data)
        {
            try
            {
                if (data == _CommandStop)
                {
                    byte[] messageStop = Encoding.UTF8.GetBytes(data);
                    Model.BasicPublish(Config.ExchangeName, Config.RoutingKey, null, messageStop);
                    ServiceWork = false;
                    return;
                }

                byte[] messageBodyBytes = Encoding.UTF8.GetBytes(data);
                Model.BasicPublish(Config.ExchangeName, Config.RoutingKey, null, messageBodyBytes);
            }
            catch (Exception exception)
            {
                byte[] messageStop = Encoding.UTF8.GetBytes(_CommandStop);
                Model.BasicPublish(Config.ExchangeName, Config.RoutingKey, null, messageStop);
                ServiceWork = false;
                Console.WriteLine(exception);
            }
        }

        public bool ServiceOperability()
        {
            if (ServiceWork == true)
            {
                return true;
            }
            else
            {
                if (Model != null && Connection != null)
                {
                    Model.Close();
                    Connection.Close();
                }
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
        private const string _CommandStop = "stop";

        public QueueTransit(string configForFirstQueue, string configForSecondQueue)
        {
            try
            {
                this.FirstQueue = new Queue(configForFirstQueue);
                this.SubscriptionForFirstQueue = new Subscription(FirstQueue.Model, FirstQueue.QueueName, false);
                this.SecondQueue = new Queue(configForSecondQueue);
                this.SubscriptionForSecondQueue = new Subscription(SecondQueue.Model, SecondQueue.QueueName, false);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Process.GetCurrentProcess().Kill();
            }
        }

        public void TransitData()
        {
            try
            {
                var basicDeliveryEventArgs = SubscriptionForFirstQueue.Next();
                var messageContent = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);
                if (messageContent == _CommandStop)
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
                ServiceWork = false;
                SendToTwoQueue(_CommandStop);
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
                if (messageContent == _CommandStop)
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
                ServiceWork = false;
            }
        }

        public bool ServiceOperability()
        {
            if (ServiceWork == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
