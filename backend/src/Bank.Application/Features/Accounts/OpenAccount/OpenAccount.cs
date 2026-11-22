using Bank.Domain;
using MediatR;
using Bank.Application.Abstractions;

namespace Bank.Application.Features.Accounts.OpenAccount;

public sealed record OpenAccountCommand(string OwnerName, string Currency) : IRequest<Guid>;

internal sealed class OpenAccountCommandHandler(IAccountRepository accounts) : IRequestHandler<OpenAccountCommand, Guid>
{
    public async Task<Guid> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        var accountId = Guid.NewGuid();

        // 1. Domain decision validates invariants and produces the immutable event
        var openedEvent = BankAccount.Open(accountId, request.OwnerName, request.Currency);

        // 2. Persist a new stream with AccountOpened as the first event
        await accounts.OpenAsync(accountId, openedEvent, cancellationToken);
        return accountId;
    }
}