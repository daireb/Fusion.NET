// Disable parallelization for tests - Reactor is not yet thread-safe!
// TODO make it thread safe and remove this attribute
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]