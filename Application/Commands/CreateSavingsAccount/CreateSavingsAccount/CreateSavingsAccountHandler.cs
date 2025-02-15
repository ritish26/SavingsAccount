using Domain.Aggregates;
using Domain.Events;
using Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Application.Commands.CreateSavingsAccount;

public class CreateSavingsAccountHandler : IHandleMessages<CreateSavingsAccountCommand>
{
    private readonly ILogger<CreateSavingsAccountHandler> _logger;
    private readonly ISavingsAccountRepository _savingsAccountRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateSavingsAccountHandler(ILogger<CreateSavingsAccountHandler> logger, ISavingsAccountRepository  savingsAccountRepository)
    {
        _logger = logger; 
        _savingsAccountRepository = savingsAccountRepository;
    }

    public async Task Handle(CreateSavingsAccountCommand message, IMessageHandlerContext context)
    {
        var logContext = new Dictionary<string, object>
        {
            { "CommandName", nameof(CreateSavingsAccountCommand) }
        };
        
        using var logScope = _logger.BeginScope(logContext);
        _logger.LogInformation("Creating saving account");
        
        try
        {
            await _savingsAccountRepository.Create(() =>
                Task.FromResult(new SavingsAccountAggregate(message.BankName, message.Balance,
                    message.BankId, message.AccountId)));
            _logger.LogInformation("SavingsAccount created successfully");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, $"Account is not able to create du to the error {ex.Message}");
        }
        
    }
}