namespace LpjGuess.Frontend.EventTypes;

/// <summary>
/// Represents an event with no arguments coming from the GUI.
/// </summary>
public delegate void Event();

/// <summary>
/// An event coming from the GUI with an argument of type T.
/// </summary>
/// <param name="args">Event data.</param>
/// <typeparam name="T">Event argument data type.</typeparam>
public delegate void Event<T>(T args);
