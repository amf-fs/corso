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
        (
            account.Id,
            account.Name,
            account.Username,
            account.Password
        ));

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
        (
            account.Id,
            account.Name,
            account.Username,
            account.Password
        );

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, AccountRequest newValues)
    {
        if (!await repository.ExistsAsync(id))
        {
            return NotFound();
        }

        var account = new Account()
        {
            Id = id,
            Name = newValues.Name,
            Username = newValues.Username,
            Password = newValues.Password
        };

        await repository.UpdateAsync(account);
        return NoContent();
    }
}

public record AccountRequest(string Name, string Username, string Password);
public record AccountResponse(int Id, string Name, string Username, string Password);