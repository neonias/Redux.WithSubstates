using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates.Tests
{
    [TestClass]
    public class StoreTest
    {
        private const string InitialState = "initial_state";
        private const string NewState = "new_state";

        [TestMethod]
        public void Store_CanGetCurrentState()
        {
            // given
            var reducer = new Mock<IReducer<string>>();
            reducer
                .Setup(_ => _.Reduce(InitialState, It.IsAny<IAction>()))
                .Returns(NewState);
            IStore<string> store = new Store<string>(InitialState, reducer.Object);

            // when
            var state = store.State;

            // then
            Assert.AreEqual(InitialState, state);
        }

        [TestMethod]
        public void Store_ChangesTheStateToWhatReducerReturns()
        {
            // given
            var reducer = new Mock<IReducer<string>>();
            var action = new Mock<ChangeStringAction>();
            reducer
                .Setup(_ => _.Reduce(InitialState, It.IsAny<ChangeStringAction>()))
                .Returns(NewState);
            IStore<string> store = new Store<string>(InitialState, reducer.Object);

            // when
            store.Dispatch(action.Object);
            var state = store.State;

            // then
            Assert.AreEqual(NewState, state);
        }

        [TestMethod]
        public void Store_ItShouldNotifySubscribersWhenReducerReturnsUpdatedState()
        {
            // given
            var reducer = new Mock<IReducer<string>>();
            var action = new ChangeStringAction(InitialState);
            var newState = "";
            reducer
                .Setup(_ => _.Reduce(InitialState, action))
                .Returns(NewState);

            IStore<string> store = new Store<string>(InitialState, reducer.Object);

            // when
            store.Subscribe(_ => newState = _);
            store.Dispatch(action);

            // then
            Assert.AreEqual(NewState, newState);
            reducer.VerifyAll();
        }

        [TestMethod]
        public void Store_ItShouldCallReducersReduceMethodWithGivenAction()
        {
            // given
            var reducer = new Mock<IReducer<string>>();
            var action = new ChangeStringAction(InitialState);
            reducer
                .Setup(_ => _.Reduce(InitialState, action))
                .Returns(NewState);

            IStore<string> store = new Store<string>(InitialState, reducer.Object);

            // when
            store.Dispatch(action);

            // then
            reducer.VerifyAll();
        }

        [TestMethod]
        public void Store_ItShouldNotNotifySubscribersWhenReducerReturnsTheSameState()
        {
            // given
            var reducer = new Mock<IReducer<string>>();
            var action = new Mock<IAction>();
            var newState = "";
            reducer
                .Setup(_ => _.Reduce(InitialState, It.IsAny<IAction>()))
                .Returns(InitialState);

            IStore<string> store = new Store<string>(InitialState, reducer.Object);

            // when
            store.Subscribe(_ => newState = _);
            store.Dispatch(action.Object);

            // then
            Assert.AreEqual(InitialState, newState);
            reducer.VerifyAll();
        }

        [TestMethod]
        public void Store_Select_ShouldReturnAStoreWithCorrectSubstate()
        {
            // given
            var dt = DateTime.Now;
            var state = new CompositeState("john", dt);
            var reducer = CreateBasicReducerMock(state);
            IStore<CompositeState> store = new Store<CompositeState>(state, reducer.Object);

            // when
            IStore<string> stringSubStore = store.Select(_ => _.Name);
            IStore<DateTime> dateTimeSubStore = store.Select(_ => _.UpdatedAt);

            // then
            Assert.IsInstanceOfType(stringSubStore.State, typeof(string));
            Assert.IsInstanceOfType(dateTimeSubStore.State, typeof(DateTime));
        }

        [TestMethod]
        public void Store_Select_SubstoresShouldOnlyReactToChangesOfSubstate()
        {
            // STORY
            // Subscribed actions are called at least once - for the initial state.
            // After dispatch (WHEN), the reducer only changes the Name property of
            // the CompositeState. Therefore, stringSubStore subscribers should be
            // called twice while dateTimeSubStore subscribers only once.

            // given
            var dt = DateTime.Now;
            var state = new CompositeState("john", dt);
            var updateNameAction = new ChangeStringAction("mike");
            var reducer = new Mock<IReducer<CompositeState>>();
            reducer
                .Setup(_ => _.Reduce(state, updateNameAction))
                .Returns((CompositeState oldState, ChangeStringAction action) =>
                {
                    oldState.Name = action.NewString;
                    return oldState;
                });
            IStore<CompositeState> store = new Store<CompositeState>(state, reducer.Object);
            var numCallsStringSubStore = 0;
            var numCallsDateTimeSubStore = 0;
            IStore<string> stringSubStore = store.Select(_ => _.Name);
            IStore<DateTime> dateTimeSubStore = store.Select(_ => _.UpdatedAt);
            stringSubStore.Subscribe(_ =>
            {
                numCallsStringSubStore++;
            });
            dateTimeSubStore.Subscribe(_ =>
            {
                numCallsDateTimeSubStore++;
            });

            // when
            store.Dispatch(updateNameAction);

            // then
            Assert.AreEqual(2, numCallsStringSubStore);
            Assert.AreEqual(1, numCallsDateTimeSubStore);
        }

        [TestMethod]
        public void Store_ExecuteForEveryDispatch_ShouldBeRunForDispatchOfAnyAction()
        {
            // given
            var dt = DateTime.Now;
            var state = new CompositeState("john", dt);
            var reducer = CreateBasicReducerMock(state);
            IStore<CompositeState> store = new Store<CompositeState>(state, reducer.Object);
            var numDispatches = 0;
            var actionMock = new Mock<IAction>();

            // when
            store.ExecuteForEveryDispatch(action => numDispatches++);
            store.Dispatch(new ChangeStringAction(""));
            store.Dispatch(actionMock.Object);

            // then
            Assert.AreEqual(2, numDispatches);
        }

        [TestMethod]
        public void Store_State_IfTheStateIsUpdatedThePropertyShouldBeUpdatedToo()
        {
            // given
            var dt = DateTime.Now;
            var state = new CompositeState("john", dt);
            var newState = new CompositeState("mike", dt);
            var reducer = new Mock<IReducer<CompositeState>>();
            var action = new Mock<IAction>();
            var dispatchTime = 0;
            reducer
                .Setup(_ => _.Reduce(state, It.IsAny<IAction>()))
                .Returns(newState);
            IStore<CompositeState> store = new Store<CompositeState>(state, reducer.Object);

            // when & then
            store.Subscribe(_ =>
            {
                if (dispatchTime++ == 1)
                    Assert.IsTrue(store.State == newState);
            });
            store.Dispatch(action.Object);
        }

        private Mock<IReducer<T>> CreateBasicReducerMock<T>(T reduceReturnValue)
        {
            var reducer = new Mock<IReducer<T>>();
            reducer
                .Setup(_ => _.Reduce(It.IsAny<T>(), It.IsAny<IAction>()))
                .Returns(reduceReturnValue);
            return reducer;
        }
    }

    public class ChangeStringAction : IAction
    {
        public string NewString { get; }

        public ChangeStringAction()
        {
        }

        public ChangeStringAction(string newString)
        {
            NewString = newString;
        }
    }

    public class CompositeState
    {
        public string Name { get; set; }
        public DateTime UpdatedAt { get; set; }

        public CompositeState(string name, DateTime updatedAt)
        {
            Name = name;
            UpdatedAt = updatedAt;
        }
    }
}
