namespace SampleMassengerSignalR.Models;

public class UserInfo
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
    public DateTime ConnectedAt { get; set; }
}
