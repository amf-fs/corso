using System.ComponentModel.DataAnnotations;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationsController(ISecretStore secretStore, IHasher hasher) : ControllerBase
{
    private readonly ISecretStore secretStore = secretStore;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AuthorizationRequest request)
    {
        var storedHash = await secretStore.GetRequiredSecretAsync("masterHash");
        var salt = await secretStore.GetRequiredSecretAsync("salt");
        var hashOfRequest = hasher.Create(request.MasterPassword, salt);
        
        if (hashOfRequest != storedHash)
        {
            return Unauthorized();
        }

        HttpContext.CreateAuthenticationSession();
        return Ok();
    }
}

public record AuthorizationRequest
{
    [Required]
    [StringLength(32, MinimumLength = 8)]
    public required string MasterPassword { get; set; }
};