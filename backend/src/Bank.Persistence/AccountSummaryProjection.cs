using Bank.Application.Abstractions;
using Bank.Domain;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace Bank.Persistence;

/// <summary>Builds the AccountSummary read model from account events, updated inline in the same transaction.</summary>
public class AccountSummaryProjection : SingleStreamProjection<AccountSummary, Guid>
{
    public static AccountSummary Create(AccountOpened e) => new()
    {
        Id = e.AccountId,
        OwnerName = e.OwnerName,
        Currency = e.Currency
    };

    public static void Apply(MoneyDeposited e, AccountSummary s)
    {
        s.Balance += e.Amount;
        s.TotalDeposited += e.Amount;
        s.TransactionCount++;
    }

    public static void Apply(MoneyWithdrawn e, AccountSummary s)
    {
        s.Balance -= e.Amount;
        s.TotalWithdrawn += e.Amount;
        s.TransactionCount++;
    }
}
