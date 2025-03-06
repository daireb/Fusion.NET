using System;
using System.Collections.Generic;
using Fusion;

namespace Fusion.Tests
{
    /// <summary>
    /// Helper classes and utilities for testing the Fusion library.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// A mock implementation of IDependent for testing.
        /// </summary>
        public class MockDependent : IDependent
        {
            private readonly Action _invalidateAction;
            
            /// <summary>
            /// Number of times Invalidate has been called.
            /// </summary>
            public int InvalidateCount { get; private set; }

            /// <summary>
            /// Initializes a new instance of the MockDependent class.
            /// </summary>
            /// <param name="invalidateAction">Optional action to execute when Invalidate is called.</param>
            public MockDependent(Action invalidateAction = null)
            {
                _invalidateAction = invalidateAction;
            }

            /// <summary>
            /// Called when this dependent should be invalidated.
            /// </summary>
            public void Invalidate()
            {
                InvalidateCount++;
                _invalidateAction?.Invoke();
            }
        }

        /// <summary>
        /// A mock implementation of IObservable for testing.
        /// </summary>
        public class MockObservable : IObservable
        {
            private readonly HashSet<IDependent> _dependents = new HashSet<IDependent>();
            
            /// <summary>
            /// Collection of dependents that have been added.
            /// </summary>
            public IReadOnlyCollection<IDependent> Dependents => _dependents;
            
            /// <summary>
            /// Number of times dependents have been notified.
            /// </summary>
            public int NotifyCount { get; private set; }

            /// <summary>
            /// Adds a dependent to this observable.
            /// </summary>
            /// <param name="dependent">The dependent to add.</param>
            public void AddDependent(IDependent dependent)
            {
                _dependents.Add(dependent);
            }

            /// <summary>
            /// Removes a dependent from this observable.
            /// </summary>
            /// <param name="dependent">The dependent to remove.</param>
            public void RemoveDependent(IDependent dependent)
            {
                _dependents.Remove(dependent);
            }

            /// <summary>
            /// Notifies all dependents.
            /// </summary>
            public void NotifyAllDependents()
            {
                NotifyCount++;
                foreach (var dependent in _dependents)
                {
                    dependent.Invalidate();
                }
            }
        }

        /// <summary>
        /// A test-specific state implementation that exposes additional metrics.
        /// </summary>
        /// <typeparam name="T">The type of value stored in the state.</typeparam>
        public class TestState<T> : State<T>
        {
            /// <summary>
            /// Number of times the value has been accessed.
            /// </summary>
            public int AccessCount { get; private set; }
            
            /// <summary>
            /// Number of times the value has been set.
            /// </summary>
            public int UpdateCount { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestState class.
            /// </summary>
            /// <param name="initialValue">The initial value.</param>
            public TestState(T initialValue) : base(initialValue)
            {
            }

            /// <summary>
            /// Gets or sets the current value, tracking access and update counts.
            /// </summary>
            public new T Value
            {
                get
                {
                    AccessCount++;
                    return base.Value;
                }
                set
                {
                    UpdateCount++;
                    base.Value = value;
                }
            }
        }

        /// <summary>
        /// A test-specific computed implementation that exposes additional metrics.
        /// </summary>
        /// <typeparam name="T">The type of value computed.</typeparam>
        public class TestComputed<T> : Computed<T>
        {
            private readonly Func<T> _computeFunc;
            
            /// <summary>
            /// Number of times the value has been accessed.
            /// </summary>
            public int AccessCount { get; private set; }
            
            /// <summary>
            /// Number of times the value has been recomputed.
            /// </summary>
            public int ComputeCount { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestComputed class.
            /// </summary>
            /// <param name="computeFunc">The function to compute the value.</param>
            public TestComputed(Func<T> computeFunc) : base(WrapComputeFunc(computeFunc, out var counter))
            {
                _computeFunc = computeFunc;
                ComputeCountRef = counter;
            }

            private ComputeCounter ComputeCountRef { get; }

            /// <summary>
            /// Gets the current computed value, tracking access and computation counts.
            /// </summary>
            public new T Value
            {
                get
                {
                    AccessCount++;
                    ComputeCount = ComputeCountRef.Count;
                    return base.Value;
                }
            }

            private class ComputeCounter
            {
                public int Count { get; set; }
            }

            private static Func<T> WrapComputeFunc(Func<T> original, out ComputeCounter counter)
            {
                var computeCounter = new ComputeCounter();
                counter = computeCounter;
                
                return () => {
                    computeCounter.Count++;
                    return original();
                };
            }
        }

        /// <summary>
        /// Records events that occur during reactive operations.
        /// </summary>
        public class EventRecorder
        {
            private readonly List<string> _events = new List<string>();
            
            /// <summary>
            /// Gets the recorded events.
            /// </summary>
            public IReadOnlyList<string> Events => _events;
            
            /// <summary>
            /// Records an event with the specified name.
            /// </summary>
            /// <param name="name">The name of the event.</param>
            public void RecordEvent(string name)
            {
                _events.Add(name);
            }
            
            /// <summary>
            /// Creates an action that records an event with the specified name.
            /// </summary>
            /// <param name="name">The name of the event.</param>
            /// <returns>An action that records the event.</returns>
            public Action RecordAction(string name)
            {
                return () => RecordEvent(name);
            }
            
            /// <summary>
            /// Creates an action that records an event with the specified name and value.
            /// </summary>
            /// <typeparam name="T">The type of the value.</typeparam>
            /// <param name="name">The name of the event.</param>
            /// <returns>An action that records the event with the value.</returns>
            public Action<T> RecordAction<T>(string name)
            {
                return (value) => RecordEvent($"{name}:{value}");
            }
            
            /// <summary>
            /// Clears all recorded events.
            /// </summary>
            public void Clear()
            {
                _events.Clear();
            }
        }
    }
}