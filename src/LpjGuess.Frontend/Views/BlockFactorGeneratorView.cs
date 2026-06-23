using Gtk;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a top-level factor generator.
/// </summary>
public class BlockFactorGeneratorView : TopLevelFactorGeneratorView, IBlockFactorGeneratorView
{
    /// <summary>
    /// The entry containing the block type.
    /// </summary>
    private readonly SuggestionEntryView blockTypeEntry;

    /// <summary>
    /// The entry containing the block name.
    /// </summary>
    private readonly SuggestionEntryView blockNameEntry;
    private IReadOnlyList<ParameterTarget> targetSuggestions;

    /// <inheritdoc />
    public new Event<IModelChange<BlockFactorGenerator>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="BlockFactorGeneratorView"/> instance.
    /// </summary>
    public BlockFactorGeneratorView() : base()
    {
        OnChanged = new Event<IModelChange<BlockFactorGenerator>>();
        targetSuggestions = [];

        // Create input widgets for the factor name, and block type/name.
        blockTypeEntry = new SuggestionEntryView("e.g. pft or st");
        blockNameEntry = new SuggestionEntryView("e.g. TeBE");
        blockTypeEntry.OnCommitted.ConnectTo(OnBlockTypeChanged);
        blockNameEntry.OnCommitted.ConnectTo(OnBlockNameChanged);

        SetControls(
            ("Block Type", blockTypeEntry.GetWidget()),
            ("Block Name", blockNameEntry.GetWidget()),
            ("Parameter Name", nameEntry.GetWidget()),
            ("Values", valuesTypeView.GetWidget()));

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string blockType, string blockName)
    {
        // Populate the scalar inputs.
        blockTypeEntry.SetText(blockType);
        blockNameEntry.SetText(blockName);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        blockTypeEntry.Dispose();
        blockNameEntry.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public override void SetTargetSuggestions(IEnumerable<ParameterTarget> targets)
    {
        targetSuggestions = targets
            .Where(target => target.BlockType is not null)
            .ToList();
        RefreshTargetSuggestions();
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
    }

    /// <summary>
    /// Commit a changed block name.
    /// </summary>
    /// <param name="value">The new block name.</param>
    private void OnBlockNameChanged(string value)
    {
        RefreshTargetSuggestions();
        OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
            factor => factor.BlockName,
            (factor, name) => factor.BlockName = name,
            value));
    }

    /// <summary>
    /// Commit a changed block type.
    /// </summary>
    /// <param name="value">The new block type.</param>
    private void OnBlockTypeChanged(string value)
    {
        RefreshTargetSuggestions();
        OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
            factor => factor.BlockType,
            (factor, type) => factor.BlockType = type,
            value));
    }

    private void RefreshTargetSuggestions()
    {
        string blockType = blockTypeEntry.Text.Trim();
        string blockName = blockNameEntry.Text.Trim();
        IEnumerable<ParameterTarget> matchingType = targetSuggestions.Where(target =>
            string.IsNullOrEmpty(blockType) ||
            string.Equals(target.BlockType, blockType, StringComparison.OrdinalIgnoreCase));
        IEnumerable<ParameterTarget> matchingBlock = matchingType.Where(target =>
            string.IsNullOrEmpty(blockName) ||
            string.Equals(target.BlockName, blockName, StringComparison.OrdinalIgnoreCase));

        blockTypeEntry.SetSuggestions(targetSuggestions.Select(target => target.BlockType!));
        blockNameEntry.SetSuggestions(matchingType.Select(target => target.BlockName!));
        nameEntry.SetSuggestions(matchingBlock.Select(target => target.ParameterName));
    }
}
