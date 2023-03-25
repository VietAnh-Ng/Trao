namespace TraoApp.Shared;

public interface ISensorHubClient
{
    Task SensorDataReceived(uint time, float value);
}
