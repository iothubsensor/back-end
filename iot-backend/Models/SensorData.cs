namespace iot_backend.Models;

public partial class SensorData
{
    public int SensorDataId { get; set; }
    
    public int Data { get; set; }
    public DateTime Date { get; set; }
    
    public int SensorId { get; set; }
    public virtual Sensor Sensor { get; set; }
}