using System.Text.Json;

namespace SmallEco.DTO;

public class TokenGenerationRequest
{
  public Guid UserId { get; set; }
  public string Email { get; set; } = string.Empty;
  public Dictionary<string,string>? CustomClaims { get; set; }
}
