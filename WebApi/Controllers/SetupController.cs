using Infrastructure.EventStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SavingsAccount.Controllers;


[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SetupController> _logger;
    private readonly IEventStoreAdmin _eventStoreAdmin;

    public SetupController(IConfiguration configuration,
        ILogger<SetupController> logger, IEventStoreAdmin eventStoreAdmin)
    {
        _configuration = configuration;
        _logger = logger;
        _eventStoreAdmin = eventStoreAdmin;
    }

    [HttpPost("")]
    public async Task<IActionResult> Setup()
    {
        await CreateProjectionAsync();
        return Ok();
    }

    private async Task CreateProjectionAsync()
    {
        var projectionName = "Bank.SavingsAccountProjection";
        if (!await _eventStoreAdmin.ProjectionExists(projectionName))
        {
            _logger.LogInformation($"projection {projectionName} does not exists, Creating projection.......",
                projectionName);
            await _eventStoreAdmin.CreateContinuousProjection(projectionName,
                ProjectionQuery, true, true, false);
        }
        else
        {
            _logger.LogInformation($"projection {projectionName} already exists.", projectionName);
        }
    }

    private static readonly string ProjectionQuery =
        @"
options({
    $includeLinks: false, 
}); 

fromAll()
.when({ 
    $any: function(state, evt){ 
        var streamId = evt.streamId || ''; 
        streamId = streamId.toLowerCase(); 

        if(!streamId.startsWith('bank.savings_account.')){
            return;
        } 

        if(streamId.startsWith('bank.savings_account.changelog.') || 
        streamId.startsWith('bank.savings_account.snapshot.')){ 
            return; 
        } 

        linkTo('bank.savings_account.changelog', evt); 
    } 
});";
}