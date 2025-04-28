using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Service.Services;
using System.Security.Claims;

namespace Roomify.GP.API.Hubs
{

    [Authorize(Roles = "NormalUser,InteriorDesigner")]
    public class PrivateChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public PrivateChatHub(IMessageService messageService , IServiceScopeFactory serviceScopeFactory)
        {
            _messageService = messageService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task SendMessage(ChatModel chatModel)
        {
            if (chatModel == null || string.IsNullOrWhiteSpace(chatModel.Message))
            {
                throw new HubException("Invalid message content.");
            }

            // ✅ خليه يعتمد على الـ SenderId اللي في التوكن نفسه
            var userIdFromToken = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdFromToken, out Guid parsedSenderId))
            {
                throw new HubException("Invalid sender token.");
            }

            chatModel.SenderId = parsedSenderId;

            if (chatModel.ReceiverId == Guid.Empty)
            {
                throw new HubException("Receiver ID cannot be empty.");
            }

            await _messageService.SaveMessage(chatModel);

            await Clients.User(chatModel.ReceiverId.ToString())
                         .SendAsync("ReceiveMessage", chatModel.Message);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var connectionId = Context.ConnectionId;

            Console.WriteLine("🟢 Hub Connected - UserId: " + userId);

            if (!string.IsNullOrEmpty(userId))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.UserConnections.Add(new UserConnection
                {
                    UserId = Guid.Parse(userId),
                    ConnectionId = connectionId,
                    ConnectedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var connection = await db.UserConnections.FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
            if (connection != null)
            {
                db.UserConnections.Remove(connection);
                await db.SaveChangesAsync();
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
