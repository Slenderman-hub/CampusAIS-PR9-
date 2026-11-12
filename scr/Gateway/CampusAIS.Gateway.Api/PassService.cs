using System.Text.Json;

public static class FakeDb
{
    public static Dictionary<Guid, bool> Passes = new();
    public static List<OutboxMessage> Outbox = new();
}

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PassService
{
    // Транзакционный отзыв пропуска и сохранение события в Outbox
    public void RevokePass(Guid passId)
    {
        try
        {
            if (!FakeDb.Passes.ContainsKey(passId))
                FakeDb.Passes[passId] = true;

            FakeDb.Passes[passId] = false;

            var message = new OutboxMessage
            {
                EventType = "PassRevoked",
                Payload = JsonSerializer.Serialize(new { PassId = passId, RevokedAt = DateTime.UtcNow }),
                IsProcessed = false
            };
            FakeDb.Outbox.Add(message);

            Console.WriteLine($"[PassService] Pass {passId} revoked. Event added to Outbox.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PassService] Transaction failed, rolling back: {ex.Message}");
        }
    }
}