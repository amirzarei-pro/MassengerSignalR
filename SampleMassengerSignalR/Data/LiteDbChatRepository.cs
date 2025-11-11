using LiteDB;
using Microsoft.Extensions.Hosting;
using SampleMassengerSignalR.Models;

namespace SampleMassengerSignalR.Data;

public interface IChatRepository
{
    // Messages
    void SaveMessage(MessageRecord msg);
    IEnumerable<MessageRecord> GetChat(string userA, string userB, int skip = 0, int take = 50);
    IEnumerable<ConversationSummary> GetUserConversations(string username, int skip = 0, int take = 50);

    // Users
    UserInfo AddUser(string userName, string name, string? connectionId);
    UserInfo? GetUser(string userName);
    IEnumerable<UserInfo> ListUsers();
    void RemoveByConnection(string connectionId);

    // Misc
    bool HasAnyMessages();
}

public class LiteDbChatRepository : IChatRepository
{
    private readonly string _dbPath;

    public LiteDbChatRepository(IHostEnvironment env)
    {
        _dbPath = Path.Combine(env.ContentRootPath, "App_Data", "chat.db");
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
    }

    private static string ChatKeyOf(string a, string b)
    {
        var p = new[] { a.Trim().ToLowerInvariant(), b.Trim().ToLowerInvariant() };
        Array.Sort(p);
        return $"{p[0]}__{p[1]}";
    }

    // Messages
    public void SaveMessage(MessageRecord msg)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<MessageRecord>("messages");
        col.EnsureIndex(x => x.ChatKey);
        col.EnsureIndex(x => x.SentAt);
        col.EnsureIndex(x => x.From);
        col.EnsureIndex(x => x.To);
        col.Insert(msg);
    }

    public IEnumerable<MessageRecord> GetChat(string userA, string userB, int skip = 0, int take = 50)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<MessageRecord>("messages");
        var key = ChatKeyOf(userA, userB);
        return col.Query()
            .Where(x => x.ChatKey == key)
            .OrderBy(x => x.SentAt)
            .Skip(skip)
            .Limit(take)
            .ToList();
    }

    public IEnumerable<ConversationSummary> GetUserConversations(string username, int skip = 0, int take = 50)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<MessageRecord>("messages");
        var u = username.Trim().ToLowerInvariant();

        // Fetch all messages where user is either sender or receiver
        var list = col.Query()
            .Where(x => x.From.ToLower() == u || x.To.ToLower() == u)
            .ToList();

        // Group in memory by peer
        var groups = list
            .GroupBy(m => m.From.Equals(username, StringComparison.OrdinalIgnoreCase) ? m.To.ToLowerInvariant() : m.From.ToLowerInvariant());

        var summaries = groups
            .Select(g => new ConversationSummary
            {
                Peer = g.Key,
                LastFrom = g.OrderByDescending(x => x.SentAt).First().From,
                LastText = g.OrderByDescending(x => x.SentAt).First().Text,
                LastAt = g.Max(x => x.SentAt),
                TotalMessages = g.Count()
            })
            .OrderByDescending(s => s.LastAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        return summaries;
    }

    // Users
    public UserInfo AddUser(string userName, string name, string? connectionId)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<UserInfo>("users");
        col.EnsureIndex(x => x.UserName, true);

        var existing = col.FindOne(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            var u = new UserInfo
            {
                UserName = userName,
                Name = name,
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow
            };
            col.Insert(u);
            return u;
        }
        else
        {
            existing.Name = name;
            existing.ConnectionId = connectionId;
            existing.ConnectedAt = DateTime.UtcNow;
            col.Update(existing);
            return existing;
        }
    }

    public UserInfo? GetUser(string userName)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<UserInfo>("users");
        return col.FindOne(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<UserInfo> ListUsers()
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<UserInfo>("users");
        return col.FindAll()
            .Select(u => new UserInfo { UserName = u.UserName, Name = u.Name, ConnectedAt = u.ConnectedAt })
            .ToList();
    }

    public void RemoveByConnection(string connectionId)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<UserInfo>("users");
        var u = col.FindOne(x => x.ConnectionId == connectionId);
        if (u != null)
        {
            col.Delete(u.Id);
        }
    }

    public bool HasAnyMessages()
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<MessageRecord>("messages");
        return col.Count() > 0;
    }
}
