// 

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace MovieManager.Desktop
{
	public class TaskCommand : RelayCommand<object>
	{
		/// <inheritdoc />
		public TaskCommand(Func<object, Task> execute) : base(execute)
		{
		}

		/// <inheritdoc />
		public TaskCommand(Func<object, Task> execute, Predicate<object> canExecute) : base(execute, canExecute)
		{
		}
	}

	public class RelayCommand<T> : ICommand
	{
		private static readonly ILogger Log = LogManager.GetLogger(nameof(TaskCommand));

		#region Fields

		private int _executing;

		readonly Func<T, Task> _execute = null;
		readonly Predicate<T> _canExecute = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
		/// </summary>
		/// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
		/// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
		public RelayCommand(Func<T, Task> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Func<T, Task> execute, Predicate<T> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand Members

		///<summary>
		///Defines the method that determines whether the command can execute in its current state.
		///</summary>
		///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		///<returns>
		///true if this command can be executed; otherwise, false.
		///</returns>
		public bool CanExecute(object parameter)
		{
			var canExecute = _canExecute == null ? _executing == 0 : _canExecute((T)parameter) && _executing == 0;
			Log.Trace($"CanExecute {canExecute}.");
			return canExecute;
		}

		///<summary>
		///Occurs when changes occur that affect whether or not the command should execute.
		///</summary>
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		///<summary>
		///Defines the method to be called when the command is invoked.
		///</summary>
		///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
		public async void Execute(object parameter)
		{
			if (Interlocked.Exchange(ref _executing, 1) == 0)
			{
				CommandManager.InvalidateRequerySuggested();
				try
				{
					await _execute((T)parameter);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
				Interlocked.Exchange(ref _executing, 0);
				await Task.Delay(50);
				CommandManager.InvalidateRequerySuggested();
			}
		}

		#endregion
	}
}