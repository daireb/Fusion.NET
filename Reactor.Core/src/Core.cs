using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactor
{
    /// <summary>
    /// Interface for objects that depend on observables.
    /// </summary>
    public interface IDependent
    {
        /// <summary>
        /// Invalidates the current value, causing a recalculation when next accessed.
        /// </summary>
        void Invalidate();
    }

    /// <summary>
    /// Interface for objects that can notify dependents of changes.
    /// </summary>
    public interface IObservable
    {
        /// <summary>
        /// Adds a dependent to this observable.
        /// </summary>
        /// <param name="dependent">The dependent to add.</param>
        void AddDependent(IDependent dependent);

        /// <summary>
        /// Removes a dependent from this observable.
        /// </summary>
        /// <param name="dependent">The dependent to remove.</param>
        void RemoveDependent(IDependent dependent);
    }

    /// <summary>
    /// Interface for objects that can notify their dependents of changes.
    /// </summary>
    public interface INotifiable
    {
        /// <summary>
        /// Notifies all dependents that this object has changed.
        /// </summary>
        void NotifyDependents();
    }

    /// <summary>
    /// Represents a reactive value that can be observed and notifies dependents when its value changes.
    /// </summary>
    /// <typeparam name="T">The type of value contained.</typeparam>
    public interface IReactive<T> : IObservable, INotifiable
    {
        /// <summary>
        /// Gets the current value.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Event that is raised when the value changes.
        /// </summary>
        event EventHandler<T> ValueChanged;
    }
    
    /// <summary>
    /// Static class for tracking dependencies during computation.
    /// </summary>
    public static class DependencyTracker
    {
        private static readonly Stack<IDependent> DependentStack = new Stack<IDependent>();
        private static readonly HashSet<IObservable> CurrentDependencies = new HashSet<IObservable>();
        
        /// <summary>
        /// Gets the current dependent being tracked, if any.
        /// </summary>
        public static IDependent CurrentDependent => 
            DependentStack.Count > 0 ? DependentStack.Peek() : null;

        /// <summary>
        /// Executes the specified function with dependency tracking.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the function.</typeparam>
        /// <param name="dependent">The dependent to use as the current computation.</param>
        /// <param name="func">The function to execute.</param>
        /// <returns>The dependencies that were accessed and the result of the function.</returns>
        public static (HashSet<IObservable> Dependencies, T Result) Track<T>(IDependent dependent, Func<T> func)
        {
            DependentStack.Push(dependent);
            CurrentDependencies.Clear();
            
            try
            {
                T result = func();
                return (new HashSet<IObservable>(CurrentDependencies), result);
            }
            finally
            {
                DependentStack.Pop();
                CurrentDependencies.Clear();
            }
        }

        /// <summary>
        /// Tracks that the current computation depends on the specified observable.
        /// </summary>
        /// <param name="observable">The observable to track.</param>
        public static void TrackDependency(IObservable observable)
        {
            if (DependentStack.Count > 0)
            {
                CurrentDependencies.Add(observable);
                observable.AddDependent(CurrentDependent);
            }
        }
    }
}