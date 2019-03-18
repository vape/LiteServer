using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace LiteServer.Controllers.Chats
{
    public interface IChatManager
    {
        Task NotifyNewMessageAsync(uint groupId);
        Task NotifyConnectedAsync(uint groupId, Guid user);

        Task Listen(uint groupId, Guid user, WebSocket socket, CancellationToken cancellationToken);
        int GetConnectUsersCount(uint groupId);
    }

    public class ChatManager : IChatManager
    {
        private static ConcurrentDictionary<uint, ChatHub> hubs = new ConcurrentDictionary<uint, ChatHub>();

        private readonly ILogger logger;

        public ChatManager(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task NotifyDisconnectedAsync(uint groupId, Guid user)
        {
            ChatHub hub = hubs.GetOrAdd(groupId, new ChatHub(groupId, logger));
            await hub.Notify(NotificationType.UserDisconnected, new CancellationToken());
        }

        public async Task NotifyConnectedAsync(uint groupId, Guid user)
        {
            ChatHub hub = hubs.GetOrAdd(groupId, new ChatHub(groupId, logger));
            await hub.Notify(NotificationType.UserConnected, new CancellationToken());
        }

        public async Task NotifyNewMessageAsync(uint groupId)
        {
            ChatHub hub = hubs.GetOrAdd(groupId, new ChatHub(groupId, logger));
            await hub.Notify(NotificationType.NewMessage, new CancellationToken());
        }

        public async Task Listen(uint groupId, Guid userUuid, WebSocket socket, CancellationToken cancellationToken)
        {
            var hub = hubs.GetOrAdd(groupId, new ChatHub(groupId, logger));
            hub.Attach(userUuid, socket);
            
            await hub.Notify(NotificationType.UserConnected, cancellationToken);
            await hub.Listen(userUuid, cancellationToken);
            await hub.Notify(NotificationType.UserDisconnected, cancellationToken);
        }

        public int GetConnectUsersCount(uint groupId)
        {
            var hub = hubs.GetOrAdd(groupId, new ChatHub(groupId, logger));
            return hub.ConnectionsCount;
        }
    }
}
