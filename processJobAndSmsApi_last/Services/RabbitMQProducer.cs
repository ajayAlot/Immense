using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public class RabbitMQProducer
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQProducer> _logger;
    private readonly string _queueName;

    public RabbitMQProducer(RabbitMQService rabbitMQService, ILogger<RabbitMQProducer> logger, RabbitMQConfig config)
    {
        _channel = rabbitMQService.Channel;
        _logger = logger;
        _queueName = config.QueueName;
    }

    public Task PublishMessageAsync<T>(T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        _channel.BasicPublish(exchange: "",
                             routingKey: _queueName,
                             basicProperties: null,
                             body: body);

        _logger.LogInformation("Published message to RabbitMQ queue.");
        return Task.CompletedTask;
    }
}