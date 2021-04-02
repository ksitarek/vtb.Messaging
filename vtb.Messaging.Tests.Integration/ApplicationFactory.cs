using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace vtb.Messaging.Tests.Integration
{
    public class ApplicationFactory : WebApplicationFactory<ApplicationStartup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["RabbitMq:UserName"] = "guest",
                        ["RabbitMq:Password"] = "guest",
                        ["RabbitMq:HostName"] = "localhost",
                        ["RabbitMq:Port"] = ContainerHelper.RmqPort.ToString()
                    });
                })
                .ConfigureWebHostDefaults(x =>
                {
                    x.UseStartup<ApplicationStartup>();
                    x.UseTestServer();
                });
            return builder;
        }
    }
}