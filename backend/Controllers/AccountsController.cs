using System.Threading.Tasks;
using CorsoApi.Core;
using CorsoApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IAccountsRepository repository) : ControllerBase
{
    private readonly IAccountsRepository repository = repository;

    [HttpGet]
    public async Task<ActionResult<List<AccountResponse>>> GetAll()
    {
        var accounts = await repository.GetAllAsync();

        var response = accounts.Select(account => new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Username = account.Username,
            Password = account.Password
        });

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> Post(AccountRequest request)
    {
        var account = new Account
        {
            Name = request.Name,
            Username = request.Username,
            Password = request.Password
        };

        await repository.AddAsync(account);

        var response = new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Password = account.Password,
            Username = account.Username
        };

        return Ok(response);
    }
}

public class AccountRequest
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AccountResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}