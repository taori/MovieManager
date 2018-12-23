using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace MovieManager.Desktop.Behaviours
{
	public class PersistSettingsOnWindowChange : Behavior<Window>
	{
		private static readonly NLog.ILogger Log = NLog.LogManager.GetLogger(nameof(PersistSettingsOnWindowChange));

		private CompositeDisposable _subscriptions;

		/// <inheritdoc />
		protected override void OnAttached()
		{
			_subscriptions?.Dispose();
			_subscriptions = new CompositeDisposable
			{
				Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(
						add => AssociatedObject.SizeChanged += add,
						remove => AssociatedObject.SizeChanged -= remove)
					.Throttle(TimeSpan.FromSeconds(2))
					.Subscribe(NextSize),
				Observable.FromEventPattern<EventHandler, EventArgs>(
						add => AssociatedObject.LocationChanged += add,
						remove => AssociatedObject.LocationChanged -= remove)
					.Throttle(TimeSpan.FromSeconds(2))
					.Subscribe(NextLocation)
			};

			base.OnAttached();
		}

		private void NextLocation(EventPattern<EventArgs> obj)
		{
			Log.Debug($"Location changed. Updating Settings file.");
			Properties.Settings.Default.Save();
		}

		private void NextSize(EventPattern<SizeChangedEventArgs> obj)
		{
			Log.Debug($"Size changed. Updating Settings file.");
			Properties.Settings.Default.Save();
		}

		/// <inheritdoc />
		protected override void OnDetaching()
		{
			_subscriptions.Dispose();
			base.OnDetaching();
		}
	}
}