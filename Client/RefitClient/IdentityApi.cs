using Refit;
using SmallEco.DTO;

namespace SmallEco.Client.RefitClient;

public interface IdentityApi
{
  [Post("/api/Identity/GenerateToken")]
  Task<String> GenerateTokenAsync(TokenGenerationRequest request);
}
