using System;
using System.Collections.Generic;
using System.Linq;

namespace Fusion
{
    /// <summary>
    /// Contains extension methods for working with reactive state.
    /// </summary>
    public static class ReactiveExtensions
    {
        #region Value Transformations

        /// <summary>
        /// Creates a computed value based on the specified state.
        /// </summary>
        /// <typeparam name="TSource">The type of the source state.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="state">The source state.</param>
        /// <param name="selector">The selector function to apply to the state value.</param>
        /// <returns>A computed value that updates when the source state changes.</returns>
        public static Computed<TResult> Select<TSource, TResult>(this State<TSource> state, Func<TSource, TResult> selector)
        {
            return new Computed<TResult>(() => selector(state.Value));
        }

        /// <summary>
        /// Creates a computed value based on the specified computed value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source computed value.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="computed">The source computed value.</param>
        /// <param name="selector">The selector function to apply to the computed value.</param>
        /// <returns>A computed value that updates when the source computed value changes.</returns>
        public static Computed<TResult> Select<TSource, TResult>(this Computed<TSource> computed, Func<TSource, TResult> selector)
        {
            return new Computed<TResult>(() => selector(computed.Value));
        }

        /// <summary>
        /// Creates a computed boolean value that indicates whether the value of the state satisfies a condition.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The state.</param>
        /// <param name="predicate">The predicate function to apply to the state value.</param>
        /// <returns>A computed boolean value that updates when the state changes.</returns>
        public static Computed<bool> Any<T>(this State<T> state, Func<T, bool> predicate)
        {
            return new Computed<bool>(() => predicate(state.Value));
        }

        /// <summary>
        /// Creates a computed boolean value that indicates whether the value of the computed value satisfies a condition.
        /// </summary>
        /// <typeparam name="T">The type of the computed value.</typeparam>
        /// <param name="computed">The computed value.</param>
        /// <param name="predicate">The predicate function to apply to the computed value.</param>
        /// <returns>A computed boolean value that updates when the computed value changes.</returns>
        public static Computed<bool> Any<T>(this Computed<T> computed, Func<T, bool> predicate)
        {
            return new Computed<bool>(() => predicate(computed.Value));
        }

        #endregion

        #region Collection Operations

        /// <summary>
        /// Creates a computed collection by applying a transformation to each element of the source collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result collection.</typeparam>
        /// <param name="state">The state containing the source collection.</param>
        /// <param name="selector">The transformation function to apply to each element.</param>
        /// <returns>A computed collection that updates when the source state changes.</returns>
        public static Computed<IEnumerable<TResult>> Select<TCollection, TSource, TResult>(
            this State<TCollection> state,
            Func<TSource, TResult> selector)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TResult>>(() => state.Value.Select(selector));
        }

        /// <summary>
        /// Creates a computed collection by applying a transformation to each element of the source collection
        /// from a computed value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result collection.</typeparam>
        /// <param name="computed">The computed value containing the source collection.</param>
        /// <param name="selector">The transformation function to apply to each element.</param>
        /// <returns>A computed collection that updates when the source computed value changes.</returns>
        public static Computed<IEnumerable<TResult>> Select<TCollection, TSource, TResult>(
            this Computed<TCollection> computed,
            Func<TSource, TResult> selector)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TResult>>(() => computed.Value.Select(selector));
        }

        /// <summary>
        /// Creates a computed collection by applying a transformation to each element of the source collection,
        /// using both the element and its index.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result collection.</typeparam>
        /// <param name="state">The state containing the source collection.</param>
        /// <param name="selector">The transformation function to apply to each element and its index.</param>
        /// <returns>A computed collection that updates when the source state changes.</returns>
        public static Computed<IEnumerable<TResult>> SelectWithIndex<TCollection, TSource, TResult>(
            this State<TCollection> state,
            Func<TSource, int, TResult> selector)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TResult>>(() => state.Value.Select((item, index) => selector(item, index)));
        }

        /// <summary>
        /// Creates a computed collection containing elements from the source collection that satisfy a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <param name="state">The state containing the source collection.</param>
        /// <param name="predicate">The function to test each element for a condition.</param>
        /// <returns>A computed collection that updates when the source state changes.</returns>
        public static Computed<IEnumerable<TSource>> Where<TCollection, TSource>(
            this State<TCollection> state,
            Func<TSource, bool> predicate)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TSource>>(() => state.Value.Where(predicate));
        }

        /// <summary>
        /// Creates a computed collection containing elements from the source collection that satisfy a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <param name="computed">The computed value containing the source collection.</param>
        /// <param name="predicate">The function to test each element for a condition.</param>
        /// <returns>A computed collection that updates when the source computed value changes.</returns>
        public static Computed<IEnumerable<TSource>> Where<TCollection, TSource>(
            this Computed<TCollection> computed,
            Func<TSource, bool> predicate)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TSource>>(() => computed.Value.Where(predicate));
        }

        #endregion

        #region Observation Methods

        /// <summary>
        /// Observes changes to this state, executing the callback when changes occur.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The state to observe.</param>
        /// <param name="callback">The callback to execute when the state changes.</param>
        /// <returns>An observer that can be disposed to stop observing.</returns>
        public static Observer Subscribe<T>(this State<T> state, Action<T> callback)
        {
            return Observer.Create(state, callback);
        }

        /// <summary>
        /// Observes changes to this computed value, executing the callback when changes occur.
        /// </summary>
        /// <typeparam name="T">The type of the computed value.</typeparam>
        /// <param name="computed">The computed value to observe.</param>
        /// <param name="callback">The callback to execute when the computed value changes.</param>
        /// <returns>An observer that can be disposed to stop observing.</returns>
        public static Observer Subscribe<T>(this Computed<T> computed, Action<T> callback)
        {
            return Observer.Create(computed, callback);
        }

        #endregion
    }
}