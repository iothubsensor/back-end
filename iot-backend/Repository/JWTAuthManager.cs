using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iot_backend.Models;
using iot_backend.Models.api;
using iot_backend.Models.user;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace iot_backend.Repository;

public class JWTAuthManager : IJWTAuthManager
{
    private readonly IConfiguration _configuration;

    public JWTAuthManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public Response<object> GenerateJWT(User user)
    {
        Response<object> response = new Response<object>();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtAuth:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[] {
            new Claim("user_id", user.UserId.ToString()),
            new Claim("roles", user.Role.ToString().ToLower()),
            new Claim("Date", DateTime.Now.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signedToken = new JwtSecurityToken(_configuration["JwtAuth:Issuer"],
          _configuration["JwtAuth:Issuer"],
          claims,
          expires: DateTime.Now.AddMinutes(120),
          signingCredentials: credentials);

        
        response.data = new {token = new JwtSecurityTokenHandler().WriteToken(signedToken), expiresIn = DateTimeOffset.Now.AddMinutes(120).ToUnixTimeMilliseconds(), user = new UserDTO(user)};
        response.code = 200;
        response.message = "Token generated";
        
        return response;
    }
}