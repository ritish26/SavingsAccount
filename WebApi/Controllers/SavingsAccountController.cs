using Application.Commands;
using Application.Commands.AddTransaction;
using Application.Requests;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SavingsAccount.Controllers;

[ApiController]
[Route("api/savings-account")]
public class SavingsAccountController : Controller
{
    private readonly ILogger<SavingsAccountController> _logger;
    private readonly IMessageSession _messageSession;
    private readonly IMapper _mapper;
    
    public SavingsAccountController(ILogger<SavingsAccountController> logger, IMessageSession messageSession, IMapper mapper)
    {
        _logger = logger;
        _messageSession = messageSession;
        _mapper = mapper;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSavingsAccount([FromBody] CreateSavingsAccountRequest request)
    {
        if (request.Balance < 0)
        {
           _logger.LogError("Invalid balance to create savings account"); 
           return BadRequest();
        }
        
        var command = _mapper.Map<CreateSavingsAccountCommand>(request);
        await _messageSession.SendLocal(command); 
        
       _logger.LogInformation("Savings account created successfully");
       return Ok();
    }
    
    [HttpPut("add-transaction/{bankId:long}/{accountId}")]
    public async Task<IActionResult> AddTransaction(long bankId, string accountId,
        [FromBody] AddTransactionRequest request)
    {
        if (request.Amount < 0)
        {
            _logger.LogError("Invalid balance for adding transaction "); 
            return BadRequest();
        }
        
        var command = _mapper.Map<AddTransactionCommand>(request);
        command.BankId = bankId;
        command.AccountId = accountId;
        
        await _messageSession.SendLocal(command); 
        _logger.LogInformation("Transaction added successfully");
        return Ok();
    }
    
}
