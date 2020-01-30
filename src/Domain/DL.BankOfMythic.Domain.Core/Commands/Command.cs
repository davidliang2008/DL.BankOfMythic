using DL.BankOfMythic.Domain.Core.Events;
using System;

namespace DL.BankOfMythic.Domain.Core.Commands
{
    public abstract class Command : Message
    {
        public DateTime TimeStamp { get; protected set; }

        protected Command()
        {
            this.TimeStamp = DateTime.UtcNow;
        }
    }
}
