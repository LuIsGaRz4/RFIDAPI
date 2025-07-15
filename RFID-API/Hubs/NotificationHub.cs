using Microsoft.AspNetCore.SignalR;

namespace RFID_API.Hubs
{
    public class NotificationHub : Hub
    {
        // Método que puede llamar el cliente para enviar un mensaje al servidor,
        // y que este lo reenvía a todos los clientes conectados
        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        // Método para enviar un mensaje solo al cliente actual (quien llamó)
        public async Task SendMessageToCaller(string message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        // Método para enviar mensaje a un usuario específico (requiere autenticación y UserId configurado)
        public async Task SendMessageToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveMessage", message);
        }
    }
}
