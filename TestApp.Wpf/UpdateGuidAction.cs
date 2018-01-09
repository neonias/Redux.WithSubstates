using System;
using Redux.WithSubstates.Actions;

namespace TestApp.Wpf
{
    public class UpdateGuidAction : IAction
    {
        public Guid Guid { get; set; }

        public UpdateGuidAction(Guid guid)
        {
            this.Guid = guid;
        }
    }
}
