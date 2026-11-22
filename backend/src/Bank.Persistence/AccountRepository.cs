using Bank.Application.Abstractions;
using Bank.Domain;
using Marten;

namespace Bank.Persistence;

internal sealed class AccountRepository(IDocumentSession session) : IAccountRepository
{
    public async Task OpenAsync(Guid accountId, AccountOpened opened, CancellationToken cancellationToken)
    {
        session.Events.StartStream<BankAccount>(accountId, opened);
        await session.SaveChangesAsync(cancellationToken);
    }
}
