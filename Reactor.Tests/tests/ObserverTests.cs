using System;
using System.Collections.Generic;
using Xunit;
using Reactor;

namespace Reactor.Tests
{
    public class ObserverTests
    {
        [Fact]
        public void Create_WithState_ExecutesCallbackImmediately()
        {
            // Arrange
            var state = new State<int>(42);
            int callbackValue = 0;
            
            // Act
            var observer = Observer.Create(state, value => callbackValue = value);
            
            // Assert
            Assert.Equal(42, callbackValue);
        }

        [Fact]
        public void Create_WithComputed_ExecutesCallbackImmediately()
        {
            // Arrange
            var computed = new Computed<string>(() => "test");
            string callbackValue = null;
            
            // Act
            var observer = Observer.Create(computed, value => callbackValue = value);
            
            // Assert
            Assert.Equal("test", callbackValue);
        }

        [Fact]
        public void Create_WithState_ExecutesCallbackOnValueChange()
        {
            // Arrange
            var state = new State<int>(1);
            int callbackCount = 0;
            int lastValue = 0;
            
            var observer = Observer.Create(state, value => {
                callbackCount++;
                lastValue = value;
            });
            
            // Initial callback already happened, so reset count
            callbackCount = 0;
            
            // Act
            state.Value = 2;
            
            // Assert
            Assert.Equal(1, callbackCount);
            Assert.Equal(2, lastValue);
        }

        [Fact]
        public void Create_WithComputed_ExecutesCallbackOnValueChange()
        {
            // Arrange
            var state = new State<int>(1);
            var computed = new Computed<int>(() => state.Value * 2);
            
            int callbackCount = 0;
            int lastValue = 0;
            
            var observer = Observer.Create(computed, value => {
                callbackCount++;
                lastValue = value;
            });
            
            // Initial callback already happened, so reset count
            callbackCount = 0;
            
            // Act
            state.Value = 2;
            
            // Assert
            Assert.Equal(1, callbackCount);
            Assert.Equal(4, lastValue);
        }

        [Fact]
        public void Create_WithState_DoesNotExecuteCallbackForSameValue()
        {
            // Arrange
            var state = new State<string>("test");
            int callbackCount = 0;
            
            var observer = Observer.Create(state, _ => callbackCount++);
            
            // Initial callback already happened, so reset count
            callbackCount = 0;
            
            // Act
            state.Value = "test"; // Same value
            
            // Assert
            Assert.Equal(0, callbackCount);
        }

        [Fact]
        public void Create_WithComputed_DoesNotExecuteCallbackForSameComputedValue()
        {
            // Arrange
            var state = new State<string>("TEST");
            var computed = new Computed<string>(() => state.Value.ToUpper());
            
            int callbackCount = 0;
            
            var observer = Observer.Create(computed, _ => callbackCount++);
            
            // Initial callback already happened, so reset count
            callbackCount = 0;
            
            // Act
            state.Value = "test"; // Different value, but same computed result
            
            // Assert
            Assert.Equal(0, callbackCount);
        }

        [Fact]
        public void Dispose_StopsObservingState()
        {
            // Arrange
            var state = new State<int>(1);
            int callbackCount = 0;
            
            var observer = Observer.Create(state, _ => callbackCount++);
            
            // Initial callback already happened, so reset count
            callbackCount = 0;
            
            // Act
            observer.Dispose();
            state.Value = 2;
            
            // Assert
            Assert.Equal(0, callbackCount);
        }

        [Fact]
        public void Dispose_StopsObservingComputed()
        {
            // Arrange
            var state = new State<int>(1);
            var computed = new Computed<int>(() => state.Value * 2);
            
            int callbackCount = 0;
            
            var observer = Observer.Create(computed, _ => callbackCount++);
            
            // Initial callback already happened, so reset count
            callbackCount = 0;
            
            // Act
            observer.Dispose();
            state.Value = 2;
            
            // Assert
            Assert.Equal(0, callbackCount);
        }

        [Fact]
        public void MultipleObservers_CanObserveSameState()
        {
            // Arrange
            var state = new State<int>(1);
            
            int firstObserverCount = 0;
            int secondObserverCount = 0;
            
            var observer1 = Observer.Create(state, _ => firstObserverCount++);
            var observer2 = Observer.Create(state, _ => secondObserverCount++);
            
            // Initial callbacks already happened, so reset counts
            firstObserverCount = 0;
            secondObserverCount = 0;
            
            // Act
            state.Value = 2;
            
            // Assert
            Assert.Equal(1, firstObserverCount);
            Assert.Equal(1, secondObserverCount);
        }

        [Fact]
        public void DisposingOneObserver_DoesNotAffectOthers()
        {
            // Arrange
            var state = new State<int>(1);
            
            int firstObserverCount = 0;
            int secondObserverCount = 0;
            
            var observer1 = Observer.Create(state, _ => firstObserverCount++);
            var observer2 = Observer.Create(state, _ => secondObserverCount++);
            
            // Initial callbacks already happened, so reset counts
            firstObserverCount = 0;
            secondObserverCount = 0;
            
            // Act
            observer1.Dispose();
            state.Value = 2;
            
            // Assert
            Assert.Equal(0, firstObserverCount);
            Assert.Equal(1, secondObserverCount);
        }
    }
}