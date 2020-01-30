using DL.BankOfMythic.Domain.Core.Events;
using System.Threading.Tasks;

namespace DL.BankOfMythic.Domain.Core.Bus
{
    public interface IEventHandler<in TEvent>
        where TEvent : Event
    {
        Task Handle(TEvent @event);
    }
}
