using Microsoft.Extensions.DependencyInjection;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Infrastructure.Identity;
using Microfinance.Infrastructure.Notifications;
using Microfinance.Infrastructure.Storage;
using Microfinance.Infrastructure.Services;

namespace Microfinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Identity & JWT
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
      

        // Storage & Notifications
        services.AddTransient<IFileStorageService, LocalFileStorageService>();
        services.AddTransient<ISmsNotificationService, SmsNotificationService>();

        return services;
    }
}