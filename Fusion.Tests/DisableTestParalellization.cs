// Disable parallelization for tests - Fusion.NET is not thread-safe!
// TODO make it thread safe and remove this attribute
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]