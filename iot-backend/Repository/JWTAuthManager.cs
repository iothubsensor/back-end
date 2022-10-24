using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iot_backend.Models;
using iot_backend.Models.api;
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
    
    public Response<string> GenerateJWT(User user)
    {
        Response<string> response = new Response<string>();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtAuth:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[] {
            new Claim("user_id", user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("roles", user.Role.ToString().ToLower()),
            new Claim("Date", DateTime.Now.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(_configuration["JwtAuth:Issuer"],
          _configuration["JwtAuth:Issuer"],
          claims,
          expires: DateTime.Now.AddMinutes(120),
          signingCredentials: credentials);

        response.Data = new JwtSecurityTokenHandler().WriteToken(token);
        response.code = 200;
        response.message = "Token generated";
        
        return response;
    }
}