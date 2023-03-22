using Microsoft.AspNetCore.SignalR.Client;

namespace TraoApp.Client.Clients;

public class SensorHubClient
{
    public event Action<long, float>? OnSensorDataReceived;

    private HubConnection Connection;

    public SensorHubClient()
    {
        Connection = new HubConnectionBuilder().WithUrl("/hub/sensor").Build();

        Connection.On<long, float>("SensorDataReceived", (time, value) => OnSensorDataReceived?.Invoke(time, value));
    }

    public async Task Connect()
    {
        if(Connection.State == HubConnectionState.Disconnected)
        {
            await Connection.StartAsync();
        }
    }
}
