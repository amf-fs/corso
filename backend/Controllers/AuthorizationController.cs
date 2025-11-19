using System.ComponentModel.DataAnnotations;
using System.Text;
using CorsoApi.Infrastructure;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationController(ISecretStore secretStore) : ControllerBase
{
    private readonly ISecretStore secretStore = secretStore;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AuthorizationRequest request)
    {
        var storedHash = await secretStore.GetSecretAsync("masterHash");

        if (storedHash is null)
        {
            //TODO: Add logs;
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var salt = await secretStore.GetSecretAsync("salt");

        if (salt is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        //TODO: Common service
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(request.MasterPassword));
        argon2.Salt = Encoding.UTF8.GetBytes(salt);
        argon2.DegreeOfParallelism = Environment.ProcessorCount;
        argon2.Iterations = 4;
        argon2.MemorySize = 65536; //64MB
        var hashOfRequest = Convert.ToBase64String(await argon2.GetBytesAsync(32));
        
        if (hashOfRequest != storedHash)
        {
            return Unauthorized();
        }

        //Set session
        HttpContext.Session.Clear();
        HttpContext.Session.SetString("IsAuthorized", "true");

        return Ok();
    }
}

public record AuthorizationRequest
{
    [Required]
    [StringLength(32, MinimumLength = 8)]
    public required string MasterPassword { get; set; }
};