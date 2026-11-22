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

    public async Task<bool> ModifyAsync(Guid accountId, Func<BankAccount, object> decide, CancellationToken cancellationToken)
    {
        // Replay the stream to get the current state, then append with the expected version for optimistic concurrency.
        var state = await session.Events.FetchStreamStateAsync(accountId, cancellationToken);
        if (state is null)
        {
            return false;
        }

        var account = await session.Events.AggregateStreamAsync<BankAccount>(accountId, token: cancellationToken);
        if (account is null)
        {
            return false;
        }

        session.Events.Append(accountId, state.Version + 1, decide(account));
        await session.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<AccountSummary?> GetSummaryAsync(Guid accountId, CancellationToken cancellationToken)
        => session.LoadAsync<AccountSummary>(accountId, cancellationToken);

    public async Task<IReadOnlyList<AccountSummary>> ListSummariesAsync(CancellationToken cancellationToken)
        => await session.Query<AccountSummary>().OrderBy(a => a.OwnerName).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TransactionItem>?> GetTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var events = await session.Events.FetchStreamAsync(accountId, token: cancellationToken);
        if (events.Count == 0)
        {
            return null;
        }

        return events
            .Select(e => e.Data switch
            {
                MoneyDeposited d => new TransactionItem("Deposit", d.Amount, d.Reference, e.Timestamp),
                MoneyWithdrawn w => new TransactionItem("Withdrawal", w.Amount, w.Reference, e.Timestamp),
                _ => null
            })
            .OfType<TransactionItem>()
            .OrderByDescending(t => t.Timestamp)
            .ToList();
    }

    public Task<BankAccount?> GetSnapshotAsync(Guid accountId, CancellationToken cancellationToken)
        => session.LoadAsync<BankAccount>(accountId, cancellationToken);
}
