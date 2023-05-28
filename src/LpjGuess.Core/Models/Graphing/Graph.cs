using LpjGuess.Core.Interfaces.Graphing;

namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A graph.
/// </summary>
public class Graph
{
	/// <summary>
	/// Graph title.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// Series displayed on this graph.
	/// </summary>
	public List<ISeries> Series { get; set; }

	/// <summary>
	/// Create a new <see cref="Graph"/> instance.
	/// </summary>
	/// <param name="title">Graph title.</param>
	public Graph(string title)
	{
		Title = title;
		Series = new List<ISeries>();
	}

	/// <summary>
	/// Default constructor provided for serialization purposes.
	/// </summary>
	public Graph() : this(string.Empty) { }
}
