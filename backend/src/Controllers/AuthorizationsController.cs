using System.ComponentModel.DataAnnotations;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CorsoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationsController(IConfiguration configuration, IHasher hasher, ILogger<AuthorizationsController> logger) : ControllerBase
{
    private readonly IConfiguration configuration = configuration;
    private readonly ILogger<AuthorizationsController> _logger = logger;

    [HttpPost]
    public IActionResult Post([FromBody] AuthorizationRequest request)
    {
        var storedHash = configuration["masterHash"]!;
        var salt = configuration["salt"]!;
        var hashOfRequest = hasher.Create(request.MasterPassword, salt);

        // Debug-level logs will only be emitted if the configured minimum level allows it
        _logger.LogDebug("Master password: {masterPassword}", request.MasterPassword);
        _logger.LogDebug("Hash of request: {hashOfRequest}", hashOfRequest);

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