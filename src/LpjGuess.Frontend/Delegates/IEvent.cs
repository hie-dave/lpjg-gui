namespace LpjGuess.Frontend.Delegates;

/// <summary>
/// An interface to an event.
/// </summary>
public interface IEvent
{
	/// <summary>
	/// Connect this event source to the specified sink.
	/// </summary>
	/// <param name="handler">The event handler.</param>
	void ConnectTo(Action handler);
}

/// <summary>
/// An interface to a parameterised event.
/// </summary>
public interface IEvent<T>
{
	/// <summary>
	/// Connect this event source to the specified sink.
	/// </summary>
	/// <param name="handler">The event handler.</param>
	void ConnectTo(Action<T> handler);
}
