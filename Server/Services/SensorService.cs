using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;
using TraoApp.Server.Hubs;
using TraoApp.Shared;

namespace TraoApp.Server.Services;

public class SensorService : BackgroundService
{
    private readonly IHubContext<SensorHub, ISensorHubClient> SensorHubContext;
    private readonly AutoResetEvent DataEvent = new(false);
    private readonly List<WebSocket> ClientSockets = new();

    public record SensorData(uint Time, float Value);

    public readonly Queue<SensorData> SensorDatas = new();
    public readonly ControlOption ControlOption = new() { RotationDirection = true, SampleRate = 50, MotorSpeed = 200, SignalsToPlot = true, };

    public SensorService(IHubContext<SensorHub, ISensorHubClient> sensorHub)
    {
        SensorHubContext = sensorHub;
    }

    public async Task HandlerAsync(WebSocket webSocket)
    {
        Console.WriteLine($"Socket client connected {webSocket.SubProtocol}");
        ClientSockets.Add(webSocket);
        try
        {
            var buffer = new byte[1024];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!receiveResult.CloseStatus.HasValue)
            {
                Console.WriteLine($"Raw: {BitConverter.ToString(buffer)}");
                if (buffer[0] == 0xF0 && buffer[9] == 0xAA)
                {
                    SensorDatas.Enqueue(new(BitConverter.ToUInt32(buffer, 1), BitConverter.ToSingle(buffer, 5)));
                    DataEvent.Set();
                    Console.WriteLine($"Received {(int)buffer[5]}");
                }
                //if (receiveResult.MessageType == WebSocketMessageType.) { }

                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        ClientSockets.Remove(webSocket);
    }

    public async Task SetRotationDirection(bool direction)
    {
        var buffer = new byte[] { 0xF0, 0x01, 0x00, 0xA0, };
        buffer[2] = (byte)(direction ? 1 : 0);
        ControlOption.RotationDirection = direction;
        Queue<WebSocket> clients = new(ClientSockets);
        while (clients.TryDequeue(out WebSocket? webSocket) && webSocket is not null)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }

    public async Task SetSampleRate(int sampleRate)
    {
        var buffer = new byte[] { 0xF0, 0x02, 0x00, 0x00, 0x00, 0x00, 0xA0, };
        buffer[2] = (byte)(sampleRate);
        buffer[3] = (byte)(sampleRate >> 8);
        buffer[4] = (byte)(sampleRate >> 16);
        buffer[5] = (byte)(sampleRate >> 24);
        ControlOption.SampleRate = sampleRate;
        Queue<WebSocket> clients = new(ClientSockets);
        while (clients.TryDequeue(out WebSocket? webSocket) && webSocket is not null)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }

    public async Task SetSignalsToPlot(bool signalsToPlot)
    {
        var buffer = new byte[] { 0xF0, 0x03, 0x00, 0xA0, };
        buffer[2] = (byte)(signalsToPlot ? 1 : 0);
        ControlOption.SignalsToPlot = signalsToPlot;
        Queue<WebSocket> clients = new(ClientSockets);
        while (clients.TryDequeue(out WebSocket? webSocket) && webSocket is not null)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }

    public async Task SetMotorSpeed(int motorSpeed)
    {
        var buffer = new byte[] { 0xF0, 0x04, 0x00, 0x00, 0x00, 0x00, 0xA0, };
        buffer[2] = (byte)(motorSpeed);
        buffer[3] = (byte)(motorSpeed >> 8);
        buffer[4] = (byte)(motorSpeed >> 16);
        buffer[5] = (byte)(motorSpeed >> 24);
        ControlOption.MotorSpeed = motorSpeed;
        Queue<WebSocket> clients = new(ClientSockets);
        while (clients.TryDequeue(out WebSocket? webSocket) && webSocket is not null)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            while (SensorDatas.TryDequeue(out var sensorData))
            {
                await SensorHubContext.Clients.All.SensorDataReceived(sensorData.Time, sensorData.Value);
            }

            if (WaitHandle.WaitAny(new[] { DataEvent, stoppingToken.WaitHandle }) != 0) continue;
        }
    }
}
