using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vtb.Messaging.Configuration;
using vtb.Messaging.Pipelines;
using vtb.Messaging.Tests.Integration.TestClasses.Filters;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration
{
    public class ApplicationStartup
    {
        public ApplicationStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<StringBuilder>();
            services.AddScoped<TestTenantFilter.TenantProvider>();

            var consumePipeline = PipelineBuilder.Create()
                .AddFilter<TestLockFilter>()
                .AddFilter<TestTenantFilter>();

            BusHostConfigurator
                .WithConfiguration(Configuration.GetSection("RabbitMq"), new[] { typeof(ApplicationStartup).Assembly })
                .HandleCommand<TestCommand1>()
                .HandleCommand<TestCommand2>()
                .HandleEvent<TestEvent1>()
                .HandleEvent<TestEvent2>()
                .ConsumePipeline(consumePipeline)
                .Configure(services);
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}