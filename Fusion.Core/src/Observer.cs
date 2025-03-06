using System;

namespace Fusion
{
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
    /// Represents an observer that can subscribe to changes in reactive state.
    /// </summary>
    public class Observer : IDisposable
    {
        private Action _callback;
        private bool _isDisposed;

        private Observer(Action callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Creates an observer that reacts to changes in the specified state.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The state to observe.</param>
        /// <param name="callback">The callback to execute when the state changes.</param>
        /// <returns>An observer that can be disposed to stop observing.</returns>
        public static Observer Create<T>(State<T> state, Action<T> callback)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // Execute callback with initial value
            callback(state.Value);

            // Create observer
            var observer = new Observer(() => callback(state.Value));
            
            // Subscribe to state changes
            EventHandler<T> handler = (sender, value) => observer._callback();
            state.ValueChanged += handler;

            // Store cleanup logic in the Dispose method
            observer._onDispose = () => state.ValueChanged -= handler;
            
            return observer;
        }

        /// <summary>
        /// Creates an observer that reacts to changes in the specified computed value.
        /// </summary>
        /// <typeparam name="T">The type of the computed value.</typeparam>
        /// <param name="computed">The computed value to observe.</param>
        /// <param name="callback">The callback to execute when the computed value changes.</param>
        /// <returns>An observer that can be disposed to stop observing.</returns>
        public static Observer Create<T>(Computed<T> computed, Action<T> callback)
        {
            if (computed == null) throw new ArgumentNullException(nameof(computed));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // Execute callback with initial value
            callback(computed.Value);

            // Create observer
            var observer = new Observer(() => callback(computed.Value));
            
            // Subscribe to computed value changes
            EventHandler<T> handler = (sender, value) => observer._callback();
            computed.ValueChanged += handler;

            // Store cleanup logic in the Dispose method
            observer._onDispose = () => computed.ValueChanged -= handler;
            
            return observer;
        }

        private Action _onDispose;

        /// <summary>
        /// Stops observing the state or computed value.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _onDispose?.Invoke();
                _callback = null;
                _onDispose = null;
                _isDisposed = true;
            }
        }
    }
}