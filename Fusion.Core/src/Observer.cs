using System;

namespace Fusion
{
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
        /// Creates an observer that reacts to changes in the specified reactive value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="reactive">The reactive value to observe.</param>
        /// <param name="callback">The callback to execute when the value changes.</param>
        /// <returns>An observer that can be disposed to stop observing.</returns>
        public static Observer Create<T>(IReactive<T> reactive, Action<T> callback)
        {
            if (reactive == null) throw new ArgumentNullException(nameof(reactive));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // Execute callback with initial value
            callback(reactive.Value);

            // Create observer
            var observer = new Observer(() => callback(reactive.Value));
            
            // Subscribe to value changes
            EventHandler<T> handler = (sender, value) => observer._callback();
            reactive.ValueChanged += handler;

            // Store cleanup logic in the Dispose method
            observer._onDispose = () => reactive.ValueChanged -= handler;
            
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