using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Base class for all presenters.
/// </summary>
/// <typeparam name="TView"></typeparam>
public abstract class PresenterBase<TView> : IPresenter<TView>
	where TView : IView
{
	/// <summary>
	/// The view object.
	/// </summary>
	protected readonly TView view;

	/// <summary>
	/// The command registry.
	/// </summary>
	protected readonly ICommandRegistry registry;

	/// <summary>
	/// Create a new <see cref="PresenterBase{TView}"/> instance.
	/// </summary>
	/// <param name="view">The view.</param>
	public PresenterBase(TView view)
	{
		this.view = view;
		registry = CommandRegistry.Instance;
	}

	/// <inheritdoc />
	public TView GetView() => view;

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
