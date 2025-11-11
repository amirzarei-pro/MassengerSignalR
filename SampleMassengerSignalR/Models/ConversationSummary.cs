namespace SampleMassengerSignalR.Models;

public class ConversationSummary
{
 public string Peer { get; set; } = string.Empty;
 public string LastFrom { get; set; } = string.Empty;
 public string LastText { get; set; } = string.Empty;
 public DateTime LastAt { get; set; }
 public int TotalMessages { get; set; }
}
