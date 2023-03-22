namespace TraoApp.Shared;

public interface ISensorHubClient
{
    Task SensorDataReceived(long time, float value);
}
