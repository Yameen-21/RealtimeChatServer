using System.Net.WebSockets;
using RealtimeChatServer.Services;
using RealtimeChatServer.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register Services (Dependency Injection)
builder.Services.AddSingleton<DialogflowService>();
builder.Services.AddSingleton<ChatWebSocketHandler>();

var app = builder.Build();

// ✅ Enable WebSockets
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);

// ✅ WebSocket Endpoint
app.Map("/chat", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var handler = context.RequestServices.GetRequiredService<ChatWebSocketHandler>();

        await handler.HandleAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();
