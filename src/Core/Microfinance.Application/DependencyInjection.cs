using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microfinance.Application.Common.Behaviors;
using Microfinance.Application.Engines;

namespace Microfinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // MediatR & Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Amortization Engines
        services.AddTransient<IAmortizationCalculator, FlatRateCalculator>();
        services.AddTransient<IAmortizationCalculator, ReducingBalanceCalculator>();
        services.AddTransient<AmortizationCalculatorFactory>();

        return services;
    }
}