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
        /// Creates a computed value based on the specified reactive value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source value.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="reactive">The source reactive value.</param>
        /// <param name="selector">The selector function to apply to the value.</param>
        /// <returns>A computed value that updates when the source value changes.</returns>
        public static Computed<TResult> Select<TSource, TResult>(this IReactive<TSource> reactive, Func<TSource, TResult> selector)
        {
            return new Computed<TResult>(() => selector(reactive.Value));
        }

        /// <summary>
        /// Creates a computed boolean value that indicates whether the value satisfies a condition.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="reactive">The reactive value.</param>
        /// <param name="predicate">The predicate function to apply to the value.</param>
        /// <returns>A computed boolean value that updates when the reactive value changes.</returns>
        public static Computed<bool> Any<T>(this IReactive<T> reactive, Func<T, bool> predicate)
        {
            return new Computed<bool>(() => predicate(reactive.Value));
        }

        #endregion

        #region Collection Operations

        /// <summary>
        /// Creates a computed collection by applying a transformation to each element of the source collection.
        /// </summary>
        /// <typeparam name="TCollection">The collection type.</typeparam>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result collection.</typeparam>
        /// <param name="reactive">The reactive value containing the source collection.</param>
        /// <param name="selector">The transformation function to apply to each element.</param>
        /// <returns>A computed collection that updates when the source value changes.</returns>
        public static Computed<IEnumerable<TResult>> Select<TCollection, TSource, TResult>(
            this IReactive<TCollection> reactive,
            Func<TSource, TResult> selector)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TResult>>(() => reactive.Value.Select(selector));
        }

        /// <summary>
        /// Creates a computed collection by applying a transformation to each element of the source collection,
        /// using both the element and its index.
        /// </summary>
        /// <typeparam name="TCollection">The collection type.</typeparam>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result collection.</typeparam>
        /// <param name="reactive">The reactive value containing the source collection.</param>
        /// <param name="selector">The transformation function to apply to each element and its index.</param>
        /// <returns>A computed collection that updates when the source value changes.</returns>
        public static Computed<IEnumerable<TResult>> SelectWithIndex<TCollection, TSource, TResult>(
            this IReactive<TCollection> reactive,
            Func<TSource, int, TResult> selector)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TResult>>(() => reactive.Value.Select((item, index) => selector(item, index)));
        }

        /// <summary>
        /// Creates a computed collection containing elements from the source collection that satisfy a condition.
        /// </summary>
        /// <typeparam name="TCollection">The collection type.</typeparam>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <param name="reactive">The reactive value containing the source collection.</param>
        /// <param name="predicate">The function to test each element for a condition.</param>
        /// <returns>A computed collection that updates when the source value changes.</returns>
        public static Computed<IEnumerable<TSource>> Where<TCollection, TSource>(
            this IReactive<TCollection> reactive,
            Func<TSource, bool> predicate)
            where TCollection : IEnumerable<TSource>
        {
            return new Computed<IEnumerable<TSource>>(() => reactive.Value.Where(predicate));
        }

        #endregion

        #region Observation Methods

        /// <summary>
        /// Observes changes to this reactive value, executing the callback when changes occur.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="reactive">The reactive value to observe.</param>
        /// <param name="callback">The callback to execute when the value changes.</param>
        /// <returns>An observer that can be disposed to stop observing.</returns>
        public static Observer Subscribe<T>(this IReactive<T> reactive, Action<T> callback)
        {
            return Observer.Create(reactive, callback);
        }

        #endregion
    }
}