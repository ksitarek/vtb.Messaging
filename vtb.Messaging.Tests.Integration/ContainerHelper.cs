using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace vtb.Messaging.Tests.Integration
{
    public class ContainerHelper
    {
        public static readonly int RmqPort = 5675;
        private const string _containerNamePrefix = "VTBTEST";
        private const string _rmqImage = "rabbitmq";
        private const string _rmqImageTag = "3";
        private DockerClient _dockerClient;
        private string _rabbitMqContainerId;

        private bool _containersStopped = false;

        public ContainerHelper()
        {
        }

        public async Task StartContainers()
        {
            _dockerClient = GetDockerClient();
            await CleanupPreviousRuns();
            await EnsureRabbitImagePulled();

            var createContainerResponse = await CreateRabbitMqContainer();
            _rabbitMqContainerId = createContainerResponse.ID;
            await StartRabbitMqContainer();
        }

        public async Task StopContainers()
        {
            if (!_containersStopped)
            {
                await StopAndRemoveContainerById(_rabbitMqContainerId);

                _containersStopped = true;
            }
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

        private async Task StopAndRemoveContainerById(string containerId)
        {
            try
            {
                await _dockerClient.Containers
                    .StopContainerAsync(containerId, new ContainerStopParameters());
                await _dockerClient.Containers
                    .RemoveContainerAsync(containerId, new ContainerRemoveParameters());
            }
            catch (DockerContainerNotFoundException)
            {
                // this should not be a big deal for us
            }
        }

        private Task StartRabbitMqContainer()
        {
            return _dockerClient
                .Containers
                .StartContainerAsync(_rabbitMqContainerId, new ContainerStartParameters());
        }

        private async Task CleanupPreviousRuns()
        {
            var list = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters()
            {
                All = true
            });

            var testContainers = list.Where(x => x.Names.Any(n => n.StartsWith($"/{_containerNamePrefix}")));
            foreach (var testContainer in testContainers)
                await StopAndRemoveContainerById(testContainer.ID);
        }

        private Task<CreateContainerResponse> CreateRabbitMqContainer()
        {
            var parameters = new CreateContainerParameters()
            {
                Image = $"{_rmqImage}:{_rmqImageTag}",
                Name = $"{_containerNamePrefix}_RabbitMq_{Guid.NewGuid()}",
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
    }
}