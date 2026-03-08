using System.Collections.Generic;
using System.Text;

namespace blekenbleu.SimHub_Remote_menu
{
	partial class HttpServer	// works in .NET Framework 4.8 WPF User Control library (SimHub plugin)
	{
		internal static string HTMLtable(string tableStyle = "font-size: 25px;",
				 string messageStyle = "width:600; color:navy; background-color:silver")
		{
			StringBuilder builder = new StringBuilder("\n<p id=msg style='{").Append(messageStyle).Append("'>");	// https://jonskeet.uk/csharp/stringbuilder.html

			// message paragraph
			builder.Append(ViewModel.SSEtext(false)).Append("</p>");

			// slider and label
			builder.Append("<input id='myRange' type='range' value='50' style='width:500'> ");		// replace	'50'
			builder.Append("<label id=active for='myRange'>unset</label>");							// replace 'unset'

			builder.Append("\n<table id=tok style='").Append(tableStyle).Append("'>\n<br><tr><th>Property</th><th>Current</th><th>Previous</th><th>Default</th></tr>");
			foreach (var row in WebMenu.simValues)
				builder.Append($"\n<tr><td>{row.Name}</td><td>{row.Current}</td><td>{row.Previous}</td><td>{row.Default}</td></tr>");
			builder.Append("\n</table>").Append(ClientSSE());
			return builder.ToString();
		}
	}
}
