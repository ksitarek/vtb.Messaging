using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace vtb.Messaging.Providers
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly ILogger<ConnectionProvider> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private IAutorecoveringConnection _connection;

        private readonly RetryPolicy ConnectionRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryForever(x => TimeSpan.FromMilliseconds(x * 100));

        public bool IsConnected
        {
            get => _connection != null && _connection.IsOpen;
        }

        public ConnectionProvider(
            ILogger<ConnectionProvider> logger,
            IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public IModel CreateModel()
        {
            if (_connection == null)
                ConnectionRetryPolicy.Execute(() =>
                {
                    _connection = Connect();
                });

            if (!_connection.IsOpen)
                throw new InvalidOperationException("To create channel there must be open connection to RabbitMQ.");

            return _connection.CreateModel();
        }

        private IAutorecoveringConnection Connect()
        {
            var connection = _connectionFactory.CreateConnection() as IAutorecoveringConnection;
            if (connection == null)
            {
                throw new NotSupportedException("Non-recoverable connection is not supported");
            }

            _logger.LogInformation("Connected to {cid}", GetConnectionId(connection));

            connection.ConnectionShutdown += OnConnectionShutdown;
            connection.ConnectionBlocked += OnConnectionBlocked;
            connection.ConnectionUnblocked += OnConnectionUnblocked;
            connection.RecoverySucceeded += OnRecoverySucceeded;

            return connection;
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            var connection = (IConnection)sender;
            _logger.LogInformation("Disconnected from: {cid} because of: {reason}",
                GetConnectionId(connection),
                e.ReplyText);
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            var connection = (IConnection)sender;
            _logger.LogInformation("Connection {cid} blocked because of: {reason}",
                GetConnectionId(connection),
                e.Reason);
        }

        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            var connection = (IConnection)sender;
            _logger.LogInformation("Connection {cid} unblocked.", GetConnectionId(connection));
        }

        private void OnRecoverySucceeded(object sender, EventArgs e)
        {
            var connection = (IConnection)sender;
            _logger.LogInformation("Connection {cid} was recovered.", GetConnectionId(connection));
        }

        private string GetConnectionId(IConnection connection)
        {
            return $"{connection.Endpoint.HostName}:{connection.Endpoint.Port}";
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.ConnectionBlocked -= OnConnectionBlocked;
                _connection.ConnectionUnblocked -= OnConnectionUnblocked;
                _connection.RecoverySucceeded -= OnRecoverySucceeded;
                _connection.Dispose();
            }
        }
    }
}