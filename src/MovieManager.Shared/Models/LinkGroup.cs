using System;
using System.Collections.Generic;

namespace MovieManager.Shared.Models
{
	public class LinkGroup : ICloneable
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Thumbnail { get; set; }

		public bool IsFavourite { get; set; }

		public virtual List<LinkEntry> Links { get; set; } = new List<LinkEntry>();

		/// <inheritdoc />
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}