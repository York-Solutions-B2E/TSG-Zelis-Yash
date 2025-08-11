using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommunicationLifecycle.Infrastructure.Data;
using CommunicationLifecycle.Infrastructure.Repositories;
using CommunicationLifecycle.Infrastructure.Messaging;

namespace CommunicationLifecycle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<CommunicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<ICommunicationRepository, CommunicationRepository>();
        services.AddScoped<ICommunicationTypeRepository, CommunicationTypeRepository>();

        // Add Messaging
        services.AddSingleton<IRabbitMQService, RabbitMQService>();

        return services;
    }
} 