using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SmallEco.DTO;

namespace SmallEco.Server.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
  //# resource
  static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

  //# for injection
  ILogger<IdentityController> _logger;
  IConfiguration _config;

  public IdentityController(ILogger<IdentityController> logger, IConfiguration config)
  {
    _logger = logger;
    _config = config;
  }

  /// <seealso cref="https://github.com/doggy8088/AspNetCore6JwtAuthDemo/blob/main/JwtHelpers.cs"/>
  [HttpPost("[action]")]
  public IActionResult GenerateToken([FromBody] TokenGenerationRequest request)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"]);

      var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Sub, request.Email),
      new(JwtRegisteredClaimNames.Email, request.Email),
      new("userid", request.UserId.ToString())
    };

      if (request.CustomClaims != null)
      {
        foreach (var claimPair in request.CustomClaims)
        {
          //var jsonElement = claimPair.Value;
          //var valueType = jsonElement.ValueKind switch
          //{
          //  JsonValueKind.True => ClaimValueTypes.Boolean,
          //  JsonValueKind.False => ClaimValueTypes.Boolean,
          //  JsonValueKind.Number => ClaimValueTypes.Double,
          //  _ => ClaimValueTypes.String
          //};

          var claim = new Claim(claimPair.Key, claimPair.Value, ClaimValueTypes.String);
          claims.Add(claim);
        }
      }

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.Add(TokenLifetime),
        Issuer = _config["JwtSettings:Issuer"],
        Audience = _config["JwtSettings:Audience"], // SigningKey
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"])), SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      var jwt = tokenHandler.WriteToken(token);

      return Ok(jwt);
    }
    catch (Exception ex)
    {
      return BadRequest(new ErrMsg(ex.Message));
    }
  }
}
