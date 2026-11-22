using Bank.Application.Abstractions;
using Bank.Application.Features.Accounts.Deposit;
using Bank.Application.Features.Accounts.GetAccount;
using Bank.Application.Features.Accounts.OpenAccount;
using Bank.Application.Features.Accounts.Withdraw;
using Bank.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController(ISender sender) : ControllerBase
{
    public sealed record OpenAccountRequest(string OwnerName, string Currency);
    public sealed record TransactionRequest(decimal Amount, string Reference);
    public sealed record OpenAccountResponse(Guid Id);
    public sealed record BalanceResponse(Guid Id, decimal Balance);

    [HttpPost]
    [EndpointSummary("Open a new account")]
    [EndpointDescription("Command. Appends an AccountOpened event, which starts a new event stream for the account. Returns the new account id.")]
    [ProducesResponseType<OpenAccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Open(OpenAccountRequest request, CancellationToken ct) =>
        Run(async () =>
        {
            var id = await sender.Send(new OpenAccountCommand(request.OwnerName, request.Currency), ct);
            return Created($"/api/accounts/{id}", new OpenAccountResponse(id));
        });

    [HttpPost("{id:guid}/deposit")]
    [EndpointSummary("Deposit money")]
    [EndpointDescription("Command. Loads the account (aggregate), checks the amount is positive, and appends a MoneyDeposited event. Returns the new balance.")]
    [ProducesResponseType<BalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Deposit(Guid id, TransactionRequest request, CancellationToken ct) =>
        Run(async () => Ok(new BalanceResponse(id, await sender.Send(new DepositCommand(id, request.Amount, request.Reference), ct))));

    [HttpPost("{id:guid}/withdraw")]
    [EndpointSummary("Withdraw money")]
    [EndpointDescription("Command. Loads the account (aggregate), checks the amount is positive and covered by the balance, and appends a MoneyWithdrawn event. Returns the new balance. 409 if funds are insufficient.")]
    [ProducesResponseType<BalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Withdraw(Guid id, TransactionRequest request, CancellationToken ct) =>
        Run(async () => Ok(new BalanceResponse(id, await sender.Send(new WithdrawCommand(id, request.Amount, request.Reference), ct))));

    [HttpGet]
    [EndpointSummary("List accounts (projection)")]
    [EndpointDescription("Query. Returns all AccountSummary read models, ordered by owner name. Used to fill account pickers.")]
    [ProducesResponseType<IReadOnlyList<AccountSummary>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List(CancellationToken ct) =>
        Run(async () => Ok(await sender.Send(new ListAccountsQuery(), ct)));

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get account summary (projection)")]
    [EndpointDescription("Query. Reads the AccountSummary read model, built by a projection from the events: owner, currency, balance, totals and transaction count. Use this for display and reporting.")]
    [ProducesResponseType<AccountSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetSummary(Guid id, CancellationToken ct) =>
        Run(async () => Ok(await sender.Send(new GetAccountSummaryQuery(id), ct)));

    [HttpGet("{id:guid}/transactions")]
    [EndpointSummary("List account transactions")]
    [EndpointDescription("Query. Reads the account's event stream and returns its deposits and withdrawals (type, amount, reference, time), newest first.")]
    [ProducesResponseType<IReadOnlyList<TransactionItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetTransactions(Guid id, CancellationToken ct) =>
        Run(async () => Ok(await sender.Send(new GetTransactionsQuery(id), ct)));

    [HttpGet("{id:guid}/snapshot")]
    [EndpointSummary("Get account snapshot (aggregate state)")]
    [EndpointDescription("Query. Reads the stored snapshot of the BankAccount aggregate, i.e. the current state produced by replaying the events. This is the same object the commands use to make decisions.")]
    [ProducesResponseType<BankAccount>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetSnapshot(Guid id, CancellationToken ct) =>
        Run(async () => Ok(await sender.Send(new GetAccountSnapshotQuery(id), ct)));

    private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (AccountNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == "Insufficient funds")
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
