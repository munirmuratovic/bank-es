namespace Bank.Domain;

public record AccountOpened(Guid AccountId, string OwnerName, string Currency);
public record MoneyDeposited(Guid AccountId, decimal Amount, string Reference);
public record MoneyWithdrawn(Guid AccountId, decimal Amount, string Reference);