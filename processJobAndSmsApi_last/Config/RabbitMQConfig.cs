public class RabbitMQConfig
{
    public string HostName { get; set; } = "localhost";
    public string QueueName { get; set; } = "dlr_queue";
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
}