using MediatR;

namespace DL.BankOfMythic.Domain.Core.Events
{
    public abstract class Message : IRequest<bool>
    {
        public string MessageType { get; protected set; }

        protected Message()
        {
            this.MessageType = GetType().Name;
        }
    }
}
