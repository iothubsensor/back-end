using Microsoft.Build.Framework;

namespace iot_backend.Models.sensor;

public class SensorModel
{
    [Required]
    public string name { get; set; }
    
    [Required]
    public string description { get; set; }
}