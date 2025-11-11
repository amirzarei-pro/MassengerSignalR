GUIDE for Developers

Overview
This project is a demo chat application built with ASP.NET Core SignalR. It contains a `RealtimeHub` SignalR hub, a repository abstraction `IChatRepository`, and model types under `Models/`.

Repository structure
- `SampleMassengerSignalR/` - main project folder
  - `Hubs/RealtimeHub.cs` - SignalR hub that handles registration, messaging, and conversation listing
  - `Data/` - repository implementations and interfaces (look for `IChatRepository`)
  - `Models/` - model classes: `MessageRecord`, `UserInfo`, `ConversationSummary`

Running locally
1. Install .NET 9 SDK from Microsoft if not installed.
2. (Optional) Run MongoDB locally or use an in-memory repo if available.
3. From the solution folder:
   - `cd SampleMassengerSignalR`
   - `dotnet build`
   - `dotnet run`

Connecting a client
- The hub is available at the path configured in `Program.cs`. By default, it may use `/realtime`.
- Use the official SignalR JavaScript client to connect from a web UI:
  - `const connection = new signalR.HubConnectionBuilder().withUrl('/realtime').build();`
  - Use `connection.invoke('Register', username, name)` or `connection.send('SendMessage', from, to, text)` as needed.

Development and testing
- The `IChatRepository` abstraction is used to swap backing stores. For tests, implement a simple in-memory repository that tracks users and messages.
- Ensure hub method signatures remain compatible with the client code. Hub method names are case-insensitive when invoked from JS.

Common tasks
- Add features by updating hub methods (`Hubs/RealtimeHub.cs`) and repository interfaces in `Data/`.
- When changing models, ensure both server and client serializers handle property changes. The project uses default System.Text.Json.

Troubleshooting
- If messages aren't delivered, check the connection id mapping in `IChatRepository` implementation and ensure users are added to groups using `Groups.AddToGroupAsync`.
- For startup issues, inspect console logs for binding URLs and errors.

Submitting changes
- Create a branch, make small focused commits, and open a pull request to `master`.
- Include unit tests where practical and describe manual steps to verify SignalR interactions.

Contact
- For questions about the code layout, inspect `Hubs/RealtimeHub.cs` and `Data/` implementations. If you'd like, add more detailed docs to this guide.