public class OutboxWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var unprocessed = FakeDb.Outbox.Where(m => !m.IsProcessed).ToList();

            foreach (var message in unprocessed)
            {
                // Публикация сообщения в брокер
                Console.WriteLine($"[RabbitMQ] Published event: {message.EventType} -> {message.Payload}");

                message.IsProcessed = true;
            }

            await Task.Delay(3000, stoppingToken);
        }
    }
}