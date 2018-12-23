using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.EntityFrameworkCore;
using MovieManager.Desktop.Properties;
using MovieManager.Desktop.Services;
using MovieManager.Shared.Models;
using MovieManager.Shared.Services;
using NLog;
using Reactive.Bindings;

namespace MovieManager.Desktop.ViewModels
{
	public enum DisplayTypeFilter
	{
		[Description("Both")]
		Both,
		[Description("Movies")]
		Movies,
		[Description("Series")]
		Series
	}

	public class MainViewModel : ViewModelBase
	{
		public MainViewModel()
		{
			using (var dbContext = DbContextFactory.Create())
			{
				try
				{
					dbContext.Database.Migrate();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}

			ReloadViewCommand = new TaskCommand(ReloadViewExecute);
			ReloadDataCommand = new TaskCommand(ReloadDataExecute);

			CurrentSearch = new ReactiveProperty<string>();
			CurrentSearch
				.Throttle(TimeSpan.FromMilliseconds(500))
				.Subscribe(SearchChanged);

			DisplayFilter = new ReactiveProperty<DisplayTypeFilter>();
			DisplayFilter
				.Throttle(TimeSpan.FromMilliseconds(500))
				.Subscribe(FilterChanged);

			DisplayCount = new ReactiveProperty<int>();
			DisplayCount
				.Throttle(TimeSpan.FromSeconds(2))
				.Subscribe(DisplayCountChanged);
			DisplayCount.Value = Settings.Default.DisplayLastXDays;

			ControlsEnabled = new ReactiveProperty<bool>();

			FavouritesOnly = new ReactiveProperty<bool>();
			FavouritesOnly
				.Subscribe(FavouritesOnlyChanged);

			WideMode = new ReactiveProperty<bool>();
		}

		private static readonly ILogger Log = LogManager.GetLogger(nameof(MainViewModel));

		public ReactiveProperty<DisplayTypeFilter> DisplayFilter { get; }

		private ICommand _reloadViewCommand;

		public ICommand ReloadViewCommand
		{
			get => _reloadViewCommand;
			set => SetValue(ref _reloadViewCommand, value, nameof(ReloadViewCommand));
		}

		private ICommand _reloadDataCommand;

		public ICommand ReloadDataCommand
		{
			get => _reloadDataCommand;
			set => SetValue(ref _reloadDataCommand, value, nameof(ReloadDataCommand));
		}

		private ObservableCollection<LinkGroupViewModel> _items = new ObservableCollection<LinkGroupViewModel>();

		public ObservableCollection<LinkGroupViewModel> Items
		{
			get => _items;
			set => SetValue(ref _items, value, nameof(Items));
		}

		public ReactiveProperty<int> DisplayCount { get; }

		public ReactiveProperty<bool> ControlsEnabled { get; }

		public ReactiveProperty<bool> FavouritesOnly { get; }

		public ReactiveProperty<bool> WideMode { get; }

		public ReactiveProperty<string> CurrentSearch { get; }

		private void SearchChanged(string value)
		{
			Log.Debug($"Search term changed to {value}.");
			ReloadViewCommand.Execute(null);
		}

		private void FilterChanged(DisplayTypeFilter value)
		{
			Log.Debug($"FilterChanged {value}.");
			ReloadViewCommand.Execute(null);
		}

		private void DisplayCountChanged(int value)
		{
			if (value > 0)
			{
				Log.Debug($"Display count changed to {value}.");
				Settings.Default.DisplayLastXDays = value;
				Settings.Default.Save();
				ReloadViewCommand.Execute(null);
			}
		}

		private void FavouritesOnlyChanged(bool obj)
		{
			Log.Debug($"FavouritesOnlyChanged changed to {obj}.");
			ReloadViewCommand.Execute(null);
		}

		private async Task ReloadDataExecute(object arg)
		{
			var loader = new ContentCacheLoader(new ContentStorage(), new WebLoader());
			await loader.LoadContentOfDaysAsync(DisplayCount.Value);
		}

		private async Task ReloadViewExecute(object obj)
		{
			this.ControlsEnabled.Value = false;
			if (Debugger.IsAttached)
				await Task.Delay(2000);
			var client = new ContentStorage();
			Log.Debug($"Loading last {DisplayCount.Value} days.");
			var filePaths = client.GetFilePaths().OrderByDescending(d => d).Take(DisplayCount.Value);

			Items = await GetComposedModelAsync(filePaths);
			this.ControlsEnabled.Value = true;
		}

		private async Task<ObservableCollection<LinkGroupViewModel>> GetComposedModelAsync(IEnumerable<string> filePaths)
		{
			var newItems = new ObservableCollection<LinkGroupViewModel>();
			var parser = new HtmlParser(new HtmlParserOptions());

			Log.Debug($"Getting documents.");
			var documents = await GetDocumentsAsync(filePaths, parser);
			await AddNodesAsync(newItems, documents);

			var filtered = new ObservableCollection<LinkGroupViewModel>(
				newItems
					.Where(MatchFavourite)
					.Where(MatchDisplayType)
					.Where(MatchSearchWord)
			);

			return filtered;
		}

		private bool MatchFavourite(LinkGroupViewModel item)
		{
			return !FavouritesOnly.Value || item.IsFavourite.Value;
		}


		private bool MatchSearchWord(LinkGroupViewModel item)
		{
			if (string.IsNullOrEmpty(CurrentSearch.Value))
				return true;

			return Compare(item.Name, CurrentSearch.Value);
		}

		private bool MatchDisplayType(LinkGroupViewModel item)
		{
			if (DisplayFilter.Value == DisplayTypeFilter.Both)
				return true;
			switch (DisplayFilter.Value)
			{
				case DisplayTypeFilter.Both:
					return true;
				case DisplayTypeFilter.Movies:
					return !item.IsSeries;
				case DisplayTypeFilter.Series:
					return item.IsSeries;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private bool Compare(string haystack, string needle)
		{
			return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private async Task AddNodesAsync(ObservableCollection<LinkGroupViewModel> newItems, IEnumerable<IHtmlDocument> documents)
		{
			Log.Debug($"AddNodes");
			var parser = new DocumentParser();
			var lookup = documents.SelectMany(document => parser.ParseLinks(document)).ToLookup(d => d.Thumbnail);
			var links = new HashSet<string>();
			foreach (var lookupGroup in lookup)
			{
				var group = new LinkGroup();
				group.Thumbnail = lookupGroup.Key;
				group.Name = lookupGroup.Key;
				foreach (var link in lookupGroup)
				{
					group.Thumbnail = link.Thumbnail;
					group.Name = link.Name;
					if (links.Add(link.Url))
						group.Links.Add(new LinkEntry(false, link.Url, link.DateTime));
				}
				var groupVm = new LinkGroupViewModel(group);
				groupVm.SaveRequired.Subscribe(SaveGroupAsync);
				newItems.Add(groupVm);
			}

			using (var client = HttpClientFactory.Create())
			{
				using (var dbContext = DbContextFactory.Create())
				{
					var localClient = client;
					var context = dbContext;
					await Task.WhenAll(newItems.Select(async d =>
					{
						var linkGroup = await context
							.Groups
							.AsNoTracking()
							.Include(f => f.Links)
							.FirstOrDefaultAsync(f => d.Model.Name == f.Name);
						return d.InitializeAsync(localClient, linkGroup);
					}));
				}
			}
		}

		private async void SaveGroupAsync(LinkGroupViewModel groupVm)
		{
			Log.Debug($"{nameof(SaveGroupAsync)}");
			using (var context = DbContextFactory.Create())
			{
				var match = await context
					.Groups
					.AsNoTracking()
					.Include(d => d.Links)
					.FirstOrDefaultAsync(d => d.Name == groupVm.Model.Name);

				if (match == null)
				{
					groupVm.UpdateModel();
					context.Groups.Add(groupVm.Model);
				}
				else
				{
					groupVm.UpdateModel();
					context.Update(groupVm.Model);
				}
				var success = await context.SaveChangesAsync() > 0;
				Log.Info($"Toggle save success: {success}.");
				if (!success)
				{
					Log.Error($"Saving failed.");
				}
			}
		}

		private async Task<IEnumerable<IHtmlDocument>> GetDocumentsAsync(IEnumerable<string> filePaths, HtmlParser parser)
		{
			List<IHtmlDocument> documents = new List<IHtmlDocument>();

			foreach (var filePath in filePaths)
			{
				string content;
				using (var reader = new StreamReader(filePath))
				{
					content = await reader.ReadToEndAsync();
					documents.Add(await parser.ParseAsync(content));
				}
			}

			return documents;
		}

	}
}