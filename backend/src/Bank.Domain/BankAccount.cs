using System.Text.Json.Serialization;

namespace Bank.Domain;

public class BankAccount
{
    [JsonInclude]
    public Guid Id { get; private set; }
    [JsonInclude]
    public string OwnerName { get; private set; } = string.Empty;
    [JsonInclude]
    public string Currency { get; private set; } = "USD";
    [JsonInclude]
    public decimal Balance { get; private set; }

    public BankAccount()
    {
        
    }

    public static AccountOpened Open(Guid accountId, string ownerName, string currency)
    {
        if(string.IsNullOrWhiteSpace(ownerName))
        {
            throw new ArgumentException("Owner name is required", nameof(ownerName));
        }

        return new AccountOpened(accountId, ownerName.Trim(), currency.ToUpperInvariant());
    }

    public MoneyDeposited Deposit(decimal amount, string reference)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero", nameof(amount));
        }

        // Balance += amount; // this is done in the event handler
        return new MoneyDeposited(Id, amount, reference);
    }

    public MoneyWithdrawn Withdraw(decimal amount, string reference)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be greater than zero", nameof(amount));
        }

        if(Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        // Balance -= amount; // this is done in the event handler
        return new MoneyWithdrawn(Id, amount, reference);
    }
    
    public void Apply(AccountOpened @event)
    {
        Id = @event.AccountId;
        OwnerName = @event.OwnerName;
        Currency = @event.Currency;
        Balance = 0m;
    }

    public void Apply(MoneyDeposited @event)
    {
        Balance += @event.Amount;
    }

    public void Apply(MoneyWithdrawn @event)
    {
        Balance -= @event.Amount;
    }
}