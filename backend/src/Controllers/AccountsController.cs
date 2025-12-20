using System.Diagnostics;
using CorsoApi.Core;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CorsoApi.Controllers;

//TODO: secure the controller with authentication
[ApiController]
[Route("api/[controller]")]
[SessionAuthorize]
public class AccountsController(IAccountsVault vault, CsvParser csvParser) : ControllerBase
{
    private readonly IAccountsVault vault = vault;
    private readonly CsvParser csvParser = csvParser;

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

    [HttpPost("import")]
    public async Task<ActionResult> Import(IFormFile file)
    {  
        if(file is null || file.Length == 0)
        {
            return this.BadRequestProblem("File", $"The file {file?.FileName} was not imported because is empty.");
        }

        using var stream =  file.OpenReadStream();
        var validation = await csvParser.ValidateAsync<Account>(stream);

        if(validation.Error is not null)
        {
            return this.BadRequestProblem("File", validation.Error.Message);
        }

        var accounts = await csvParser.ParseAsync<Account>(stream);
        await vault.UnLockAsync();
        
        foreach(var account in accounts)
        {
            vault.Add(account);
        }

        await vault.LockAsync();
        return Ok();
    }
}

public record AccountRequest(string Name, string Username, string Password);
public record AccountResponse(int Id, string Name, string Username, string Password);