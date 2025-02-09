namespace Application.Commands.AddTransaction;

public class AddTransactionHandler : IHandleMessages<AddTransactionCommand>
{
    private IHandleMessages<AddTransactionCommand> _handleMessagesImplementation;
    public Task Handle(AddTransactionCommand message, IMessageHandlerContext context)
    {
        return _handleMessagesImplementation.Handle(message, context);
    }
}