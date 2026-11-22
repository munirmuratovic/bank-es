using JasperFx.Events.Projections;
using Marten.Events.Projections;
using Bank.Domain;
using JasperFx.Events;
using JasperFx;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            options.DatabaseSchemaName = "bank_db";

            options.Events.AddEventTypes([
                typeof(Bank.Domain.AccountOpened),
                typeof(Bank.Domain.MoneyDeposited),
                typeof(Bank.Domain.MoneyWithdrawn)
            ]);

            options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline);
            options.Projections.Add<AccountSummaryProjection>(ProjectionLifecycle.Inline);
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
        })
        .UseLightweightSessions();

        services.AddScoped<Bank.Application.Abstractions.IAccountRepository, AccountRepository>();

        return services;
    }
}