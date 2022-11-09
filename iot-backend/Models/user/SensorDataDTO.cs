namespace iot_backend.Models.user;

public class SensorDataDTO
{
    public SensorDataDTO(SensorData sensorData)
    {
        SensorDataId = sensorData.SensorDataId;
        Data = sensorData.Data;

        Date = new DateTimeOffset(sensorData.Date).ToUnixTimeMilliseconds();
    }
    
    public int SensorDataId { get; set; }
    public double Data { get; set; }
    public long Date { get; set; }
}