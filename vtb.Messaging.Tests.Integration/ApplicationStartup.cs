using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using vtb.Messaging.Configuration;
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
            BusHostConfigurator
                .WithConfiguration(Configuration.GetSection("RabbitMq"))
                .HandleCommand<TestCommand1>()
                .HandleCommand<TestCommand2>()
                .HandleEvent<TestEvent1>()
                .HandleEvent<TestEvent2>()
                .Configure(services);
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }

    [SetUpFixture]
    public class BaseApplicationTestFixture : IDisposable
    {
        public static readonly int RmqPort = 5675;
        private const string _rmqImage = "rabbitmq";
        private const string _rmqImageTag = "3";
        private DockerClient _dockerClient;
        private string _rabbitMqContainerId;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _dockerClient = GetDockerClient();
            await EnsureRabbitImagePulled();

            var createContainerResponse = await CreateRabbitMqContainer();
            _rabbitMqContainerId = createContainerResponse.ID;

            await StartRabbitMqContainer();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dockerClient.Containers
                .StopContainerAsync(_rabbitMqContainerId, new ContainerStopParameters());
            await _dockerClient.Containers
                .RemoveContainerAsync(_rabbitMqContainerId, new ContainerRemoveParameters());
        }

        private Task StartRabbitMqContainer()
        {
            return _dockerClient
                .Containers
                .StartContainerAsync(_rabbitMqContainerId, new ContainerStartParameters());
        }

        private Task<CreateContainerResponse> CreateRabbitMqContainer()
        {
            var parameters = new CreateContainerParameters()
            {
                Image = $"{_rmqImage}:{_rmqImageTag}",
                Name = $"TEST_RabbitMq_{Guid.NewGuid()}",
                HostConfig = new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>()
                    {
                        ["5672/tcp"] = new PortBinding[]
                        {
                            new() { HostPort = $"{RmqPort}/tcp" }
                        },
                        ["4369/tcp"] = new PortBinding[]
                        {
                            new() { HostPort = $"43690/tcp" }
                        },
                        ["5671/tcp"] = new PortBinding[]
                        {
                            new() { HostPort = $"56710/tcp" }
                        },
                        ["15691/tcp"] = new PortBinding[]
                        {
                            new() { HostPort = $"15692/tcp" }
                        },
                        ["15692/tcp"] = new PortBinding[]
                        {
                            new() { HostPort = $"15693/tcp" }
                        },
                        ["25672/tcp"] = new PortBinding[]
                        {
                            new() { HostPort = $"25673/tcp" }
                        }
                    }
                }
            };

            return _dockerClient
                .Containers
                .CreateContainerAsync(parameters);
        }

        private async Task EnsureRabbitImagePulled()
        {
            await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = $"{_rmqImage}:{_rmqImageTag}"
            }, null, new Progress<JSONMessage>());
        }

        private static DockerClient GetDockerClient()
        {
            var dockerUri = IsRunningOnWindows()
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";
            return new DockerClientConfiguration(new Uri(dockerUri))
                .CreateClient();
        }

        private static bool IsRunningOnWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        public void Dispose()
        {
            OneTimeTearDown().GetAwaiter().GetResult();
        }
    }
}