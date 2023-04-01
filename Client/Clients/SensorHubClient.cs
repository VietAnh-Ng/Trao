using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TraoApp.Shared;

namespace TraoApp.Client.Clients;

public class SensorHubClient
{
    private readonly HubConnection Connection;

    public SensorHubClient(NavigationManager nav)
    {
        Connection = new HubConnectionBuilder().WithUrl(nav.ToAbsoluteUri("/hub/sensor")).Build();
    }

    public async Task Connect()
    {
        if (Connection.State == HubConnectionState.Disconnected)
        {
            await Connection.StartAsync();
        }
    }

    public async Task Disconnect()
    {
        if (Connection.State != HubConnectionState.Disconnected)
        {
            await Connection.StopAsync();
        }
    }

    public async Task<ControlOption> GetControlOption() => await Connection.InvokeAsync<ControlOption>("GetControlOption");
    public async Task<bool> SetRotationDirection(bool direction) => await Connection.InvokeAsync<bool>("SetRotationDirection", direction);
    public async Task<int> SetSampleRate(int sampleRate) => await Connection.InvokeAsync<int>("SetSampleRate", sampleRate);
    public async Task<bool> SetSignalsToPlot(bool signalsToPlot) => await Connection.InvokeAsync<bool>("SetSignalsToPlot", signalsToPlot);
    public async Task<int> SetMotorSpeed(int motorSpeed) => await Connection.InvokeAsync<int>("SetMotorSpeed", motorSpeed);

    public async Task<bool> SetRotationDirectionPR() => await Connection.InvokeAsync<bool>("SetRotationDirection", true);
    public async Task<bool> SetRotationDirectionFW() => await Connection.InvokeAsync<bool>("SetRotationDirection", false);

    public Task StartReadingSensor() => Connection.InvokeAsync("StartReadingSensor");
    public Task StopReadingSensor() => Connection.InvokeAsync("StopReadingSensor");
}
