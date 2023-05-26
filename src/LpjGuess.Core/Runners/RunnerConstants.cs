namespace LpjGuess.Core.Runners;

/// <summary>
/// Constants used by multiple runners.
/// </summary>
internal static class RunnerConstants
{
	/// <summary>
	/// CLI argument which should precede the input module.
	/// </summary>
	public const string InputCliArgument = "-input";

	/// <summary>
	/// Name of the guess executable.
	/// </summary>
	public const string ExeName = 
#if WINDOWS
	"guesscmd.exe";
#else
	"guess";
#endif

	/// <summary>
	/// Name of the PATH environment variable.
	/// </summary>
	public const string PathVariableName = "PATH";

	/// <summary>
	/// Delimiter used for the PATH environment variable.
	/// </summary>
	public const string PathDelim = 
#if WINDOWS
		";";
#else
		":";
#endif
}
