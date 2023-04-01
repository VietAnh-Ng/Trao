using System;
using System.Net.WebSockets;
using TraoApp.Shared;

namespace TraoApp.Server.Services;

public class SensorService : BackgroundService
{
    private readonly List<WebSocket> DeviceClientSockets = new();
    private readonly List<WebSocket> ViewerClientSockets = new();
    public readonly ControlOption ControlOption = new() { RotationDirection = true, MotorSpeed = 1000, };
    private readonly Queue<byte[]> SendQueue = new();

    public async Task HandlerAsync(WebSocket webSocket)
    {
        try
        {
            byte[] bufSend = new byte[] { 0xF0, 0x00, 0x00, 0x00, 0x00, 0xAA };
            List<byte> bufferSend = new();
            var buffer = new byte[10];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (!receiveResult.CloseStatus.HasValue
                && receiveResult.MessageType == WebSocketMessageType.Binary
                && receiveResult.Count == 3
                && buffer[0] == 0xF0 && buffer[2] == 0xAA)
            {
                if (buffer[1] == 0x01)
                {
                    DeviceClientSockets.Add(webSocket);
                    Console.WriteLine($"DeviceClientSocket connected");
                    try
                    {
                        while (!receiveResult.CloseStatus.HasValue)
                        {
                            if (receiveResult.MessageType == WebSocketMessageType.Binary
                                && receiveResult.Count == 6
                                && buffer[0] == 0xF0 && buffer[5] == 0xAA)
                            {
                                bufSend[1] = buffer[1];
                                bufSend[2] = buffer[2];
                                bufSend[3] = buffer[3];
                                bufSend[4] = buffer[4];

                                SendQueue.Enqueue((byte[])bufSend.Clone());
                            }

                            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }
                    }
                    catch { }
                    Console.WriteLine($"DeviceClientSocket Disconnect");
                    DeviceClientSockets.Remove(webSocket);
                }
                else if (buffer[1] == 0x02)
                {
                    ViewerClientSockets.Add(webSocket);
                    Console.WriteLine($"ViewerClientSocket connected");
                    try
                    {
                        while (!receiveResult.CloseStatus.HasValue)
                        {
                            // to do
                            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }
                    }
                    catch { }
                    Console.WriteLine($"ViewerClientSocket Disconnect");
                    ViewerClientSockets.Remove(webSocket);
                }
            }


            await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, receiveResult.CloseStatusDescription, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task SetRotationDirection(bool direction)
    {
        var buffer = new byte[] { 0xF0, 0x01, 0x00, 0xA0, };
        buffer[2] = (byte)(direction ? 1 : 0);
        ControlOption.RotationDirection = direction;
        Queue<WebSocket> clients = new(DeviceClientSockets);
        while (clients.TryDequeue(out WebSocket? webSocket) && webSocket is not null)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
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
        Queue<WebSocket> clients = new(DeviceClientSockets);
        while (clients.TryDequeue(out WebSocket? webSocket) && webSocket is not null)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task circleTask = Task.Delay(1000, stoppingToken);
        await circleTask;

        while (!stoppingToken.IsCancellationRequested)
        {
            circleTask = Task.Delay(5, CancellationToken.None);
            while (SendQueue.TryDequeue(out byte[]? data) && ViewerClientSockets.Any())
            {
                await ViewerClientSockets.Last().SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            await circleTask;
        }
    }
}
