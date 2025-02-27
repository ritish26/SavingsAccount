using System.Reflection;
using Domain.Aggregates;
using Infrastructure;
using Infrastructure.EventStore;
using Infrastructure.Extensions;
using Infrastructure.Projections;
using Infrastructure.Projections.Grains;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Projections;
using Projections.ViewBuilders;
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
    //Configure mongodb
    var mongoSetting = _configuration.GetSection("MongoDBSettings:ConnectionString").Value;
    var mongoClient = new MongoClient(mongoSetting);
    services.AddSingleton<IMongoClient>(mongoClient);
    services.AddSingleton<CorrelationIdMiddleware>();
    services.AddEventStore(_configuration); 
    services.AddTransient<IAggregateStore, AggregateStore>();
    services.AddTransient<ISavingsAccountRepository, SavingsAccountRepository>();
    services.AddSingleton<IEventStore, Infrastructure.EventStore.EventStore>();
    services.AddAutoMapper(Assembly.Load("Application"));  //Load the automapper profiles from Application Project
    services.AddSingleton<IMongoContext, MongoContext>();
    services.AddTransient<SavingsAccountViewBuilder>();
    services.AddTransient<IProjection, SavingsAccountProjection>();
    services.AddTransient<IProjectionCheckpointStore, ApplicationViewCheckpointStore>();
    services.AddTransient<ITenantProjectionManagerFactory, TenantProjectionManagerFactory>();
    services.AddTransient<ITenantViewProjection, TenantViewProjection>();
    services.AddTransient<IViewProjectionGrain, ViewProjectionGrain>();
    services.AddHostedService<ChangelogPartitionBackgroundService>();
    services.AddHttpClient<IEventStoreAdmin, EventStoreAdmin>();
    services.AddSwaggerGen();
    services.AddControllers();
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