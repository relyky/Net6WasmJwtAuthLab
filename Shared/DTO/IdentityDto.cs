namespace SmallEco.DTO;

public class TokenGenerationRequest
{
  public Guid UserId { get; set; }
  public string Email { get; set; } = string.Empty;
  public string CustomClaims { get; set; } = string.Empty;
}
