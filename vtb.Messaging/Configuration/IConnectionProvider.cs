using System;
using RabbitMQ.Client;

namespace vtb.Messaging.Configuration
{
    public interface IConnectionProvider : IDisposable
    {
        bool IsConnected { get; }
        IModel CreateModel();
    }
}