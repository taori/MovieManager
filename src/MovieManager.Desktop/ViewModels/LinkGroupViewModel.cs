using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MovieManager.Shared.Models;
using MovieManager.Shared.Services;
using NLog;
using Reactive.Bindings;

namespace MovieManager.Desktop.ViewModels
{
	public class LinkGroupViewModel : ViewModelBase, IDisposable
	{
		private readonly CompositeDisposable _disposables = new CompositeDisposable();

		private static readonly ILogger Log = LogManager.GetLogger(nameof(LinkGroupViewModel));

		public readonly LinkGroup Model;

		private ObservableCollection<LinkEntryViewModel> _items = new ObservableCollection<LinkEntryViewModel>();

		public ObservableCollection<LinkEntryViewModel> Items
		{
			get => _items;
			set => SetValue(ref _items, value, nameof(Items));
		}

		public ReactiveProperty<bool> IsFavourite { get; } = new ReactiveProperty<bool>(mode: ReactivePropertyMode.None);

		private string _thumbnail;

		public string Thumbnail
		{
			get => _thumbnail;
			set => SetValue(ref _thumbnail, value, nameof(Thumbnail));
		}

		private string _name;

		public string Name
		{
			get => _name;
			set => SetValue(ref _name, value, nameof(Name));
		}

		private bool _isSeries;

		public bool IsSeries
		{
			get => _isSeries;
			set => SetValue(ref _isSeries, value, nameof(IsSeries));
		}

		private readonly Subject<LinkGroupViewModel> _saveRequired = new Subject<LinkGroupViewModel>();
		public IObservable<LinkGroupViewModel> SaveRequired => _saveRequired;

		private ICommand _toggleFavouriteCommand;

		public ICommand ToggleFavouriteCommand
		{
			get => _toggleFavouriteCommand ?? (_toggleFavouriteCommand = new TaskCommand(ToggleFavouriteExecute));
			set => SetValue(ref _toggleFavouriteCommand, value, nameof(ToggleFavouriteCommand));
		}

		private Task ToggleFavouriteExecute(object arg)
		{
			IsFavourite.Value = !IsFavourite.Value;
			return Task.CompletedTask;
		}

		public LinkGroupViewModel(LinkGroup model)
		{
			Model = model;
			Thumbnail = model.Thumbnail;
			Name = model.Name;
			foreach (var entry in model.Links)
			{
				var entryVm = new LinkEntryViewModel(entry);
				Items.Add(entryVm);
			}

			IsSeries = Items.Any(d => SeriesIdentifierRegex.IsMatch(d.Link));
		}

		private void IsFavouriteChanged(bool obj)
		{
			Log.Debug($"{nameof(IsFavouriteChanged)}");
			_saveRequired.OnNext(this);
		}

		private void EntryIsViewedChanged(bool obj)
		{
			Log.Debug($"{nameof(EntryIsViewedChanged)}");
			_saveRequired.OnNext(this);
		}

		private readonly Regex SeriesIdentifierRegex = new Regex(@"\.html,s(?<series>[\d]+)e?(?<episode>[\d]+)?$", RegexOptions.Compiled);

		private static readonly Dictionary<string, LinkEntry> Empty = new Dictionary<string, LinkEntry>();

		public async Task InitializeAsync(HttpClient client, LinkGroup group)
		{
			if (group != null)
			{
				IsFavourite.Value = group.IsFavourite;
				Model.Id = group.Id;
			}

			IsSeries = Items.Any(d => d.Link.Contains(",s") && SeriesIdentifierRegex.IsMatch(d.Link));

			var storage = new ImageStorage();
			var filePath = storage.CreateImagePath(new ImageToken(Thumbnail));
			var links = new HashSet<string>();
			var linksById = group?.Links.ToDictionary(d => d.Link) ?? Empty;
			for (var index = Items.Count - 1; index >= 0; index--)
			{
				var item = Items[index];
				if (!links.Add(item.Link))
				{
					Items.RemoveAt(index);
				}
				else
				{
					if(linksById.TryGetValue(item.Model.Link, out var entry))
					{
						item.IsViewed.Value = entry.IsViewed;
					}
				}
			}

			if (File.Exists(filePath))
			{
				Thumbnail = filePath;
			}
			else
			{
				try
				{
					Log.Debug($"Path to image {filePath} does not exist.");

					var loader = new ImageLoader(client);
					var imagePrefix = ConfigurationManager.AppSettings["rootUrl"];
					Uri fullImageUrl;
					try
					{
						fullImageUrl = new Uri(new Uri(imagePrefix, UriKind.Absolute), new Uri(Thumbnail, UriKind.Relative));
					}
					catch (Exception e)
					{
						Log.Debug($"Composing download Uri from [{imagePrefix}] and [{Thumbnail}].");
						Log.Error(e);
						return;
					}
					var bytes = await loader.LoadAsync(fullImageUrl);
					try
					{
						await storage.SaveAsync(bytes, new ImageToken(Thumbnail));
						Thumbnail = filePath;
					}
					catch (Exception e)
					{
						Log.Debug($"Saving of image {Thumbnail} failed.");
						Log.Error(e);
					}
				}
				catch (Exception e)
				{
					Log.Debug($"Loading of image {Thumbnail} failed.");
					Log.Error(e);
				}
			}

			foreach (var item in Items)
			{
				_disposables.Add(item.IsViewed.Subscribe(EntryIsViewedChanged));
			}

			_disposables.Add(IsFavourite.Subscribe(IsFavouriteChanged));
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_disposables?.Dispose();
			IsFavourite?.Dispose();
			_saveRequired?.Dispose();
		}

		public void UpdateModel()
		{
			this.Model.IsFavourite = IsFavourite.Value;
			foreach (var link in Items)
			{
				link.UpdateModel();
			}
		}
	}
}