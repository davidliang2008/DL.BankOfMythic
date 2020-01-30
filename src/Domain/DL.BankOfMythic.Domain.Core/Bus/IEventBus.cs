using DL.BankOfMythic.Domain.Core.Commands;
using DL.BankOfMythic.Domain.Core.Events;
using System.Threading.Tasks;

namespace DL.BankOfMythic.Domain.Core.Bus
{
    public interface IEventBus
    {
        Task SendCommand<TCommand>(TCommand command) 
            where TCommand : Command;

        void Publish<TEvent>(TEvent @event) 
            where TEvent : Event;

        void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}
