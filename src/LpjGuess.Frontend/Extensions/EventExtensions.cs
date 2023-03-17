using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for the event classes.
/// </summary>
public static class EventExtensions
{
	/// <summary>
	/// Connect the source event to the sink event.
	/// </summary>
	/// <param name="source">Source event.</param>
	/// <param name="sink">Sink event.</param>
	public static void ConnectTo(this Event source, Event sink)
	{
		source.ConnectTo(() => sink.Invoke());
	}

	/// <summary>
	/// Connect the parameterised source event to the sink event.
	/// </summary>
	/// <param name="source">The source event.</param>
	/// <param name="sink">The sink event.</param>
	public static void ConnectTo<T>(this Event<T> source, Event<T> sink)
	{
		source.ConnectTo(x => sink.Invoke(x));
	}
}
