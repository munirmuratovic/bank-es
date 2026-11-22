using Bank.Application.Abstractions;
using Bank.Domain;
using MediatR;

namespace Bank.Application.Features.Accounts.GetAccount;

public sealed record GetAccountSummaryQuery(Guid AccountId) : IRequest<AccountSummary>;
public sealed record GetAccountSnapshotQuery(Guid AccountId) : IRequest<BankAccount>;

internal sealed class GetAccountSummaryQueryHandler(IAccountRepository accounts)
    : IRequestHandler<GetAccountSummaryQuery, AccountSummary>
{
    public async Task<AccountSummary> Handle(GetAccountSummaryQuery request, CancellationToken cancellationToken)
        => await accounts.GetSummaryAsync(request.AccountId, cancellationToken)
           ?? throw new AccountNotFoundException(request.AccountId);
}

internal sealed class GetAccountSnapshotQueryHandler(IAccountRepository accounts)
    : IRequestHandler<GetAccountSnapshotQuery, BankAccount>
{
    public async Task<BankAccount> Handle(GetAccountSnapshotQuery request, CancellationToken cancellationToken)
        => await accounts.GetSnapshotAsync(request.AccountId, cancellationToken)
           ?? throw new AccountNotFoundException(request.AccountId);
}
