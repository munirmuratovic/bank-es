using Bank.Application.Abstractions;
using MediatR;

namespace Bank.Application.Features.Accounts.Deposit;

/// <summary>Returns the new balance.</summary>
public sealed record DepositCommand(Guid AccountId, decimal Amount, string Reference) : IRequest<decimal>;

internal sealed class DepositCommandHandler(IAccountRepository accounts) : IRequestHandler<DepositCommand, decimal>
{
    public async Task<decimal> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        var balance = 0m;
        var found = await accounts.ModifyAsync(request.AccountId, account =>
        {
            var evt = account.Deposit(request.Amount, request.Reference);
            balance = account.Balance + evt.Amount;
            return evt;
        }, cancellationToken);

        return found ? balance : throw new AccountNotFoundException(request.AccountId);
    }
}
