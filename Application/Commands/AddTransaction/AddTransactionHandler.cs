using Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Application.Commands.AddTransaction;

public class AddTransactionHandler : IHandleMessages<AddTransactionCommand>
{ 
    private ILogger<AddTransactionHandler> _logger;
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
            throw new ArgumentNullException("Savings account cannot be negative");
        }

        await _savingsAccountRepository.Update($"{message.BankName}-{message.AccountId}", savingsAccount =>
            {
                if (message.TransactionType != null)
                    savingsAccount.AddTransaction(message.TransactionType, message.Amount);
                return Task.CompletedTask;
            });
    }

}