using System;
using System.Configuration;
using MovieManager.Shared.Models;
using Reactive.Bindings;

namespace MovieManager.Desktop.ViewModels
{
	public class LinkEntryViewModel : ViewModelBase
	{
		public readonly LinkEntry Model;

		public LinkEntryViewModel(LinkEntry model)
		{
			Model = model;
			Link = new Uri(new Uri(ConfigurationManager.AppSettings["rootUrl"], UriKind.Absolute), model.Link).ToString();
			Time = model.Time;
			IsViewed.Value = model.IsViewed;
		}

		private string _link;

		public string Link
		{
			get => _link;
			set => SetValue(ref _link, value, nameof(Link));
		}

		private DateTime _time;

		public DateTime Time
		{
			get => _time;
			set
			{
				if (SetValue(ref _time, value, nameof(Time)))
				{
					var dif = (DateTime.Now - value);
					PassedTimeString = dif.ToString("dd");
				}
			}
		}

		private string _passedTimeString;

		public string PassedTimeString
		{
			get => _passedTimeString;
			set => SetValue(ref _passedTimeString, value, nameof(PassedTimeString));
		}

		public ReactiveProperty<bool> IsViewed { get; } = new ReactiveProperty<bool>(mode: ReactivePropertyMode.None);

		public void UpdateModel()
		{
			Model.IsViewed = IsViewed.Value;
		}
	}
}