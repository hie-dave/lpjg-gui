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
	/// Optional axis title for the X axis. If null, a default title will be
	/// generated and used based on the data being displayed.
	/// </summary>
	public string? XAxisTitle { get; set; }

	/// <summary>
	/// Optional axis title for the Y axis. If null, a default title will be
	/// generated and used based on the data being displayed.
	/// </summary>
	public string? YAxisTitle { get; set; }

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

	/// <summary>
	/// Get the axis requirements of all series for this graph.
	/// </summary>
	/// <returns>The axis requirements of all series for this graph.</returns>
	public IEnumerable<AxisRequirements> GetAxisRequirements()
	{
		return Series.SelectMany(s => s.GetAxisRequirements());
	}
}
