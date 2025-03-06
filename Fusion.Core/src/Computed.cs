using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fusion
{
    /// <summary>
    /// Represents a computed value that automatically updates when its dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of value computed.</typeparam>
    public class Computed<T> : IDependent, IReactive<T>
    {
        private readonly Func<T> _computeFunc;
        private T _cachedValue;
        private bool _isDirty = true;
        private HashSet<IObservable> _dependencies = new HashSet<IObservable>();
        private readonly HashSet<IDependent> _dependents = new HashSet<IDependent>();
        
        /// <summary>
        /// Event that is raised when the computed value changes.
        /// </summary>
        public event EventHandler<T> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Computed{T}"/> class with the specified compute function.
        /// </summary>
        /// <param name="computeFunc">The function used to compute the value.</param>
        public Computed(Func<T> computeFunc)
        {
            _computeFunc = computeFunc ?? throw new ArgumentNullException(nameof(computeFunc));

			// Immediately evaluate the computed value to establish dependencies
    		var _ = Value;
        }

        /// <summary>
        /// Gets the current computed value, recalculating if necessary.
        /// </summary>
        public T Value
        {
            get
            {
                // Track that the current computation depends on this computed value
                DependencyTracker.TrackDependency(this);

                if (_isDirty)
                {
                    // Clear existing dependencies before recalculating
                    ClearDependencies();

                    // Recalculate the value, tracking dependencies
                    var (newDependencies, result) = DependencyTracker.Track(this, _computeFunc);
                    
                    _dependencies = newDependencies;
                    _cachedValue = result;
                    _isDirty = false;
                }

                return _cachedValue;
            }
        }

        /// <summary>
        /// Invalidates the current cached value, causing a recalculation on next access.
        /// </summary>
        public void Invalidate()
        {
            if (!_isDirty)
            {
                _isDirty = true;
                
                // Notify dependents immediately
                NotifyDependents();
                
                // If there are observers, recalculate the value now and raise the event
                if (ValueChanged != null)
                {
                    var oldValue = _cachedValue;
                    var newValue = Value; // This will recalculate
                    
                    if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
                    {
                        ValueChanged?.Invoke(this, newValue);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a dependent to this computed value.
        /// </summary>
        /// <param name="dependent">The dependent to add.</param>
        public void AddDependent(IDependent dependent)
        {
            _dependents.Add(dependent);
        }

        /// <summary>
        /// Removes a dependent from this computed value.
        /// </summary>
        /// <param name="dependent">The dependent to remove.</param>
        public void RemoveDependent(IDependent dependent)
        {
            _dependents.Remove(dependent);
        }

        /// <summary>
        /// Clears all current dependencies.
        /// </summary>
        private void ClearDependencies()
        {
            foreach (var dependency in _dependencies)
            {
                dependency.RemoveDependent(this);
            }
            _dependencies.Clear();
        }

        /// <summary>
        /// Notifies all dependents that this computed value has changed.
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