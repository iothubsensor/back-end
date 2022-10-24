using Microsoft.Build.Framework;

namespace iot_backend.Models.login;

public class SetupModel
{
    [Required]
    public string phone_ext { get; set; }
    
    [Required]
    public string phone_number { get; set; }
    
    [Required]
    public string address { get; set; }
    
    [Required]
    public string job_description { get; set; }
}