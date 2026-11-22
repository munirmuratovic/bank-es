using Bank.Application.Abstractions;
using MediatR;

namespace Bank.Application.Features.Accounts.Withdraw;

/// <summary>Returns the new balance.</summary>
public sealed record WithdrawCommand(Guid AccountId, decimal Amount, string Reference) : IRequest<decimal>;

internal sealed class WithdrawCommandHandler(IAccountRepository accounts) : IRequestHandler<WithdrawCommand, decimal>
{
    public async Task<decimal> Handle(WithdrawCommand request, CancellationToken cancellationToken)
    {
        var balance = 0m;
        var found = await accounts.ModifyAsync(request.AccountId, account =>
        {
            var evt = account.Withdraw(request.Amount, request.Reference);
            balance = account.Balance - evt.Amount;
            return evt;
        }, cancellationToken);

        return found ? balance : throw new AccountNotFoundException(request.AccountId);
    }
}
