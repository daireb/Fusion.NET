using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Fusion;

namespace Fusion.Tests
{
    public class ReactiveExtensionsTests
    {
        #region Value Transformations Tests

        [Fact]
        public void Select_FromState_TransformsValue()
        {
            // Arrange
            var state = new State<int>(2);
            
            // Act
            var doubled = state.Select(x => x * 2);
            
            // Assert
            Assert.Equal(4, doubled.Value);
        }

        [Fact]
        public void Select_FromState_UpdatesWhenStateChanges()
        {
            // Arrange
            var state = new State<int>(2);
            var doubled = state.Select(x => x * 2);
            
            // Act
            state.Value = 3;
            
            // Assert
            Assert.Equal(6, doubled.Value);
        }

        [Fact]
        public void Select_FromComputed_TransformsValue()
        {
            // Arrange
            var state = new State<int>(2);
            var doubled = state.Select(x => x * 2);
            
            // Act
            var quadrupled = doubled.Select(x => x * 2);
            
            // Assert
            Assert.Equal(8, quadrupled.Value);
        }

        [Fact]
        public void Select_FromComputed_UpdatesWhenComputedChanges()
        {
            // Arrange
            var state = new State<int>(2);
            var doubled = state.Select(x => x * 2);
            var quadrupled = doubled.Select(x => x * 2);
            
            // Act
            state.Value = 3;
            
            // Assert
            Assert.Equal(12, quadrupled.Value);
        }

        [Fact]
        public void Any_FromState_AppliesPredicate()
        {
            // Arrange
            var state = new State<int>(5);
            
            // Act
            var isGreaterThan3 = state.Any(x => x > 3);
            var isEven = state.Any(x => x % 2 == 0);
            
            // Assert
            Assert.True(isGreaterThan3.Value);
            Assert.False(isEven.Value);
        }

        [Fact]
        public void Any_FromState_UpdatesWhenStateChanges()
        {
            // Arrange
            var state = new State<int>(5);
            var isEven = state.Any(x => x % 2 == 0);
            
            // Act
            state.Value = 4;
            
            // Assert
            Assert.True(isEven.Value);
        }

        [Fact]
        public void Any_FromComputed_AppliesPredicate()
        {
            // Arrange
            var state = new State<int>(5);
            var doubled = state.Select(x => x * 2);
            
            // Act
            var isGreaterThan15 = doubled.Any(x => x > 15);
            var isDivisibleBy4 = doubled.Any(x => x % 4 == 0);
            
            // Assert
            Assert.False(isGreaterThan15.Value);
            Assert.False(isDivisibleBy4.Value);
        }

        [Fact]
        public void Any_FromComputed_UpdatesWhenComputedChanges()
        {
            // Arrange
            var state = new State<int>(5);
            var doubled = state.Select(x => x * 2);
            var isGreaterThan15 = doubled.Any(x => x > 15);
            
            // Act
            state.Value = 8;
            
            // Assert
            Assert.True(isGreaterThan15.Value);
        }

        #endregion

        #region Value Combinations Tests

        [Fact]
        public void Combine_MultipleStates_CombinesValues()
        {
            // Arrange
            var a = new State<int>(1);
            var b = new State<int>(2);
            var c = new State<int>(3);
            
            // Act
            var combined = ReactiveExtensions.Combine(values => 
                (int)values[0] + (int)values[1] + (int)values[2], a, b, c);
            
            // Assert
            Assert.Equal(6, combined.Value); // 1 + 2 + 3 = 6
        }

        [Fact]
        public void Combine_MultipleStates_UpdatesWhenAnyStateChanges()
        {
            // Arrange
            var a = new State<int>(1);
            var b = new State<int>(2);
            var c = new State<int>(3);
            
            var combined = ReactiveExtensions.Combine(values => 
                (int)values[0] + (int)values[1] + (int)values[2], a, b, c);
            
            // Act & Assert
            a.Value = 10;
            Assert.Equal(15, combined.Value); // 10 + 2 + 3 = 15
            
            b.Value = 20;
            Assert.Equal(33, combined.Value); // 10 + 20 + 3 = 33
            
            c.Value = 30;
            Assert.Equal(60, combined.Value); // 10 + 20 + 30 = 60
        }

        #endregion

        #region Collection Operations Tests

        [Fact]
        public void Select_OnStateCollection_TransformsEachItem()
        {
            // Arrange
            var numbers = new State<List<int>>(new List<int> { 1, 2, 3 });
            
            // Act
            var doubled = numbers.Select<List<int>, int, int>(n => n * 2);
            
            // Assert
            Assert.Equal(new[] { 2, 4, 6 }, doubled.Value);
        }

        [Fact]
        public void Select_OnStateCollection_UpdatesWhenCollectionChanges()
        {
            // Arrange
            var numbers = new State<List<int>>(new List<int> { 1, 2, 3 });
            var doubled = numbers.Select<List<int>, int, int>(n => n * 2);
            
            // Act
            numbers.Value = new List<int> { 4, 5, 6 };
            
            // Assert
            Assert.Equal(new[] { 8, 10, 12 }, doubled.Value);
        }

        [Fact]
        public void Select_OnComputedCollection_TransformsEachItem()
        {
            // Arrange
            var numbers = new State<List<int>>(new List<int> { 1, 2, 3 });
            var computed = new Computed<List<int>>(() => numbers.Value);
            
            // Act
            var doubled = computed.Select<List<int>, int, int>(n => n * 2);
            
            // Assert
            Assert.Equal(new[] { 2, 4, 6 }, doubled.Value);
        }

        [Fact]
        public void SelectWithIndex_OnStateCollection_TransformsWithIndex()
        {
            // Arrange
            var letters = new State<List<string>>(new List<string> { "a", "b", "c" });
            
            // Act
            var indexed = letters.SelectWithIndex<List<string>, string, string>((letter, index) => $"{index}:{letter}");
            
            // Assert
            Assert.Equal(new[] { "0:a", "1:b", "2:c" }, indexed.Value);
        }

        [Fact]
        public void Where_OnStateCollection_FiltersItems()
        {
            // Arrange
            var numbers = new State<List<int>>(new List<int> { 1, 2, 3, 4, 5 });
            
            // Act
            var evens = numbers.Where<List<int>, int>(n => n % 2 == 0);
            
            // Assert
            Assert.Equal(new[] { 2, 4 }, evens.Value);
        }

        [Fact]
        public void Where_OnStateCollection_UpdatesWhenCollectionChanges()
        {
            // Arrange
            var numbers = new State<List<int>>(new List<int> { 1, 2, 3, 4, 5 });
            var evens = numbers.Where<List<int>, int>(n => n % 2 == 0);
            
            // Act
            numbers.Value = new List<int> { 6, 7, 8, 9, 10 };
            
            // Assert
            Assert.Equal(new[] { 6, 8, 10 }, evens.Value);
        }

        [Fact]
        public void Where_OnComputedCollection_FiltersItems()
        {
            // Arrange
            var numbers = new State<List<int>>(new List<int> { 1, 2, 3, 4, 5 });
            var computed = new Computed<List<int>>(() => numbers.Value);
            
            // Act
            var evens = computed.Where<List<int>, int>(n => n % 2 == 0);
            
            // Assert
            Assert.Equal(new[] { 2, 4 }, evens.Value);
        }

        #endregion

        #region Observation Methods Tests

        [Fact]
        public void Subscribe_OnState_CreatesObserver()
        {
            // Arrange
            var state = new State<int>(1);
            int callbackValue = 0;
            
            // Act
            using (var observer = state.Subscribe(value => callbackValue = value))
            {
                // Assert initial value was received
                Assert.Equal(1, callbackValue);
                
                // Act - change state
                state.Value = 42;
                
                // Assert callback was invoked
                Assert.Equal(42, callbackValue);
            }
            
            // Observer was disposed, change should not affect callbackValue
            state.Value = 99;
            Assert.Equal(42, callbackValue);
        }

        [Fact]
        public void Subscribe_OnComputed_CreatesObserver()
        {
            // Arrange
            var state = new State<int>(1);
            var computed = state.Select(x => x * 2);
            int callbackValue = 0;
            
            // Act
            using (var observer = computed.Subscribe(value => callbackValue = value))
            {
                // Assert initial value was received
                Assert.Equal(2, callbackValue);
                
                // Act - change state
                state.Value = 10;
                
                // Assert callback was invoked with computed value
                Assert.Equal(20, callbackValue);
            }
            
            // Observer was disposed, change should not affect callbackValue
            state.Value = 50;
            Assert.Equal(20, callbackValue);
        }

        #endregion
    }
}