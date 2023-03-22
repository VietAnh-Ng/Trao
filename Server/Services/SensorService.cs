using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;
using TraoApp.Server.Hubs;
using TraoApp.Shared;

namespace TraoApp.Server.Services;

public class SensorService : BackgroundService 
{
    private readonly IHubContext<SensorHub, ISensorHubClient> SensorHubContext;
    private readonly AutoResetEvent DataEvent = new(false);
    public float SenSorValue { get; private set; } = 0;

    public SensorService(IHubContext<SensorHub, ISensorHubClient> sensorHub)
    {
        SensorHubContext = sensorHub;
        SenSorValue = 0;
    }

    public async Task HandlerAsync(WebSocket webSocket)
    {
        var buffer = new byte[6];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!receiveResult.CloseStatus.HasValue)
        {
            // To-do
            if (buffer[0] == 0xF0 && buffer[5] == 0xAA)
            {
                SenSorValue = BitConverter.ToSingle(buffer, 1);
                DataEvent.Set();
            }
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var task = Task.Delay(50);

            if (WaitHandle.WaitAny(new[] { DataEvent, stoppingToken.WaitHandle}, 50) != 0)
            {
                SenSorValue = 0;
            }

            await Task.WhenAll(SensorHubContext.Clients.All.SensorDataReceived(DateTime.Now.Ticks, SenSorValue), task);
        }
    }
}
