using iot_backend;
using iot_backend.Models;
using iot_backend.Utils;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => 
    {
        webBuilder.UseStartup<Startup>();
    });
    
}