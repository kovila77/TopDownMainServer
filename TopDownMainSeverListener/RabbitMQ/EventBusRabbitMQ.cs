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

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }
        }
    }
}
