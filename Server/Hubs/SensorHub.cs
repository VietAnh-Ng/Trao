using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MudBlazor;
using TraoApp.Server.Services;
using TraoApp.Shared;

namespace TraoApp.Server.Hubs;

public class SensorHub : Hub<ISensorHubClient>
{
    public ControlOption GetControlOption([FromServices] SensorService sensorService)
        => new()
        {
            RotationDirection = sensorService.ControlOption.RotationDirection,
            SampleRate = sensorService.ControlOption.SampleRate,
            MotorSpeed = sensorService.ControlOption.MotorSpeed,
            SignalsToPlot = sensorService.ControlOption.SignalsToPlot,
        };

    public async Task<bool> SetRotationDirection(bool direction, [FromServices] SensorService sensorService)
    {
        await sensorService.SetRotationDirection(direction);
        return sensorService.ControlOption.RotationDirection;
    }

    public async Task<int> SetSampleRate(int sampleRate, [FromServices] SensorService sensorService)
    {
        await sensorService.SetSampleRate(sampleRate);
        return sensorService.ControlOption.SampleRate;
    }

    public async Task<bool> SetSignalsToPlot(bool signalsToPlot, [FromServices] SensorService sensorService)
    {
        await sensorService.SetSignalsToPlot(signalsToPlot);
        return sensorService.ControlOption.SignalsToPlot;
    }

    public async Task<int> SetMotorSpeed(int motorSpeed, [FromServices] SensorService sensorService)
    {
        await sensorService.SetMotorSpeed(motorSpeed);
        return sensorService.ControlOption.MotorSpeed;
    }
}
