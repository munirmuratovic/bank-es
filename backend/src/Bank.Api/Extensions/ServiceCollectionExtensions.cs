using Bank.Application;
using Bank.Persistence;

namespace Bank.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddApplication();
        services.AddPersistence(configuration);

        return services;
    }
}
