namespace Bank.Application.Abstractions;

/// <summary>One deposit or withdrawal, read from the account's event stream.</summary>
public sealed record TransactionItem(string Type, decimal Amount, string Reference, DateTimeOffset Timestamp);
