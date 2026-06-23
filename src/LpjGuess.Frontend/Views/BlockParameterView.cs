using Gtk;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a block parameter.
/// </summary>
public class BlockParameterView : TopLevelParameterView, IBlockParameterView
{
    /// <summary>
    /// The entry widget for the block type.
    /// </summary>
    private readonly SuggestionEntryView blockTypeEntry;

    /// <summary>
    /// The entry widget for the block name.
    /// </summary>
    private readonly SuggestionEntryView blockNameEntry;
    private IReadOnlyList<ParameterTarget> targetSuggestions;

    /// <inheritdoc />
    public new Event<IModelChange<BlockParameter>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="BlockParameterView"/> instance.
    /// </summary>
    public BlockParameterView()
    {
        OnChanged = new Event<IModelChange<BlockParameter>>();
        targetSuggestions = [];

        blockTypeEntry = new SuggestionEntryView("e.g. pft or st");
        blockNameEntry = new SuggestionEntryView("e.g. TeBE");
        blockTypeEntry.OnCommitted.ConnectTo(OnBlockTypeChanged);
        blockNameEntry.OnCommitted.ConnectTo(OnBlockNameChanged);

        SetControls(
            ("Block Type", blockTypeEntry.GetWidget()),
            ("Block Name", blockNameEntry.GetWidget()),
            ("Parameter Name", nameEntry.GetWidget()),
            ("Parameter Value", valueEntry));

        ConnectBlockEvents();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        DisconnectBlockEvents();
        blockTypeEntry.Dispose();
        blockNameEntry.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public void Populate(string name, string blockType, string blockName, string value)
    {
        Populate(name, value);
        blockTypeEntry.SetText(blockType);
        blockNameEntry.SetText(blockName);
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
    /// Connect the block type and block name events.
    /// </summary>
    private void ConnectBlockEvents()
    {
    }

    /// <summary>
    /// Disconnect the block type and block name events.
    /// </summary>
    private void DisconnectBlockEvents()
    {
    }

    /// <summary>
    /// Commit a changed block type.
    /// </summary>
    /// <param name="value">The new block type.</param>
    private void OnBlockTypeChanged(string value)
    {
        RefreshTargetSuggestions();
        OnChanged.Invoke(new ModelChangeEventArgs<BlockParameter, string>(
            parameter => parameter.BlockType,
            (parameter, blockType) => parameter.BlockType = blockType,
            value));
    }

    /// <summary>
    /// Commit a changed block name.
    /// </summary>
    /// <param name="value">The new block name.</param>
    private void OnBlockNameChanged(string value)
    {
        RefreshTargetSuggestions();
        OnChanged.Invoke(new ModelChangeEventArgs<BlockParameter, string>(
            parameter => parameter.BlockName,
            (parameter, blockName) => parameter.BlockName = blockName,
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
