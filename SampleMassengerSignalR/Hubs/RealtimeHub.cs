using Microsoft.AspNetCore.SignalR;
using SampleMassengerSignalR.Data;
using SampleMassengerSignalR.Models;
using Microsoft.AspNetCore.Hosting;

namespace SampleMassengerSignalR.Hubs;

public class RealtimeHub : Hub
{
    private readonly IChatRepository _repo;

    public RealtimeHub(IChatRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;

        // If stores are empty, seed demo data (only the first time)
        if (!_repo.HasAnyMessages())
        {
            // seed two users and some messages
            var u1 = _repo.AddUser("alice", "Alice", null);
            var u2 = _repo.AddUser("bob", "Bob", null);

            var m1 = new MessageRecord
            {
                From = "alice",
                To = "bob",
                Text = "Hi Bob!",
                ChatKey = BuildKey("alice", "bob"),
                SentAt = DateTime.UtcNow.AddMinutes(-10)
            };
            _repo.SaveMessage(m1);

            var m2 = new MessageRecord
            {
                From = "bob",
                To = "alice",
                Text = "Hello Alice!",
                ChatKey = BuildKey("alice", "bob"),
                SentAt = DateTime.UtcNow.AddMinutes(-9)
            };
            _repo.SaveMessage(m2);

            var m3 = new MessageRecord
            {
                From = "alice",
                To = "bob",
                Text = "How are you?",
                ChatKey = BuildKey("alice", "bob"),
                SentAt = DateTime.UtcNow.AddMinutes(-8)
            };
            _repo.SaveMessage(m3);
        }
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _repo.RemoveByConnection(Context.ConnectionId);
        await Clients.All.SendAsync("users", _repo.ListUsers());
        await base.OnDisconnectedAsync(exception);
    }

    // register/login
    public async Task<object> Register(string userName, string name)
    {
        userName = userName.ToLower().Trim();
        var user = _repo.AddUser(userName, name, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, userName); // personal room

        await Clients.All.SendAsync("users", _repo.ListUsers()); // notify all users

        return new { success = true, user };
    }

    // check if user exists
    public Task<object> CheckUser(string userName)
    {
        userName = userName.ToLower().Trim();
        if (string.IsNullOrWhiteSpace(userName))
            return Task.FromResult<object>(new { success = false, error = "userName required" });

        var user = _repo.GetUser(userName);
        return Task.FromResult<object>(new
        {
            success = true,
            exists = user != null,
            user = user != null ? new { user.UserName, user.Name, user.ConnectedAt } : null
        });
    }

    // get conversations
    public Task<IEnumerable<ConversationSummary>> GetConversations(string userName)
    {
        userName = userName.ToLower().Trim();
        var convs = _repo.GetUserConversations(userName);
        return Task.FromResult(convs);
    }

    // get messages between two users
    public async Task<object> GetMessages(string from, string to)
    {
        from = from.ToLower().Trim();
        to = to.ToLower().Trim();

        var conv = _repo.GetChat(from, to).ToList();
        var user = _repo.GetUser(to);
        return await Task.FromResult<object>(new { messages = conv, user });
    }

    // send message
    public async Task<object> SendMessage(string from, string to, string text)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(text))
            return new { success = false };

        from = from.ToLower().Trim();
        to = to.ToLower().Trim();

        var record = new MessageRecord
        {
            From = from,
            To = to,
            Text = text,
            ChatKey = BuildKey(from, to),
            SentAt = DateTime.UtcNow
        };

        _repo.SaveMessage(record);

        // send to both users via their groups (personal room)
        await Clients.Group(from).SendAsync("message", record);
        await Clients.Group(to).SendAsync("message", record);

        // update conversations for both
        await Clients.Group(from).SendAsync("conversations", _repo.GetUserConversations(from));
        await Clients.Group(to).SendAsync("conversations", _repo.GetUserConversations(to));

        return new { success = true, message = record };
    }

    private static string BuildKey(string a, string b)
    {
        var p = new[] { a.Trim().ToLowerInvariant(), b.Trim().ToLowerInvariant() };
        Array.Sort(p);
        return $"{p[0]}__{p[1]}";
    }
}
