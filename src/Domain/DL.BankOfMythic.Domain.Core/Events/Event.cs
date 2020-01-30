using System;

namespace DL.BankOfMythic.Domain.Core.Events
{
    public abstract class Event
    {
        public DateTime Timestamp { get; protected set; }

        protected Event()
        {
            this.Timestamp = DateTime.UtcNow;
        }
    }
}
