using RabbitMQ.Client;

public class RabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService(RabbitMQConfig config)
    {
        var factory = new ConnectionFactory() { HostName = config.HostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: config.QueueName,
                             durable: config.Durable,
                             exclusive: config.Exclusive,
                             autoDelete: config.AutoDelete,
                             arguments: null);
    }

    public IModel Channel => _channel;

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}