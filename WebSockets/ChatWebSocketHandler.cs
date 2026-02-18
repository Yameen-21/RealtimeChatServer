using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using RealtimeChatServer.Models;
using RealtimeChatServer.Services;

namespace RealtimeChatServer.WebSockets
{
    public class ChatWebSocketHandler
    {
        private readonly DialogflowService _dialogflowService;

        public ChatWebSocketHandler(DialogflowService dialogflowService)
        {
            _dialogflowService = dialogflowService;
        }

        public async Task HandleAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                // Receive message from client
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Close)
                    return;

                // Convert byte stream → string
                var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Deserialize incoming JSON payload
                var chatMessage = JsonSerializer.Deserialize<ChatMessage>(
                    messageJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // Ignore invalid or empty messages
                if (chatMessage == null || string.IsNullOrWhiteSpace(chatMessage.Message))
                    continue;

                Console.WriteLine($"User: {chatMessage.Message}");

                // Send message to Dialogflow NLP engine
                var botReply = await _dialogflowService
                    .GetResponseAsync(chatMessage.Message);

                Console.WriteLine($"Bot: {botReply}");

                // Serialize bot reply → JSON
                var responseJson = JsonSerializer.Serialize(new ChatMessage
                {
                    Message = botReply
                });

                var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                // Send response back via WebSocket
                await webSocket.SendAsync(
                    new ArraySegment<byte>(responseBytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
    }
}
