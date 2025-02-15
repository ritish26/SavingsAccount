using Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Application.Commands.AddTransaction;

public class AddTransactionHandler : IHandleMessages<AddTransactionCommand>
{ 
    private readonly ILogger<AddTransactionHandler> _logger;
    private readonly ISavingsAccountRepository _savingsAccountRepository;

    public AddTransactionHandler(ILogger<AddTransactionHandler> logger,
        ISavingsAccountRepository savingsAccountRepository)
    {
        ArgumentNullException.ThrowIfNull(savingsAccountRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _savingsAccountRepository = savingsAccountRepository;
    }

    public async Task Handle(AddTransactionCommand message, IMessageHandlerContext context)
    {
        if (message.Amount < 0)
        {
            throw new ArgumentNullException("Amount for transaction cannot be negative");
        }

        try
        {
            await _savingsAccountRepository.Update($"{message.BankName}-{message.AccountId}", savingsAccount =>
            {
                if (message.TransactionType != null)
                    savingsAccount.AddTransaction(message.TransactionType, message.Amount);
                return Task.CompletedTask;
            });
            _logger.LogDebug($"Transaction {message.TransactionType} has been successfully added");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, $"Transaction {message.TransactionType} has been failed because of {ex.Message}");
        }
       
    }

}