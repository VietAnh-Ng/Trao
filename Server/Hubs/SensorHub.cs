using Microsoft.AspNetCore.SignalR;
using TraoApp.Shared;

namespace TraoApp.Server.Hubs;

public class SensorHub : Hub<ISensorHubClient>
{
}
