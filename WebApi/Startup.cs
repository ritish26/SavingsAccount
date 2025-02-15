using System.Reflection;
using Infrastructure.EventStore;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SavingsAccount.Middleware;

namespace SavingsAccount;

public class Startup
{
  private readonly IConfiguration _configuration;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddSingleton<CorrelationIdMiddleware>();
    services.AddEventStore(_configuration);
    services.AddSwaggerGen();
    services.AddControllers();
    services.AddAutoMapper(Assembly.Load("Application"));  //Load the automapper profiles from Application Project
    services.AddTransient<IAggregateStore, AggregateStore>();
    services.AddTransient<ISavingsAccountRepository, SavingsAccountRepository>();
    services.AddSingleton<IEventStore, Infrastructure.EventStore.EventStore>();
  }
  
  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {

    app.UseMiddleware<CorrelationIdMiddleware>();
    
    app.Build();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
      options.RoutePrefix = string.Empty; // This ensures Swagger UI is served at the root ("/")
    });

    app.UseHttpsRedirection();

    app.UseRouting();
    
    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllers();
    });

  }
}