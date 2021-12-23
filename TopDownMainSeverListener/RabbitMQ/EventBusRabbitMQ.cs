using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PostgresEntities.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TopDownMainServer;

namespace TopDownMainSeverListener.RabbitMQ
{
    public class EventBusRabbitMQ : IDisposable
    {
        private readonly RabbitMQPersistentConnection _persistentConnection;
        private IModel _consumerChannel;
        private string _queueName;
        ServerService _serverService = new ServerService();

        public EventBusRabbitMQ(RabbitMQPersistentConnection persistentConnection, string queueName = null)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _queueName = queueName;
        }

        public IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            if (_consumerChannel is { IsOpen: true })
            {
                _consumerChannel.Dispose();
            }

            var channel = _persistentConnection.CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            //consumer.Received += async (model, e) =>
            //{
            //    var eventName = e.RoutingKey;
            //    var message = Encoding.UTF8.GetString(e.Body);
            //    channel.BasicAck(e.DeliveryTag, multiple: false);
            //};

            //Create event when something receive
            consumer.Received += ReceivedEvent;


            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                CreateConsumerChannel();
            };

            _consumerChannel = channel;
            return channel;
        }

        private void ReceivedEvent(object sender, BasicDeliverEventArgs e)
        {   
            if (e.RoutingKey == _queueName)
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Server server = JsonConvert.DeserializeObject<Server>(message);
                _serverService.NewServer(server);
            }
        }

        //// from example
        //public void PublishUserSaveFeedback(string _queueName, UserSaveFeedback publishModel, IDictionary<string, object> headers)
        //{

        //    if (!_persistentConnection.IsConnected)
        //    {
        //        _persistentConnection.TryConnect();
        //    }

        //    using (var channel = _persistentConnection.CreateModel())
        //    {

        //        channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        //        var message = JsonConvert.SerializeObject(publishModel);
        //        var body = Encoding.UTF8.GetBytes(message);

        //        IBasicProperties properties = channel.CreateBasicProperties();
        //        properties.Persistent = true;
        //        properties.DeliveryMode = 2;
        //        properties.Headers = headers;
        //        // properties.Expiration = "36000000";
        //        //properties.ContentType = "text/plain";

        //        channel.ConfirmSelect();
        //        channel.BasicPublish(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: properties, body: body);
        //        channel.WaitForConfirmsOrDie();

        //        channel.BasicAcks += (sender, eventArgs) =>
        //        {
        //            Console.WriteLine("Sent RabbitMQ");
        //            //implement ack handle
        //        };
        //        channel.ConfirmSelect();
        //    }
        //}

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }
        }
    }
}
