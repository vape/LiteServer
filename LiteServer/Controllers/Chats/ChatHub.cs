using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteServer.Controllers.Chats
{
    public enum NotificationType : int
    {
        NewMessage = 1,
        UserConnected,
        UserDisconnected
    }

    public static class SocketExtensions
    {
        public static bool CanBeClosed (this WebSocket socket)
        {
            return socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived || socket.State == WebSocketState.CloseSent;
        }
    }

    public class ChatHub
    {
        public int ConnectionsCount
        { get { return connections.Count; } }

        private ConcurrentDictionary<Guid, WebSocket> connections = new ConcurrentDictionary<Guid, WebSocket>();

        private readonly uint groupId;
        private readonly ILogger logger;

        public ChatHub(uint groupId, ILogger logger)
        {
            this.groupId = groupId;
            this.logger = logger;
        }

        public async Task Listen(Guid user, CancellationToken cancellationToken)
        {
            var socket = connections[user];
            logger.LogInformation($"Created socket connection with user {user}");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation($"Chat connection for user {user} cancelled");
                    break;
                }

                byte[] response = null;
                try
                {
                    response = await RecieveAsync(socket, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation($"Chat connection for user {user} cancelled");
                    break;
                }
                catch (WebSocketException e)
                {
                    logger.LogInformation($"Chat connection for user {user} failed with socket exception {e.ToString()}");
                    break;
                }
                
                if (response.Length == 0)
                {
                    if (socket.State != WebSocketState.Open)
                    {
                        logger.LogInformation($"Chat connection for user {user} no longer open ({socket.State})");
                        break;
                    }

                    continue;
                }
            }

            WebSocket _;
            connections.Remove(user, out _);

            if (socket.CanBeClosed())
            {
                logger.LogInformation($"Closing chat connection for user {user} manually");
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "None", cancellationToken);
            }
            
            socket.Dispose();
        }

        public bool Attach(Guid user, WebSocket socket)
        {
            return connections.TryAdd(user, socket);
        }

        public async Task Notify(NotificationType type, CancellationToken cancellationToken)
        {
            foreach (var connection in connections.Values)
            {
                await TrySendAsync(connection, Format(type), onError: (e) => {
                    logger.LogError($"Failed to send notification: {e.Message}");
                });
            }
        }

        private byte[] Format(NotificationType type)
        {
            var data = BitConverter.GetBytes((int)type);
            return data;
        }

        private async Task SendAsync(WebSocket socket, string data, CancellationToken cancellationToken = default(CancellationToken))
        {
            var binaryData = Encoding.UTF8.GetBytes(data);
            await socket.SendAsync(binaryData, WebSocketMessageType.Text, true, cancellationToken);
        }

        private async Task SendAsync(WebSocket socket, byte[] data, CancellationToken cancellationToken = default(CancellationToken))
        {
            await socket.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken);
        }

        private async Task TrySendAsync(WebSocket socket, string data, CancellationToken cancellationToken = default(CancellationToken), Action<Exception> onError = null)
        {
            try
            {
                var binaryData = Encoding.UTF8.GetBytes(data);
                await socket.SendAsync(binaryData, WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

        private async Task TrySendAsync(WebSocket socket, byte[] data, CancellationToken cancellationToken = default(CancellationToken), Action<Exception> onError = null)
        {
            try
            {
                await socket.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken);
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

        private async Task<byte[]> RecieveAsync(WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, cancellationToken);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }
    }
}
