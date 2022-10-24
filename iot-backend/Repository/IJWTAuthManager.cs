using iot_backend.Models;
using iot_backend.Models.api;

namespace iot_backend.Repository;

public interface IJWTAuthManager
{
    Response<string> GenerateJWT(User user);
}