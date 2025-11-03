using System.Diagnostics;
using CorsoApi.Core;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Controllers;

//TODO: secure the controller with authentication
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountsVault vault;

    public AccountsController(IAccountsVault vault)
    {
        this.vault = vault;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountResponse>>> GetAll()
    {
        await vault.UnLockAsync();
        var accounts = vault.GetAll();
        await vault.LockAsync();

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

        await vault.UnLockAsync();
        vault.Add(account);
        await vault.LockAsync();

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
        await vault.UnLockAsync();
        if (!vault.Exists(id))
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

        vault.Update(account);
        await vault.LockAsync();
        return NoContent();
    }
}

public record AccountRequest(string Name, string Username, string Password);
public record AccountResponse(int Id, string Name, string Username, string Password);