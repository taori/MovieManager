using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using NLog;

namespace MovieManager.Shared.Services
{
	public class DocumentParser
	{
		private static readonly Regex DatePattern = new Regex(@"(?<d>[\d]{1,2})\.(?<m>[\d]{1,2})\.(?<y>[\d]{4})", RegexOptions.Compiled);
		private static readonly Regex ListPattern = new Regex(@"(?<=LIST)(?:(?:_)?([\d]+))+", RegexOptions.Compiled);

		private static readonly ILogger Log = LogManager.GetLogger(nameof(DocumentParser));


		/***
			 * <tr class="Even parentalfreedom LIST2_3_4_5_6_8_9_10_11_12_13_14_15_16" title="/Stream/Suits-1.html,s7" id="7a64c3bda628a6bb705b6ebbce7183ae">
			   <td class="Icon"><img src="/gr/sys/lng/1.png" alt="language" width="16" height="11"></td>
			   <td class="Title img_preview" rel="/statics/thumbs/00046000/Suits-1.jpg" title=""><span href="/Stream/Suits-1.html,s7" title="Suits" class="OverlayLabel">Suits: <span class="EpisodeDescr">Staffel 7</span></span></td>
			   <td class="Format green"></td>
			   <td class="Format green"></td>
			   <td class="Format red"></td>
			   <td class="Rating">8.9</td>
			   </tr>
			 */


		/***
			 *  <tr class="Specialnews parentalguidiance" title="/Stream/The_Good_Doctor-2.html,s2e5" id="3043007b2c5f0e50dc1893357eae4ef5">
				<td class="Icon"><img src="/gr/sys/lng/1.png" alt="language" width="16" height="11" /></td>
				<td class="Title img_preview" rel="/statics/thumbs/00108000/The_Good_Doctor-2.jpg"><a href="/Stream/The_Good_Doctor-2.html,s2e5" title="The Good Doctor" class="OverlayLabel">The Good Doctor: <span class="EpisodeDescr">Staffel 2 Episode 5</span></a></td>
				<td class="Format green"></td>
				<td class="Format green"></td>
				<td class="Format red"></td>
				<td class="Rating">8.5</td>
				</tr>
			 */


		public IEnumerable<ParsedLink> ParseLinks(IHtmlDocument document)
		{
			Log.Trace("Getting nodes for document");
			if(!TryParseDate(document, out var date))
				yield break;

			var modules = document.QuerySelectorAll("table.FullModuleTable");
			foreach (var module in modules)
			{
				var tds = module.QuerySelectorAll("td.Title");
				foreach (var td in tds)
				{
					if (td.Parent is IElement parent)
					{
						if (ListPattern.IsMatch(parent.ClassName))
						{
							var match = ListPattern.Match(parent.ClassName);
							foreach (Capture capture in match.Groups[1].Captures)
							{
								var episode = capture.Value;
								var thumbnail = td.GetAttribute("rel");
								var found = GetNodeContent(td.QuerySelector("span"), out var title, out var href)
								            || GetNodeContent(td.QuerySelector("a"), out title, out href);

								yield return new ParsedLink(date, title, thumbnail, $"{href}e{episode}");
							}
						}
						else
						{
							var thumbnail = td.GetAttribute("rel");
							var found = GetNodeContent(td.QuerySelector("span"), out var title, out var href)
							            || GetNodeContent(td.QuerySelector("a"), out title, out href);

							yield return new ParsedLink(date, title, thumbnail, href);
						}
					}
				}
			}
		}

		private bool TryParseDate(IHtmlDocument document, out DateTime date)
		{
			// <div class="Opt leftOpt Headlne"><h1>Frisches aus dem Kino vom 14.12.2018</h1></div>
			var dateContent = document.QuerySelectorAll("div.Headlne.Opt.leftOpt h1").Skip(2).FirstOrDefault()?.InnerHtml;
			if (TryParseDateString(dateContent, out date))
				return true;

			date = DateTime.MinValue;
			return false;
		}


		private bool TryParseDateString(string dateContent, out DateTime date)
		{
			date = DateTime.MinValue;
			if (!DatePattern.IsMatch(dateContent))
				return false;

			var match = DatePattern.Match(dateContent);
			var d = match.Groups["d"].Value;
			var m = match.Groups["m"].Value;
			var y = match.Groups["y"].Value;

			if (int.TryParse(d, out var dVal)
			    && int.TryParse(m, out var mVal)
			    && int.TryParse(y, out var yVal))
			{
				if (!DateTime.TryParseExact($"{dVal:00}.{mVal:00}.{yVal:0000}", "dd.MM.yyyy", null, DateTimeStyles.None, out date))
					return false;
				return true;
			}

			return false;
		}

		private bool GetNodeContent(IElement match, out string title, out string href)
		{
			if (match == null)
			{
				href = null;
				title = null;
				return false;
			}

			title = match.GetAttribute("title");
			href = match.GetAttribute("href");
			return !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href);
		}
	}
}