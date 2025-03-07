using System;
using System.Collections.Generic;
using Xunit;
using Reactor;

namespace Reactor.Tests
{
    public class ComputedTests
    {
        [Fact]
        public void Constructor_ComputesInitialValue()
        {
            // Arrange & Act
            var computed = new Computed<int>(() => 42);

            // Assert
            Assert.Equal(42, computed.Value);
        }

        [Fact]
        public void Value_TracksDependent()
        {
            // Arrange
            var state = new State<int>(1);
            var computed = new Computed<int>(() => state.Value * 2);

            // Act - accessing Value should establish dependencies
            var result = computed.Value;

            // Assert - changing state should invalidate computed
            bool wasInvalidated = false;
            computed.ValueChanged += (sender, value) => wasInvalidated = true;
            state.Value = 2;

            Assert.True(wasInvalidated);
            Assert.Equal(4, computed.Value);
        }

        [Fact]
        public void Value_CachesComputedResult()
        {
            // Arrange
            int computeCount = 0;
            var computed = new Computed<int>(() => {
                computeCount++;
                return 42;
            });

            // Act
            var result1 = computed.Value;
            var result2 = computed.Value;

            // Assert
            Assert.Equal(1, computeCount); // Function should only be called once
            Assert.Equal(42, result1);
            Assert.Equal(42, result2);
        }

        [Fact]
        public void Value_RecomputesWhenDependenciesChange()
        {
            // Arrange
            var state = new State<int>(1);
            var computed = new Computed<int>(() => state.Value * 2);
            
            // Initial access to establish dependencies
            var initialValue = computed.Value;

            // Act
            state.Value = 5;

            // Assert
            Assert.Equal(10, computed.Value);
        }

        [Fact]
        public void Value_TracksNestedDependencies()
        {
            // Arrange
            var state = new State<int>(1);
            var intermediateComputed = new Computed<int>(() => state.Value * 2);
            var finalComputed = new Computed<int>(() => intermediateComputed.Value + 5);
            
            // Initial access to establish dependencies
            var initialValue = finalComputed.Value;

            // Act
            state.Value = 3;

            // Assert
            Assert.Equal(11, finalComputed.Value); // (3 * 2) + 5 = 11
        }

        [Fact]
        public void Value_UpdatesDynamicDependencies()
        {
            // Arrange
            var toggleState = new State<bool>(true);
            var stateA = new State<int>(1);
            var stateB = new State<int>(10);
            
            var computed = new Computed<int>(() => toggleState.Value ? stateA.Value : stateB.Value);
            
            // Initial access to establish dependencies with stateA
            var initialValue = computed.Value;
            Assert.Equal(1, initialValue);

            // Act 1: Change dependency that should affect the result
            stateA.Value = 5;
            
            // Assert 1
            Assert.Equal(5, computed.Value);

            // Act 2: Change to use stateB instead
            toggleState.Value = false;
            
            // Assert 2
            Assert.Equal(10, computed.Value);

            // Act 3: Change stateA, which should no longer affect the result
            stateA.Value = 20;
            
            // Assert 3
            Assert.Equal(10, computed.Value);

            // Act 4: Change stateB, which should now affect the result
            stateB.Value = 30;
            
            // Assert 4
            Assert.Equal(30, computed.Value);
        }

        [Fact]
        public void Invalidate_TriggersRecomputation()
        {
            // Arrange
            int computeCount = 0;
            var computed = new Computed<int>(() => {
                computeCount++;
                return 42;
            });
            
            // Initial access
            var initialValue = computed.Value;
            Assert.Equal(1, computeCount);

            // Act
            computed.Invalidate();
            var newValue = computed.Value;

            // Assert
            Assert.Equal(2, computeCount);
        }

        [Fact]
        public void ValueChanged_FiresWhenValueChanges()
        {
            // Arrange
            var state = new State<int>(1);
            var computed = new Computed<int>(() => state.Value * 2);
            
            int newValue = 0;
            computed.ValueChanged += (sender, value) => newValue = value;
            
            // Initial access to establish dependencies
            var initialValue = computed.Value;

            // Act
            state.Value = 5;

            // Assert
            Assert.Equal(10, newValue);
        }

        [Fact]
        public void ValueChanged_DoesNotFireForSameValue()
        {
            // Arrange
            var state = new State<string>("test");
            var computed = new Computed<string>(() => state.Value.ToUpper());
            
            int eventCount = 0;
            computed.ValueChanged += (sender, value) => eventCount++;
            
            // Initial access to establish dependencies
            var initialValue = computed.Value;
            Assert.Equal("TEST", initialValue);
            Assert.Equal(0, eventCount); // Initial computation doesn't fire the event

            // Act
            state.Value = "TEST"; // This will compute to "TEST" again

            // Assert
            Assert.Equal(0, eventCount); // No change, so no event
        }
    }
}