using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Base class for all presenters.
/// </summary>
/// <typeparam name="TView">The view type.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
public abstract class PresenterBase<TView, TModel> : IPresenter<TView, TModel>
	where TView : IView
{
	/// <summary>
	/// The view object.
	/// </summary>
	protected readonly TView view;

	/// <summary>
	/// The model object.
	/// </summary>
	protected readonly TModel model;

	/// <summary>
	/// The command registry to use for command execution.
	/// </summary>
	protected readonly ICommandRegistry registry;

	/// <summary>
	/// The model object.
	/// </summary>
	public TModel Model => model;

	/// <summary>
	/// Create a new <see cref="PresenterBase{TView, TModel}"/> instance.
	/// </summary>
	/// <param name="view">The view.</param>
	/// <param name="model">The model.</param>
	/// <param name="registry">The command registry to use for command execution.</param>
	public PresenterBase(TView view, TModel model, ICommandRegistry registry)
	{
		this.view = view;
		this.model = model;
		this.registry = registry;
	}

	/// <inheritdoc />
	public TView GetView() => view;

	/// <inheritdoc />
	IView IPresenter.GetView() => view;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public virtual void Dispose()
	{
		view.Dispose();
	}

	/// <summary>
	/// Invoke the given command.
	/// </summary>
	/// <param name="command">The command to invoke.</param>
	protected virtual void InvokeCommand(ICommand command)
	{
		registry.Execute(command);
	}
}
