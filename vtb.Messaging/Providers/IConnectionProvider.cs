using System;
using RabbitMQ.Client;

namespace vtb.Messaging.Providers
{
    public interface IConnectionProvider : IDisposable
    {
        bool IsConnected { get; }

        IModel CreateModel();
    }
}