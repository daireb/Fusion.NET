using System;
using System.Collections.Generic;

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
    /// Manages batch updates to reactive state, delaying notifications until the batch is complete.
    /// </summary>
    public class BatchUpdate : IDisposable
    {
        private static BatchUpdate CurrentBatch = null;
            
        private readonly HashSet<INotifiable> _modifiedStates = new HashSet<INotifiable>();
        private bool _isDisposed;
        
        /// <summary>
        /// Starts a new batch update.
        /// </summary>
        /// <returns>A BatchUpdate object that should be disposed to complete the batch.</returns>
        public static BatchUpdate Start()
        {
            var batch = new BatchUpdate();
            CurrentBatch = batch;
            return batch;
        }
        
        /// <summary>
        /// Executes the specified action within a batch update.
        /// </summary>
        /// <param name="action">The action to execute as a batch.</param>
        public static void Execute(Action action)
        {
            using (var batch = Start())
            {
                action();
            }
        }
        
        /// <summary>
        /// Gets whether the current thread is executing within a batch update.
        /// </summary>
        public static bool IsInBatchUpdate => CurrentBatch != null;
        
        /// <summary>
        /// Gets the current batch update for this thread.
        /// </summary>
        public static BatchUpdate Current => CurrentBatch;
        
        /// <summary>
        /// Registers a state as modified during this batch.
        /// </summary>
        /// <param name="state">The state that was modified.</param>
        public void RegisterModifiedState(INotifiable state)
        {
            _modifiedStates.Add(state);
        }
        
        /// <summary>
        /// Completes the batch and notifies all dependents of modified states.
        /// </summary>
        public void Complete()
        {
            if (_isDisposed) return;
            
            foreach (var state in _modifiedStates)
            {
                state.NotifyDependents();
            }
            
            _modifiedStates.Clear();
            _isDisposed = true;
            CurrentBatch = null;
        }
        
        /// <summary>
        /// Disposes the batch update, completing it if not already completed.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Complete();
            }
        }
    }
}