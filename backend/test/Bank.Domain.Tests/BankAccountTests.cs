using Bank.Domain;

namespace Bank.Domain.Tests;

public class BankAccountTests
{
    [Fact]
    public void Replaying_events_reconstructs_account_state()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new BankAccount();

        // Act
        var opened = BankAccount.Open(accountId, "John Doe", "USD");
        account.Apply(opened);

        var deposited = account.Deposit(100m, "Initial deposit");
        account.Apply(deposited);

        var withdrawn = account.Withdraw(50m, "ATM withdrawal");
        account.Apply(withdrawn);

        // Assert
        Assert.Equal(accountId, account.Id);
        Assert.Equal("John Doe", account.OwnerName);
        Assert.Equal("USD", account.Currency);
        Assert.Equal(50m, account.Balance);
    }

    [Fact]
    public void Withdrawing_more_than_balance_throws_exception()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new BankAccount();
        
        var opened = BankAccount.Open(accountId, "Jane Doe", "USD");
        account.Apply(opened);

        var deposited = account.Deposit(100m, "Initial deposit");
        account.Apply(deposited);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(150m, "Attempt to overdraw"));
    }
}
