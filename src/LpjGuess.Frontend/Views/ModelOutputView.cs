using Gtk;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for editing a model output data source.
/// </summary>
public class ModelOutputView : ViewBase<Grid>, IModelOutputView
{
    /// <summary>
    /// Dropdown allowing the user to select an output file type.
    /// </summary>
    private readonly DropDownView<string> fileTypeView;

    /// <summary>
    /// Dropdown allowing the user to select an x-axis column.
    /// </summary>
    private readonly DropDownView<string> xAxisColumnView;

    /// <summary>
    /// Dropdown allowing the user to select a y-axis column.
    /// </summary>
    private readonly DropDownView<string> yAxisColumnView;

    /// <summary>
    /// Create a new <see cref="ModelOutputView"/> instance.
    /// </summary>
    public ModelOutputView() : base(new Grid())
    {
        OnEditDataSource = new Event<IModelChange<ModelOutput>>();

        fileTypeView = new DropDownView<string>();
        xAxisColumnView = new DropDownView<string>();
        yAxisColumnView = new DropDownView<string>();

        Label fileTypeLabel = Label.New("Output file");
        Label xAxisColumnLabel = Label.New("X-axis column");
        Label yAxisColumnLabel = Label.New("Y-axis column");

        widget.RowSpacing = 6;
        widget.ColumnSpacing = 6;

        widget.Attach(fileTypeLabel   , 0, 0, 1, 1);
        widget.Attach(fileTypeView    , 1, 0, 1, 1);

        widget.Attach(xAxisColumnLabel, 0, 1, 1, 1);
        widget.Attach(xAxisColumnView , 1, 1, 1, 1);

        widget.Attach(yAxisColumnLabel, 0, 2, 1, 1);
        widget.Attach(yAxisColumnView , 1, 2, 1, 1);

        widget.Hexpand = false;
        fileTypeView.Hexpand = true;
        xAxisColumnView.Hexpand = true;
        yAxisColumnView.Hexpand = true;

        fileTypeView.OnSelectionChanged.ConnectTo(OnFileTypeChanged);
        xAxisColumnView.OnSelectionChanged.ConnectTo(OnXAxisColumnChanged);
        yAxisColumnView.OnSelectionChanged.ConnectTo(OnYAxisColumnChanged);
    }

    /// <inheritdoc/>
    public Event<IModelChange<ModelOutput>> OnEditDataSource { get; private init; }

    /// <inheritdoc/>
    public void Populate(IEnumerable<string> fileTypes,
                         IEnumerable<string> columns,
                         string fileType,
                         string xColumn,
                         string yColumn)
    {
        fileTypeView.Populate(fileTypes, x => x);
        xAxisColumnView.Populate(columns, x => x);
        yAxisColumnView.Populate(columns, x => x);

        // This will fail if the collections don't contain the selected values.
        fileTypeView.Select(fileType);
        xAxisColumnView.Select(xColumn);
        yAxisColumnView.Select(yColumn);
    }

    /// <summary>
    /// Called when the user changes the x-axis column. Propagates the event
    /// back up to the owner of this view.
    /// </summary>
    /// <param name="obj">The new x-axis column.</param>
    private void OnXAxisColumnChanged(string obj)
    {
        try
        {
            var change = new ModelChangeEventArgs<ModelOutput, string>(
                m => m.XAxisColumn,
                (m, v) => m.XAxisColumn = v,
                obj);
            OnEditDataSource.Invoke(change);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user changes the y-axis column. Propagates the event
    /// back up to the owner of this view.
    /// </summary>
    /// <param name="obj">The new y-axis column.</param>
    private void OnYAxisColumnChanged(string obj)
    {
        try
        {
            var change = new ModelChangeEventArgs<ModelOutput, string>(
                m => m.YAxisColumn,
                (m, v) => m.YAxisColumn = v,
                obj);
            OnEditDataSource.Invoke(change);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user changes the output file type. Propagates the event
    /// back up to the owner of this view.
    /// </summary>
    /// <param name="fileType">The new output file type.</param>
    private void OnFileTypeChanged(string fileType)
    {
        try
        {
            var change = new ModelChangeEventArgs<ModelOutput, string>(
                m => m.OutputFileType,
                (m, v) => m.OutputFileType = v,
                fileType);
            OnEditDataSource.Invoke(change);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
