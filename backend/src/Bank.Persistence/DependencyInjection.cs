namespace Bank.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BankDb")
            ?? throw new InvalidOperationException("Connection string 'BankDb' not found.");

        services.AddMarten(options => 
        {
            options.Connection(connectionString);

            options.Events.AddEventTypes([
                typeof(Bank.Domain.AccountOpened),
                typeof(Bank.Domain.MoneyDeposited),
                typeof(Bank.Domain.MoneyWithdrawn)
            ]);

            options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline);

            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
        })
        .UseLightweightSessions();

        return services;
    }
}