using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reactor
{
    /// <summary>
    /// Represents a reactive state container that notifies dependents when its value changes.
    /// </summary>
    /// <typeparam name="T">The type of value stored in the state.</typeparam>
    public class State<T> : IReactive<T>
    {
        private T _value;
        private readonly HashSet<IDependent> _dependents = new HashSet<IDependent>();
        
        /// <summary>
        /// Event that is raised when the value of the state changes.
        /// </summary>
        public event EventHandler<T> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="State{T}"/> class with the specified initial value.
        /// </summary>
        /// <param name="initialValue">The initial value of the state.</param>
        public State(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Gets or sets the current value of the state.
        /// Setting a new value will notify all dependents if the value has changed,
        /// unless this update is part of a batch operation.
        /// </summary>
        public T Value
        {
            get
            {
                // Track that the current computation depends on this state
                DependencyTracker.TrackDependency(this);
                return _value;
            }
            set
            {
                // Check for value equality to avoid unnecessary updates
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    
                    // Notify dependents immediately
                    NotifyDependents();
                    
                    // Always trigger the ValueChanged event immediately
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        /// <summary>
        /// Adds a dependent to this state.
        /// </summary>
        /// <param name="dependent">The dependent to add.</param>
        public void AddDependent(IDependent dependent)
        {
            _dependents.Add(dependent);
        }

        /// <summary>
        /// Removes a dependent from this state.
        /// </summary>
        /// <param name="dependent">The dependent to remove.</param>
        public void RemoveDependent(IDependent dependent)
        {
            _dependents.Remove(dependent);
        }

        /// <summary>
        /// Notifies all dependents that this state has changed.
        /// </summary>
        public void NotifyDependents()
        {
            foreach (var dependent in _dependents.ToList())
            {
                dependent.Invalidate();
            }
        }
    }
}