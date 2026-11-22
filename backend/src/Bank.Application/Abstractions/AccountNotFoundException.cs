namespace Bank.Application.Abstractions;

public sealed class AccountNotFoundException(Guid accountId)
    : Exception($"Account {accountId} was not found.");
