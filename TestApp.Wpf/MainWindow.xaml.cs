using System;
using System.Globalization;
using System.Windows;
using Redux.WithSubstates;
using Redux.WithSubstates.Actions;
using Redux.WithSubstates.DecoratingInterfaces;
using Redux.WithSubstates.Extensions.Combiners;

namespace TestApp.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IStore<CompositeState> _store;

        public MainWindow()
        {
            InitializeComponent();

            _store = new Store<CompositeState>(new CompositeState(), new Reducer<CompositeState>(), new IEffect<CompositeState>[]{new Effect<CompositeState>() });
            _store = new Store<CompositeState>(
                new CompositeState(), 
                new [] {new ReducerAsAClass() }.Combine<CompositeState>(),
                new EffectAsClass().Combine<CompositeState>()
                );

            _store
                .Select(_ => _.LastTime)
                .Subscribe(_ =>
                {
                    ActualDate.Text = _.ToString(CultureInfo.InvariantCulture);
                });
            _store
                .Select(_ => _.RandomGuid)
                .Subscribe(_ =>
                {
                    RandomGuid.Text = _.ToString();
                });

            // test selection from substate
            _store
                .Select(_ => _.SubState)
                .Select(_ => _.SubStateMessage)
                .Subscribe(_ =>
                {
                    SubStateMessage.Text = _;
                });

            _store
                .Select(_ => _.SubState)
                .Select(_ => _.SubStateTime)
                .Subscribe(_ =>
                {
                    SubStateTime.Text = _.ToString(CultureInfo.InvariantCulture);
                });

            _store.ExecuteForEveryDispatch((action) => Console.WriteLine($"All action: {action.GetType()}"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _store.Dispatch(new UpdateTimeAction(DateTime.Now));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _store.Dispatch(new UpdateGuidAction(Guid.NewGuid()));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _store.Dispatch(new UpdateSubstateMessageAction($"Message at {DateTime.Now}"));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _store.Dispatch(new UpdateSubstateTimeAction(DateTime.Now));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _store.Dispatch(new UpdateAllAction());
        }
    }

    public class ReducerAsAClass : IReducerClass<CompositeState>
    {
        public CompositeState Reduce(CompositeState state, UpdateTimeAction action)
        {
            state.LastTime = action.NewTime;
            return state;
        }

        public CompositeState Reduce(CompositeState state, UpdateGuidAction action)
        {
            state.RandomGuid = action.Guid;
            return state;
        }

        public CompositeState Reduce(CompositeState state, UpdateSubstateMessageAction action)
        {
            state.SubState.SubStateMessage = action.Message;
            return state;
        }

        public CompositeState Reduce(CompositeState state, UpdateSubstateTimeAction action)
        {
            state.SubState.SubStateTime = action.NewTime;
            return state;
        }

        public CompositeState Reduce(CompositeState state, UpdateAllAction action)
        {
            state.LastTime = DateTime.Now;
            state.RandomGuid = Guid.NewGuid();
            state.SubState.SubStateMessage = Guid.NewGuid().ToString();
            state.SubState.SubStateTime = DateTime.Now;
            return state;
        }
    }

    public class EffectAsClass : IEffectClass<CompositeState>
    {
        public IAction Execute(CompositeState state, UpdateTimeAction action)
        {
            Console.WriteLine(action.GetType());
            return NullAction.Instance;
        }

        public IAction Execute(CompositeState state, UpdateGuidAction action)
        {
            Console.WriteLine(action.GetType());
            return NullAction.Instance;
        }

        public IAction Execute(CompositeState state, UpdateSubstateMessageAction action)
        {
            Console.WriteLine(action.GetType());
            return NullAction.Instance;
        }

        public IAction Execute(CompositeState state, UpdateSubstateTimeAction action)
        {
            Console.WriteLine(action.GetType());
            return NullAction.Instance;
        }
    }

    public class Reducer<T> : IReducer<CompositeState>
    {
        public CompositeState Reduce(CompositeState state, IAction action)
        {
            if (action is UpdateTimeAction)
            {
                state.LastTime = ((UpdateTimeAction) action).NewTime;
            }

            if (action is UpdateGuidAction)
            {
                state.RandomGuid = new Random().Next(0, 2) == 1 ? ((UpdateGuidAction) action).Guid : state.RandomGuid;
            }

            if (action is UpdateSubstateMessageAction)
            {
                state.SubState.SubStateMessage = ((UpdateSubstateMessageAction) action).Message;
            }

            if (action is UpdateSubstateTimeAction)
            {
                state.SubState.SubStateTime = ((UpdateSubstateTimeAction) action).NewTime;
            }

            return state;
        }
    }

    public class Effect<T> : IEffect<CompositeState>
    {
        public IAction Execute(CompositeState state, IAction action)
        {
            Console.WriteLine(action.GetType());
            return NullAction.Instance;
        }
    }
}
