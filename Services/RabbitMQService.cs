using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace SIA.Services
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQService(string hostname, string username, string password)
        {
            _factory = new ConnectionFactory()
            {
                HostName = hostname,
                UserName = username,
                Password = password
            };

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void ConsumeMessages(string queueName, Action<string> onMessageReceived)
        {
            // Declarar la cola
            _channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                onMessageReceived(message);
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
