# Redux.WithSubstates
Redux for .NET.
Supports substate subscription, asynchronous Effects and more.

## Table of contents
- [Installation](#installation)
- [Quick introduction](#quick-introduction)
- [Why another Redux for .NET?](#Why-another-Redux-for-.NET?)
- [FAQs](#faqs)
- [References](#references)

## Installation
We recommend to install Redux.WithSubstates using NuGet package manager.
You can use either graphical interface or NuGet package manager with command:
```
Install-Package Redux.WithSubstates
```

## Quick introduction
In this section you'll find description of all classes that are necessary to work with Redux.WithSubstates library.
We provide simple (yet descriptive) examples of how to use this library.

### Store
*Store* is the heart of Redux.WithSubstates library. The Store:
- Manages state
- Allows to subscribe to state changes
- Allows to dispatch *Actions*

*Store* is a generic implementation of ```IStore<TState>``` interface. This generalization allows to use any object
of any type as the application state. An example of instantiation of Store is (Reducers and Effects are introduced below):
```c#
IStore<string> reduxStore = new Store<string>("initial state", reducer, effects);
```

#### Get the current state
Property. Gets the current Redux state.
It should be only used in Reducer and Effects.

#### Dispatch an action
Makes the store to execute Reducer's and all Effects' methods that handle actions of typeof(action).
An example of dispatching an actoin on a Store:
```c#
IStore<string> store = ...
...
IAction updateTimeAction = new UpdateTimeAction(DateTime.now);
store.Dispatch(updateTimeAction); 
```

#### Select a store handling a substate
Selects a new instance of IStore that handles a substate of the
overall redux state. It allows to Subscribe to any substate of the overall state,
which makes it easier to handle state changes.

Example of substate selection:
```c#
class CompositeState
{
    public string Name { get; set; }
    public string LastName { get; set; }
}


IStore<CompositeState> store = new Store(new CompositeState(), reducer, effects);

// returns IStore<string>, because compositeState.Name is a string
IStore<string> nameSubStore = store.Select(compositeState => compositeState.Name);

// returns IStore<string>, because compositeState.LastName is a string
IStore<string> lastNameSubStore = store.Select(compositeState => compositeState.LastName);
```

Example of handling state changes without selecting substate:
```c#
class CompositeState
{
    public string Name { get; set; }
    public string LastName { get; set; }
}
... 

IStore<CompositeState> store = new Store(new CompositeState(), reducer, effects);

store.Subscribe(newState =>
{
    if(newState.Name != currentName)
        Console.WriteLine($"Name changed to {newState.Name}");
    if(newState.LastName != currentLastName)
        Console.WriteLine($"Last name changed to {newState.LastName}");
});
```

Using substate selection, we can handle this situation in a more elegant way:
```c#
class CompositeState
{
    public string Name { get; set; }
    public string LastName { get; set; }
}
... 

IStore<CompositeState> store = new Store(new CompositeState(), reducer, effects);

store.Select(state => state.Name).Subscribe(newName =>
{
    Console.WriteLine($"Name changed to {newName}");
});
store.Select(state => state.LastName).Subscribe(newLastName =>
{
    Console.WriteLine($"Last name changed to {newLastName}");
});
```

This was a toy example, but imagine that you have an application with a complex state (and don't say that it has to be a bad design) -
in such a situation handling only a substate becomes a necessity.

#### Subscribe to state change
Registers an action that will be executed when a state (or substate) changes.
```c#
IStore<CompositeState> store = new Store...
...

store.Subscribe(newState => 
{
    Console.WriteLine($"State changed: {newState.Name} {newState.LastName}");
});
```

Or, you can use a function:
```c#
...
public void HandleStateChange(CompositeState newState)
{
    Console.WriteLine($"State changed: {newState.Name} {newState.LastName}")
}

store.Subscribe(HandleStateChange);
```

### State
Literally an instance of any class (or enum, struct) can be a Redux state. Here we show a few examples
of Redux stores with different states.

```c#
\\ string
IStore<string> stringStore = new Store<string>("initial state", reducer, effects);

\\ enum
public enum DaysOfWeek {Monday, Tuesday, Wednesday}
IStore<DaysOfWeek> daysStore = new Store<DaysOfWeek>(DaysOfWeek.Tuesday, reducer, effects);

\\ custom class : see examples with CompositeState in this Readme
```

### Action
Actions are used to "trigger" different methods of Reducers, based on the type of the action.

It is the actions that make Redux different from other application-wide-state management techniques (such as public, static class):
**Rather than directly changing application state, in Redux, Reducers are responsible for changing the application state.**
In other words, **rather than telling HOW THE STATE SHOULD BE CHANGED, in client code we say to Redux Store
WHAT ACTION SHOULD BE PERFORMED".

Once again - outside Redux Store (and Reducer and Effects), one does not directly care about how a particular
action changes the application state. The client code just tells Redux Store what action it has to perform.
The client (non-Redux part of the application) may *subscribe* to these changes of the application state.

Any type that implements the interface ```IAction``` may be used as an action in Redux.WithSubstates. An example of
use of action in Redux.WithSubstates is:

```c#
IStore<string> store = new Store<string>("john", reducer, effects);
...
IAction changeNameAction = new ChangeNameAction { NewName = "joe" };
store.Dispatch(changeNameAction);
```

Will the name, part of the state, be changed? Probably yes - it depends on Reducer.

### Reducer
Originally, Reducer is a function that takes an Action, the current state and returns a fully new instance of the 
state. How and whether at all the state will be changed, depends solely on the Reducer(s) and the type of the action.

In Redux.WithSubstates, there are multiple ways how the Reducer can be defined. Basically, any class that 
implements interface ```IReducer<TState>``` is a Reducer. So, the very first way is to implement a Reducer as following:

```c#
class AppState
{
    string Name { get; set; }
    string LastName { get; set; }
    DateTime NameUpdatedAt { get; set; }
}

class AppReducer : IReducer<AppState>
{
    public AppState Reduce(AppState currentState, IAction action)
    {
        if(action is SetUserNameAction)
        {
            currentState.Name = ((SetUserNameAction)action).Name;
        }
        if(action is SetUserLastNameAction)
        {
            currentState.LastName = ((SetUserLastNameAction)action).LastName;
        }
        // yes, we are interested in SetUserNameAction again,
        // because we want to do something semantically different
        if(action is SetUserNameAction)
        {
            currentState.NameUpdatedAt = DateTime.Now;
        }  
        return currentState;
    }
}

IStore<string> store = new Store(new AppState(), new AppReducer());
```

This approach is handy only if there are a couple of Actions and if the state is small. With increasing number of
actions or more complex state, one needs to *decompose* Reducer in some sense (btw, are those
casts of ```action``` argument not nasty?). Redux.WithSubstates offers an elegant way how to define Reducer
logic in multiple classes. There are multiple benefits:
- You can define multiple ```Reduce(...)``` methods
- You can define these ```Reduce(...)``` methods in multiple classes that implement ```IReducer<TState>```
- In these methods, you ask for a specific type of ```IAction```, which helps you to get rid of nasty casting

So, what is the solution that Redux.WithSubstates offers? It is a **```Combine()``` extension method**, applicable
to anything derived from ```IEnumerable<IReducerClass<T>>```. Let's see an example.

```c#
class NameReducer : IReducerClass<AppState>
{
    public AppState UpdateNameReducer(AppState currentState, SetUserNameAction action)
    {
        currentState.Name = action.Name;
        return currentState;
    }

    public AppState UpdateNameUpdatedAtReducer(AppState currentState, SetUserNameAction action)
    {
        currentState.NameUpdatedAt = DateTime.Now;
        return currentState;
    }
}

class LastNameReducer : IReducerClass<AppState>
{
    public AppState UpdateLastNameReduce(AppState currentState, SetUserLastNameAction action)
    {
        currentState.LastName = action.LastName;
        return currentState;
    }
}

\\ now we need to get a single IReducer<TState> class. Combine() comes to aid.
IReducer<AppState> combinedReducer = new[]{ new NameReducer(), new LastNameReducer()}
    .Combine();
```

What does the ```Combine()``` extension method do? Under the hood, ```Combine()``` relies on
reflection. It basically goes through all the whole ```IEnumerable```. In every object it looks
for methods with following signature:
```c#
public TState AnyMethodNameItDoesNotMatter(TState a, ANYTHING_THAT_IMPLEMENTS_IAction b);
```

These methods wrapped by an object that implements ```IReducer<TState>```, which can be used to
construct a ```Store<TState>```.

#### How are these Reducers different from original Reducers?
Those who know the original Redux must have asked a question - but why did we *mutate* the application
state in the ```Reduce()``` method?

In fact, this implementation of Redux does not require the state to be *immutable*. Thanks to *reactive extensions*
the Store detects when a state or a substate was replaced by a new instance and notifies everyone who has subscribed
to changes of that particular (sub)state.

In other words, when you change some property of a state in a Reducer and then return the same instance of the state,
the change of that property will be detected and everybody will be notified as expected.

### Effects
Compared to Reducer, that are primarily meant to update the state and be fast, Effects are meant to execute any other activity,
communicate with any part of the system (e.g. database) and cause any side effect (well, from here the name Effect).

Compared to Reducer, *Effect should not change the state* - it is the task of the Reducer to change the state.
If the Effect "feels" that the state should be updated, it simply dispatches a new Action.

Effect does not dispatch Actions directly. Instead, Effects always return an instance of an IAction.
Effect may be any class that implements ```IEffect<TState>```.

An example of an Effect is:
```c#
class AppState
{
    string Name { get; set; }
    string LastName { get; set; }
    DateTime NameUpdatedAt { get; set; }
}

class AppEffect : IEffect<AppState>
{
    DatabaseConnector _db;

    AppEffect(DatabaseConnector db)
    {
        _db = db;
    }

    IAction Execute(AppState state, IAction action)
    {
        if(action is SetUserNameAction)
        {
            var newName = ((SetUserNameAction)action).Name;
            _db.UpdateUserName(newName);
            return new UserNameUpdatedInDbAction(newName);
        }
        if(action is SetUserLastNameAction)
        {
            var newName = ((SetUserLastNameAction)action).LastName;
            _db.UpdateUserLastName(newName);
            return new UserLastNameUpdatedInDbAction(newName);
        }
    }
}
```

After Effect finishes, the returned Action will be dispatched.

Similarly to Reducers, in more complex applications it becomes necessary to decompose Effects into multiple files.
There is again an ```Combine()``` extension method applicable to any ```IEnumerable<IEffectClass<TState>>```.

```c#
class NameEffect : IEffectClass<AppState>
{
    DatabaseConnector _db;

    NameEffect(DatabaseConnector db)
    {
        _db = db;
    }

    public IAction UpdateNameEffect(AppState currentState, SetUserNameAction action)
    {
        _db.UpdateUserName(action.Name);
        return new UserNameUpdatedAction(action.Name);
    }

    public IAction UpdateNameUpdatedAtEffect(AppState currentState, SetUserNameAction action)
    {
        _db.UpdateUserLastName(action.LastName);
        return new UserLastNameUpdatedAction(action.LastName);
    }
}

class LastNameEffect : IEffectClass<AppState>
{
    public IAction UpdateLastNameEffect(AppState currentState, SetUserLastNameAction action)
    {
        Console.WriteLine("Some long task with side effects");
        return NullAction.Instance;
    }
}

\\ now we need to get a single IReducer<TState> class. Combine() comes to aid.
IEnumerable<IEffect<AppState>> combinedEffects = new[]{ new NameEffect(db), new LastNameEffect()}
    .Combine();
```

Have you noticed ```NullAction.Instance``` in the last Effect? Sometimes (often) Effects do their job and
do not require any changes of the state. If so, one can return an instance of ```NullAction``` class.

**Important to note:** Effects are by default executed in a separate thread. It is a an optimization -
Effects are often used to communicate with database, make http requests or do some hard job. If you
want to force execution of Effect methods in sequentially (in the main thread, where also Reducers are executed),
you can implement your Effect methods in a class that implements ```ISequentialEffectClass<TState>```
and not ```IEffectClass<TState>```.

## How to use Redux.WithSubstates in your project

## Why another Redux for .NET?

## FAQs
### What is the main difference between original Redux and Redux.WithSubstates?
Redux.WithSubstates does not:
- Require an immutable state (Reducer can change some part of the state and return the same instance)

Redux.WithSubstates does:
- Execute effects by default in a separate thread as a Task
- Rely heavily on Reactive extensions

## References
This library is inspired by [Dan Abramov's](https://twitter.com/dan_abramov) [Redux](https://github.com/reactjs/redux).

Besides that, we would like to salute the first (as far as we know) author of .NET implementation of Redux, [Guillaume Salles](https://github.com/GuillaumeSalles).