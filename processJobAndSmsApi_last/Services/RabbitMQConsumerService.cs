using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class RabbitMQConsumerService
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly string _queueName;

    public RabbitMQConsumerService(RabbitMQService rabbitMQService, ILogger<RabbitMQConsumerService> logger, RabbitMQConfig config)
    {
        _channel = rabbitMQService.Channel;
        _logger = logger;
        _queueName = config.QueueName;
    }

    public void StartConsuming(Func<string, Task> processMessageAsync)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await processMessageAsync(message);
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from RabbitMQ.");
            }
        };

        _channel.BasicConsume(queue: _queueName,
                             autoAck: false,
                             consumer: consumer);

        _logger.LogInformation("Started consuming messages from RabbitMQ.");
    }
}