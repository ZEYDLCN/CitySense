using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using CitySense.Shared.Dtos;

namespace CitySense.Frontend.Services
{
    public class SignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public event Action<SensorDataPointDto>? OnSensorDataReceived;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public SignalRService(string hubUrl)
        {
            _hubUrl = hubUrl ?? throw new ArgumentNullException(nameof(hubUrl));
        }

        public async Task StartConnectionAsync()
        {
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30)
                })
                .Build();

            _hubConnection.On<SensorDataPointDto>("ReceiveSensorDataUpdate", dataPoint =>
            {
                OnSensorDataReceived?.Invoke(dataPoint);
            });

            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine($"SignalR disconnected: {error?.Message}");
            };

            _hubConnection.Reconnecting += (error) =>
            {
                Console.WriteLine($"SignalR reconnecting: {error?.Message}");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"SignalR reconnected: {connectionId}");
                return Task.CompletedTask;
            };

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR connection established.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection error: {ex}");
            }
        }

        public async Task StopConnectionAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
                Console.WriteLine("SignalR connection stopped.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                Console.WriteLine("SignalR disposed.");
            }
        }
    }
}
