using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using vtb.Messaging.Providers;

namespace vtb.Messaging.Busses
{
    public interface IBus
    {
        Task Send<TCommand>(TCommand message) where TCommand : class;

        Task Publish<TEvent>(TEvent message) where TEvent : class;

        Task Request<TRequest>(TRequest message) where TRequest : class;

        Task Respond<TResponse>(Guid messageId, TResponse response) where TResponse : class;
    }

    public class Bus : IBus
    {
        private readonly IConnectionProvider _connectionProvider;
        private IModel _model;

        private IModel Model
        {
            get
            {
                if (_model == null || !_model.IsOpen)
                {
                    _model = _connectionProvider.CreateModel();
                }

                return _model;
            }
        }

        public Bus(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public Task Publish<TEvent>(TEvent message) where TEvent : class
        {
            throw new NotImplementedException();
        }

        public Task Request<TRequest>(TRequest message) where TRequest : class
        {
            throw new NotImplementedException();
        }

        public Task Respond<TResponse>(Guid messageId, TResponse response) where TResponse : class
        {
            throw new NotImplementedException();
        }

        public Task Send<TCommand>(TCommand message) where TCommand : class
        {
            throw new NotImplementedException();
        }
    }
}