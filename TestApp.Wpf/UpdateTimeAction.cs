using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redux.WithSubstates.Actions;

namespace TestApp.Wpf
{
    public class UpdateTimeAction : IAction
    {
        public DateTime NewTime { get; }

        public UpdateTimeAction(DateTime newTime)
        {
            NewTime = newTime;
        }
    }
}
