# SampleMassengerSignalR

Lightweight demo chat application using ASP.NET Core SignalR.

Key points
- .NET 9 project (C# 13)
- Real-time messaging via SignalR Hub `RealtimeHub`
- Minimal repository abstraction (`IChatRepository`) for messages/users (example uses MongoDB types)

Quick start
1. Prerequisites
   - .NET 9 SDK
   - (Optional) MongoDB running if using the included Mongo-based repository implementation. If not using Mongo, the project may include an in-memory fallback.

2. Build and run
   - From solution root:
     - `cd SampleMassengerSignalR`
     - `dotnet build`
     - `dotnet run`
   - The app runs with Kestrel; default URLs appear in console (commonly `http://localhost:5000` / `https://localhost:5001`).

3. Web client / SignalR
   - The SignalR hub is implemented in `Hubs/RealtimeHub.cs` and is typically mapped at `/realtime` (check `Program.cs` or `Startup.cs` in the project to confirm).
   - The hub methods available to clients include:
     - `Register(userName, name)` — register/login a user
     - `CheckUser(userName)` — check existence of a user
     - `GetConversations(userName)` — get conversation summaries
     - `GetMessages(from, to)` — get chat messages
     - `SendMessage(from, to, text)` — send a message
   - Hub events emitted to clients:
     - `Connected` — sent to the caller with connection id
     - `users` — updated user list
     - `message` — new message
     - `conversations` — updated conversation list

Configuration
- Check `appsettings.json` for database/connection settings (for MongoDB or other stores).
- If using MongoDB, ensure the connection string and database name are set before running.

Development notes
- Core abstractions live in `Data/` and models in `Models/`.
- `IChatRepository` provides a swap-out point for in-memory vs persistent stores.

Contributing
- Fork -> branch -> PR. Keep changes focused and include tests where feasible.

License
- See repository root for a `LICENSE` file if provided.

If you need a step-by-step development guide, see `GUIDE.md`.