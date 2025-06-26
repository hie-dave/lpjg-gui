using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command that validates a series after other changes have been applied,
/// ensuring that all properties remain in a consistent state.
/// </summary>
public abstract class SeriesValidationCommand<T> : ICommand where T : ISeries
{
    /// <summary>
    /// The series to validate.
    /// </summary>
    protected readonly T series;

    /// <summary>
    /// The commands generated during validation.
    /// </summary>
    protected List<ICommand> generatedCommands;

    /// <summary>
    /// Creates a new <see cref="SeriesValidationCommand{T}"/> instance.
    /// </summary>
    /// <param name="series">The series to validate.</param>
    protected SeriesValidationCommand(T series)
    {
        this.series = series;
        generatedCommands = [];
    }

    /// <inheritdoc />
    public void Execute()
    {
        // Generate validation commands
        generatedCommands = GenerateValidationCommands().ToList();

        // Execute all generated commands
        foreach (ICommand command in generatedCommands)
            command.Execute();
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Undo all generated commands in reverse order
        for (int i = generatedCommands.Count - 1; i >= 0; i--)
            generatedCommands[i].Undo();
    }

    /// <inheritdoc />
    public string GetDescription()
    {
        return $"Validate {series.GetType().Name} series '{series.Title}'";
    }

    /// <summary>
    /// Validates the series and generates commands to fix any inconsistencies.
    /// </summary>
    /// <returns>A collection of commands that will fix any inconsistencies.</returns>
    protected virtual IEnumerable<ICommand> GenerateValidationCommands()
    {
        if (!IsStyleProviderValid(s => s.ColourProvider))
            yield return GenerateStyleProviderFixer(
                s => s.ColourProvider,
                (s, p) => s.ColourProvider = p);
    }

    /// <summary>
    /// Checks if the style provider is valid for the series.
    /// </summary>
    /// <typeparam name="TStyle">The type of the style.</typeparam>
    /// <param name="getter">The function to get the current value.</param>
    /// <returns>True if the style provider is valid, false otherwise.</returns>
    protected bool IsStyleProviderValid<TStyle>(
        Func<T, IStyleProvider<TStyle>> getter)
    {
        IStyleProvider<TStyle> provider = getter(series);
        StyleVariationStrategy strategy = provider.GetStrategy();

        var allowedStrategies = series.DataSource.GetAllowedStyleVariationStrategies();
        if (!allowedStrategies.Contains(StyleVariationStrategy.Fixed))
            allowedStrategies = allowedStrategies.Append(StyleVariationStrategy.Fixed);
        return allowedStrategies.Contains(strategy);
    }

    /// <summary>
    /// Generates a command to fix the style provider.
    /// </summary>
    /// <typeparam name="TStyle">The type of the style.</typeparam>
    /// <param name="getter">The function to get the current value.</param>
    /// <param name="setter">
    /// The action to set the style provider to the new value.
    /// </param>
    protected ICommand GenerateStyleProviderFixer<TStyle>(
        Func<T, IStyleProvider<TStyle>> getter,
        Action<T, IStyleProvider<TStyle>> setter)
    {
        // Create a fixed provider with the current style
        // We need to get the current style from the provider
        // Since we don't have access to ISeriesData directly, we'll create a fixed provider
        // with the default style for the type
        FixedStyleProvider<TStyle> fixedProvider = new(GetDefaultStyle<TStyle>());

        // Create the command
        return new PropertyChangeCommand<T, IStyleProvider<TStyle>>(
            series,
            getter(series),
            fixedProvider,
            setter);
    }

    /// <summary>
    /// Get the default style value used when the user has changed the property
    /// to a "fixed" strategy.
    /// </summary>
    /// <returns>The default style.</returns>
    private static TStyle GetDefaultStyle<TStyle>()
    {
        // Return appropriate default values based on the style type.
        // TODO: the corresponding commands should have better default values.
        if (typeof(TStyle) == typeof(Colour))
            return (TStyle)(object)Colours.Palette.First();
        if (typeof(TStyle) == typeof(LineType))
            return (TStyle)(object)LineType.Solid;
        if (typeof(TStyle) == typeof(LineThickness))
            return (TStyle)(object)LineThickness.Regular;

        // Default fallback
        return default!;
    }
}
