using Microfinance.Domain.Enums;
using Microfinance.Domain.Exceptions;

namespace Microfinance.Application.Engines;

public class AmortizationCalculatorFactory
{
    private readonly IEnumerable<IAmortizationCalculator> _calculators;

    public AmortizationCalculatorFactory(IEnumerable<IAmortizationCalculator> calculators)
    {
        _calculators = calculators;
    }

    public IAmortizationCalculator GetCalculator(InterestCalculationType type)
    {
        var calculator = _calculators.FirstOrDefault(c => c.SupportedType == type);
        if (calculator == null)
        {
            throw new DomainException($"No calculation engine registered for interest type: {type}");
        }
        return calculator;
    }
}