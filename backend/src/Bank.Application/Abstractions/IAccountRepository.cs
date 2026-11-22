using Bank.Domain;

namespace Bank.Application.Abstractions;

public interface IAccountRepository
{
    /// <summary>Starts a new event stream for the account.</summary>
    Task OpenAsync(Guid accountId, AccountOpened opened, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the aggregate, lets <paramref name="decide"/> produce a new event from it, and appends that event
    /// (with optimistic concurrency). Returns false if the account does not exist.
    /// </summary>
    Task<bool> ModifyAsync(Guid accountId, Func<BankAccount, object> decide, CancellationToken cancellationToken);

    /// <summary>Reads the projected read model (AccountSummary).</summary>
    Task<AccountSummary?> GetSummaryAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Reads the inline snapshot of the aggregate (current state of BankAccount).</summary>
    Task<BankAccount?> GetSnapshotAsync(Guid accountId, CancellationToken cancellationToken);
}
