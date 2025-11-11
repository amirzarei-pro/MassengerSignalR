using LiteDB;

namespace SampleMassengerSignalR.Models;

public class MessageRecord
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public string ChatKey { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
