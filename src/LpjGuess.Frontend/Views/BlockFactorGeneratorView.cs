using Gtk;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a top-level factor generator.
/// </summary>
public class BlockFactorGeneratorView : TopLevelFactorGeneratorView, IBlockFactorGeneratorView
{
    /// <summary>
    /// The entry containing the block type.
    /// </summary>
    private readonly Entry blockTypeEntry;

    /// <summary>
    /// The entry containing the block name.
    /// </summary>
    private readonly Entry blockNameEntry;

    /// <inheritdoc />
    public new Event<IModelChange<BlockFactorGenerator>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="BlockFactorGeneratorView"/> instance.
    /// </summary>
    public BlockFactorGeneratorView() : base()
    {
        OnChanged = new Event<IModelChange<BlockFactorGenerator>>();

        // Create input widgets for the factor name, and block type/name.
        blockTypeEntry = new Entry();
        blockNameEntry = new Entry();

        // Pack the scalar controls into the grid.
        AddControl("Block Type", blockTypeEntry);
        AddControl("Block Name", blockNameEntry);

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
        base.Dispose();
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
        blockTypeEntry.OnActivate += OnBlockTypeChanged;
        blockNameEntry.OnActivate += OnBlockNameChanged;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
        blockTypeEntry.OnActivate -= OnBlockTypeChanged;
        blockNameEntry.OnActivate -= OnBlockNameChanged;
    }

    /// <summary>
    /// Called when the block name entry is activated.
    /// </summary>
    /// <param name="sender">The block name entry.</param>
    /// <param name="args">The event arguments.</param>
    private void OnBlockNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
                f => f.BlockName,
                (f, name) => f.BlockName = name,
                blockNameEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the block type entry is activated.
    /// </summary>
    /// <param name="sender">The block type entry.</param>
    /// <param name="args">The event arguments.</param>
    private void OnBlockTypeChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
                f => f.BlockType,
                (f, type) => f.BlockType = type,
                blockTypeEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
