namespace LpjGuess.Frontend.Delegates;

/// <summary>
/// An event source.
/// </summary>
public class Event : IEvent
{
	/// <summary>
	/// The event callback functions.
	/// </summary>
	private readonly List<Action> handlers = new();

	/// <summary>
	/// Invoke the event.
	/// </summary>
	public void Invoke()
	{
		foreach (Action handler in handlers.ToList())
			handler();
	}

	/// <summary>
	/// Connect this event source to the specified sink.
	/// </summary>
	/// <param name="handler">The event handler.</param>
	public void ConnectTo(Action handler)
	{
		handlers.Add(handler);
	}

	/// <summary>
	/// Disconnect the event from the event handler.
	/// </summary>
	/// <param name="handler">The event sink to be disconnected.</param>
	public void DisconnectFrom(Action handler)
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
}
