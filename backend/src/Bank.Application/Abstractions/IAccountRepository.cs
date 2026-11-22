using Bank.Domain;

namespace Bank.Application.Abstractions;

public interface IAccountRepository
{
    Task OpenAsync(Guid accountId, AccountOpened opened, CancellationToken cancellationToken);
}
