using System.ComponentModel.DataAnnotations;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationsController(IConfiguration configuration, IHasher hasher) : ControllerBase
{
    private readonly IConfiguration configuration = configuration;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AuthorizationRequest request)
    {
        var storedHash = configuration["masterHash"]!;
        var salt = configuration["salt"]!;
        var hashOfRequest = hasher.Create(request.MasterPassword, salt);

        Console.WriteLine("Authorization Attempt:");
        Console.WriteLine($"Stored Hash: {storedHash}");
        Console.WriteLine($"Hash of Request: {hashOfRequest}");
        Console.WriteLine($"Salt: {salt}");
        
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