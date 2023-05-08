using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TraoApp.Server.Services;
using TraoApp.Shared;
using static MudBlazor.CategoryTypes;

namespace TraoApp.Server.Hubs;

public class SensorHub : Hub
{
    public ControlOption GetControlOption([FromServices] SensorService sensorService)
        => new()
        {
            RotationDirection = sensorService.ControlOption.RotationDirection,
            MotorSpeed = sensorService.ControlOption.MotorSpeed,
        };

    public async Task<bool> SetRotationDirection(bool direction, [FromServices] SensorService sensorService)
    {
        await sensorService.SetRotationDirection(direction);
        return sensorService.ControlOption.RotationDirection;
    }

    public Task SetRunContinuously(bool enable, [FromServices] SensorService sensorService) => sensorService.SetRunContinuously(enable);

    public Task WriteInductorPin(bool val, [FromServices] SensorService sensorService) => sensorService.WriteInductorPin(val);


    public async Task<int> SetMotorSpeed(int motorSpeed, [FromServices] SensorService sensorService)
    {
        await sensorService.SetMotorSpeed(motorSpeed);
        return sensorService.ControlOption.MotorSpeed;
    }

}
