using Bank.Application.Features.Accounts.OpenAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController(ISender sender) : ControllerBase
{
    public sealed record OpenAccountRequest(string OwnerName, string Currency);

    [HttpPost]
    public async Task<IActionResult> Open(OpenAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await sender.Send(new OpenAccountCommand(request.OwnerName, request.Currency), cancellationToken);
            return Created($"/api/accounts/{id}", new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
