using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<AccountResponse>> GetAll()
    {
        var accounts = new List<AccountResponse>
        {
            new() { Id = 1, Name = "Gmail", Username = "user@gmail.com", Password = "password123" },
            new() { Id = 2, Name = "Facebook", Username = "myuser", Password = "secret456" }
        };
        
        return Ok(accounts);
    }
}

public class AccountResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}