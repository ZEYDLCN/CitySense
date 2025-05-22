using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace CitySense.Api.Hubs
{
    public class SensorDataHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }
    }
}
