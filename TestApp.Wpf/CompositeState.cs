using System;

namespace TestApp.Wpf
{
    public class CompositeState
    {
        public DateTime LastTime { get; set; }
        public Guid RandomGuid { get; set; }
        public CompositeSubState SubState { get; set; }

        public CompositeState()
        {
            LastTime = DateTime.Now;
            RandomGuid = Guid.NewGuid();
            SubState = new CompositeSubState();
        }
    }
}
