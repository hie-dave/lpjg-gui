using Gtk;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a block parameter.
/// </summary>
public class BlockParameterView : TopLevelParameterView, IBlockParameterView
{
    /// <summary>
    /// The entry widget for the block type.
    /// </summary>
    private readonly Entry blockTypeEntry;

    /// <summary>
    /// The entry widget for the block name.
    /// </summary>
    private readonly Entry blockNameEntry;

    /// <inheritdoc />
    public new Event<IModelChange<BlockParameter>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="BlockParameterView"/> instance.
    /// </summary>
    public BlockParameterView()
    {
        OnChanged = new Event<IModelChange<BlockParameter>>();

        blockTypeEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        blockNameEntry = new Entry() { Halign = Align.Fill, Hexpand = true };

        AddControl("Block Type", blockTypeEntry);
        AddControl("Block Name", blockNameEntry);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        // DisconnectEvents() is called from the base class' Dispose().
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
    protected override void ConnectEvents()
    {
        base.ConnectEvents();
        blockTypeEntry.OnActivate += OnBlockTypeChanged;
        blockNameEntry.OnActivate += OnBlockNameChanged;
    }

    /// <inheritdoc />
    protected override void DisconnectEvents()
    {
        base.DisconnectEvents();
        blockTypeEntry.OnActivate -= OnBlockTypeChanged;
        blockNameEntry.OnActivate -= OnBlockNameChanged;
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
            OnChanged.Invoke(new ModelChangeEventArgs<BlockParameter, string>(
                p => p.BlockType,
                (p, blockType) => p.BlockType = blockType,
                blockTypeEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
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
            OnChanged.Invoke(new ModelChangeEventArgs<BlockParameter, string>(
                p => p.BlockName,
                (p, blockName) => p.BlockName = blockName,
                blockNameEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
