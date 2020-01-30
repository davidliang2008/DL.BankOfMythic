using DL.BankOfMythic.Domain.Core.Bus;
using DL.BankOfMythic.Domain.Core.Commands;
using DL.BankOfMythic.Domain.Core.Events;
using MediatR;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL.BankOfMythic.Infra.Bus.RabbitMQ
{
    public sealed class RabbitMQEventBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly IDictionary<string, IList<Type>> _handlers;
        private readonly IList<Type> _eventTypes;

        public RabbitMQEventBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, IList<Type>>();
            _eventTypes = new List<Type>();
        }

        public Task SendCommand<TCommand>(TCommand command) where TCommand : Command
        {
            return _mediator.Send(command);
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var eventName = @event.GetType().Name;

                    channel.QueueDeclare(eventName, false, false, false, null);

                    var message = JsonConvert.SerializeObject(@event);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish("", eventName, null, body);
                }
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var eventType = typeof(TEvent);

            if (!_eventTypes.Contains(eventType))
            {
                _eventTypes.Add(eventType);
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            var eventHandlerType = typeof(TEventHandler);

            // Make sure it doesn't have the handler type
            if (_handlers[eventName].Any(x => x.GetType() == eventHandlerType))
            {
                throw new ArgumentException(
                    $"Handler type { eventHandlerType.Name } already is registered for '{ eventName }'.");
            }

            _handlers[eventName].Add(eventHandlerType);

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(eventName, false, false, false, null);

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.Received += (sender, eventMessage) =>
                    {
                        var eventName = eventMessage.RoutingKey;
                        var message = Encoding.UTF8.GetString(eventMessage.Body);

                        try
                        {
                            if (_handlers.ContainsKey(eventName))
                            {
                                var subscriptions = _handlers[eventName];
                                foreach (var subscription in subscriptions)
                                {
                                    var handler = Activator.CreateInstance(subscription);
                                    if (handler == null)
                                    {
                                        continue;
                                    }

                                    var eventType = _eventTypes
                                        .SingleOrDefault(x => x.Name == eventName);

                                    var @event = JsonConvert.DeserializeObject(message, eventType);

                                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                                    // To kick off the handle method
                                    concreteType.GetMethod("Handle").Invoke(handler, new[] { @event });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        return Task.CompletedTask;
                    };

                    channel.BasicConsume(eventName, true, consumer);
                }
            }
        }
    }
}
