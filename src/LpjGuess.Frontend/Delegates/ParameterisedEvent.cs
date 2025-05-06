namespace LpjGuess.Frontend.Delegates;

/// <summary>
/// An event source for a parameterised event.
/// </summary>
public class Event<T> : IEvent<T>
{
	/// <summary>
	/// The event callback functions.
	/// </summary>
	private readonly List<Action<T>> handlers = new();

	/// <summary>
	/// Invoke the event.
	/// </summary>
	/// <param name="input">The event input parameter.</param>
	public void Invoke(T input)
	{
		foreach (Action<T> handler in handlers.ToList())
			handler(input);
	}

	/// <summary>
	/// Connect this event source to the specified sink.
	/// </summary>
	/// <param name="handler">The event handler.</param>
	public void ConnectTo(Action<T> handler)
	{
		handlers.Add(handler);
	}

	/// <summary>
	/// Disconnect the event from the event handler.
	/// </summary>
	/// <param name="handler">The event sink to be disconnected.</param>
	public void DisconnectFrom(Action<T> handler)
	{
		try
		{
			if (!handlers.Contains(handler))
				throw new InvalidOperationException($"Event source is not connected to this sink");

			handlers.Remove(handler);
		}
		catch (Exception error)
		{
			throw new InvalidOperationException($"Failed to disconnect event handler", error);
		}
	}

	/// <summary>
	/// Disconnect all event sinks from this source.
	/// </summary>
	public void DisconnectAll()
	{
		handlers.Clear();
	}

	/// <summary>
	/// Dispoe of the event by disconnecting all sinks.
	/// </summary>
	public void Dispose()
	{
		DisconnectAll();
	}
}
