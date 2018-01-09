using System;

namespace TestApp.Wpf
{
    public class CompositeSubState
    {
        public string SubStateMessage { get; set; }
        public DateTime SubStateTime { get; set; }

        public CompositeSubState()
        {
            SubStateMessage = "Initial message";
            SubStateTime = DateTime.Now;
        }
    }
}
