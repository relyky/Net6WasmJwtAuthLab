using Refit;
using SmallEco.DTO;

namespace SmallEco.Client.RefitClient;

public interface IIdentityApi
{
  [Post("/api/Identity/GenerateToken")]
  Task<String> GenerateTokenAsync(TokenGenerationRequest request);
}
