using System;

namespace MovieManager.Shared.Models
{
	public class LinkEntry
	{
		/// <inheritdoc />
		public LinkEntry(bool isViewed, string link, DateTime time)
		{
			IsViewed = isViewed;
			Link = link;
			Time = time;
		}

		/// <inheritdoc />
		public LinkEntry()
		{
		}

		public int Id { get; set; }

		public bool IsViewed { get; set; }

		public string Link { get; set; }

		public DateTime Time { get; set; }
	}
}