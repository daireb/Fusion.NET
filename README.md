# Fusion.NET

Fusion.NET is a lightweight, reactive state management library for .NET applications. It provides a simple and elegant way to create reactive applications with automatic dependency tracking and state propagation.

Fusion.NET focuses on being user-friendly and reducing boilerplate code rather than prioritizing raw performance. It's inspired by [Fusion on Roblox](https://elttob.uk/Fusion/), created by [elttob](https://github.com/Elttob).

## Philosophy

Fusion.NET prioritizes developer experience and code readability over raw performance. Its design goals include:

- **Minimal Boilerplate**: Write reactive code with minimal ceremony
- **Intuitive API**: Clear, consistent and discoverable methods and properties
- **Predictable Behavior**: Reactive updates follow an understandable pattern
- **Composability**: Reactive elements can be easily combined and transformed

While Fusion.NET is designed to be performant enough for most applications, it makes trade-offs favoring developer experience rather than maximum performance. For extremely performance-critical applications, you might need a more specialized solution.

## Features

- **Reactive State Containers**: Create observable state that automatically notifies dependents when values change
- **Computed Values**: Define values that are derived from other state and automatically update when dependencies change
- **Dependency Tracking**: Automatic tracking of dependencies between states and computed values
- **Batch Updates**: Group multiple state changes to optimise performance and avoid unnecessary updates
- **Extension Methods**: Comprehensive set of LINQ-like extension methods for transforming reactive state
- **Type Safety**: Fully type-safe API with generics support throughout
- **No Dependencies**: Written in vanilla C# with no external dependencies

> **Note:** Fusion.NET only currently supports single-threaded use and is not thread-safe.

## Installation

Fusion.NET is currently available only via GitHub. To use it in your project:

1. Clone this repository
2. Reference the project in your solution

```bash
git clone https://github.com/daireb/Fusion.NET.git
```

## Basic Usage

```csharp
using Fusion;

// Create reactive state
var count = new State<int>(0);
var message = new State<string>("Hello");

// Create computed values that react to state changes
var countDoubled = new Computed<int>(() => count.Value * 2);

// More concise syntax using extension methods
var countMessage = count.Select(c => $"{message.Value}, Count: {c}");

// Observe changes to state and computed values
count.Subscribe(value => Console.WriteLine($"Count changed to {value}"));
countMessage.Subscribe(msg => Console.WriteLine(msg));

// Update state values
count.Value = 1; // Triggers updates to countDoubled, countMessage, and observers

// Batch multiple updates to avoid intermediate notifications
using (var batch = BatchUpdate.Start())
{
    count.Value = 2;
    message.Value = "Greetings";
    // Dependents will only be notified once, after the batch completes
}

// Remember: All operations should be performed on a single thread
```

## Creating and Combining Reactive State

```csharp
// Create basic state
var firstName = new State<string>("John");
var lastName = new State<string>("Doe");

// Combine states into a computed value
var fullName = firstName.Zip(lastName, (first, last) => $"{first} {last}");

// Chain transformations
var greeting = fullName.Select(name => $"Hello, {name}!");

// Observe changes
fullName.Subscribe(name => Console.WriteLine($"Name changed to: {name}"));

// Update state to trigger reactions
firstName.Value = "Jane"; // Will update fullName and greeting
```

## Working with Collections

```csharp
var items = new State<List<string>>(new List<string> { "Apple", "Banana", "Cherry" });

// Transform each item in the collection
var upperItems = items.Select<List<string>, string, string>(item => item.ToUpper());

// Filter items
var filteredItems = items.Where<List<string>, string>(item => item.StartsWith("A"));

// Observe changes
upperItems.Subscribe(list => {
    Console.WriteLine("Upper items: " + string.Join(", ", list));
});

// Update collection
var newList = new List<string>(items.Value) { "Dragonfruit" };
items.Value = newList; // Triggers update to all derived computations
```

## Advanced Usage: Batch Updates

```csharp
// Multiple updates without intermediate reactions
BatchUpdate.Execute(() => {
    firstName.Value = "Jane";
    lastName.Value = "Smith";
    // fullName will only be recalculated once after both changes
});
```

## Architecture

Fusion.NET is built around a few core concepts:

- **State\<T>**: A container for reactive values
- **Computed\<T>**: A derived value that automatically updates when its dependencies change
- **Observer**: Subscribes to changes in state or computed values
- **BatchUpdate**: Manages transactions of multiple state changes
- **DependencyTracker**: Handles the dependency graph and automatic tracking

## Project Status

Fusion.NET is currently in active development. API may change before reaching version 1.0.

## Requirements

- .NET Standard 2.1 or higher
- Single-threaded environment (not thread-safe)

## License

[MIT License](LICENSE)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request