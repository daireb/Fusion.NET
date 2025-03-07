using System;
using System.Collections.Generic;
using Xunit;
using Reactor;

namespace Reactor.Tests
{
    public class StateTests
    {
        [Fact]
        public void Constructor_SetsInitialValue()
        {
            // Arrange
            int initialValue = 42;

            // Act
            var state = new State<int>(initialValue);

            // Assert
            Assert.Equal(initialValue, state.Value);
        }

        [Fact]
        public void Value_WhenSet_NotifiesDependents()
        {
            // Arrange
            var state = new State<int>(0);
            bool wasNotified = false;
            var dependent = new MockDependent(() => wasNotified = true);
            state.AddDependent(dependent);

            // Act
            state.Value = 42;

            // Assert
            Assert.True(wasNotified);
        }

        [Fact]
        public void Value_WhenSetToSameValue_DoesNotNotifyDependents()
        {
            // Arrange
            var state = new State<int>(42);
            bool wasNotified = false;
            var dependent = new MockDependent(() => wasNotified = true);
            state.AddDependent(dependent);

            // Act
            state.Value = 42; // Same value

            // Assert
            Assert.False(wasNotified);
        }

        [Fact]
        public void Value_WhenChanged_TriggersValueChangedEvent()
        {
            // Arrange
            var state = new State<int>(0);
            int newValue = 0;
            state.ValueChanged += (sender, value) => newValue = value;

            // Act
            state.Value = 42;

            // Assert
            Assert.Equal(42, newValue);
        }

        [Fact]
        public void AddDependent_AddsToNotificationList()
        {
            // Arrange
            var state = new State<string>("initial");
            bool wasNotified = false;
            var dependent = new MockDependent(() => wasNotified = true);

            // Act
            state.AddDependent(dependent);
            state.Value = "updated";

            // Assert
            Assert.True(wasNotified);
        }

        [Fact]
        public void RemoveDependent_RemovesFromNotificationList()
        {
            // Arrange
            var state = new State<string>("initial");
            bool wasNotified = false;
            var dependent = new MockDependent(() => wasNotified = true);
            state.AddDependent(dependent);

            // Act
            state.RemoveDependent(dependent);
            state.Value = "updated";

            // Assert
            Assert.False(wasNotified);
        }

        [Fact]
        public void NotifyDependents_NotifiesAllDependents()
        {
            // Arrange
            var state = new State<int>(0);
            bool firstNotified = false;
            bool secondNotified = false;
            var dependent1 = new MockDependent(() => firstNotified = true);
            var dependent2 = new MockDependent(() => secondNotified = true);
            state.AddDependent(dependent1);
            state.AddDependent(dependent2);

            // Act
            state.NotifyDependents();

            // Assert
            Assert.True(firstNotified);
            Assert.True(secondNotified);
        }

        [Fact]
        public void Value_GetterCreatesTrackedDependency()
        {
            // Arrange
            var state = new State<int>(0);
            var dependent = new MockDependent(() => { });

            // Act
            var (dependencies, _) = DependencyTracker.Track(dependent, () => state.Value);

            // Assert
            Assert.Contains(state, dependencies);
        }

        // Helper class for testing
        private class MockDependent : IDependent
        {
            private readonly Action _invalidateAction;

            public MockDependent(Action invalidateAction)
            {
                _invalidateAction = invalidateAction;
            }

            public void Invalidate()
            {
                _invalidateAction();
            }
        }
    }
}