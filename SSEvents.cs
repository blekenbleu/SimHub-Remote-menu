namespace blekenbleu.SimHub_Remote_menu
{
	partial class HttpServer	// works in .NET Framework 4.8 WPF User Control library (SimHub plugin)
	{
//		These server event methods correspond to source.addEventListener()s for JavaScript.cs
//		JavaScript accepts either ' or ", but JSON insists on "
		// https://github.com/CharlieDigital/dn7-server-sent-events/blob/main/api/Program.cs
		// update a table cell
		public static void SSEcell(int row, int col, string val)
		{
			SSEvent("table",
						  $"\"row\": \"{row}\","
						+ $"\"col\": \"{col}\","
						+ $"\"val\": \"{val}\"");
		}

		// highlight the currently active row
		public static void SSEscroll(int row)
		{
			// JavaScript considers <table> first row to be 1
			SSEvent("scroll", $"\"row\": \"{1 + row}\"");
		}

		// designated slider property name and value
		public static void SSEslide(double val, string prop)
		{
			SSEvent("slider", $"\"prop\": \"{prop}\", \"val\": \"{val}\"");
		}

		public static void SSEmessage(string msg)
		{
			SSErespond("event:\ndata:" + msg);
		}
	}
}
