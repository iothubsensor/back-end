namespace iot_backend.Models.api;

public class Response<T>
{
    public T data { get; set; }
    public string message { get; set; }
    public int  code { get; set; }
}