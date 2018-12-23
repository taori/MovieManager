using System;

namespace MovieManager.Shared.Services
{
	public class ParsedLink
	{
		/// <inheritdoc />
		public ParsedLink(DateTime dateTime, string name, string thumbnail, string url)
		{
			DateTime = dateTime;
			Name = name;
			Thumbnail = thumbnail;
			Url = url;
		}

		public DateTime DateTime { get; set; }

		public string Name { get; set; }

		public string Thumbnail { get; set; }

		public string Url { get; set; }
	}
}