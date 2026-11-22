namespace Bank.Application.Abstractions;

/// <summary>Read model built by a projection from the account's events. Optimised for queries.</summary>
public class AccountSummary
{
    public Guid Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public int TransactionCount { get; set; }
}
