using System;
using Redux.WithSubstates.Actions;

namespace TestApp.Wpf
{
    public class UpdateSubstateTimeAction : IAction
    {
        public DateTime NewTime { get; set; }

        public UpdateSubstateTimeAction(DateTime newTime)
        {
            NewTime = newTime;
        }
    }
}