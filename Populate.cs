using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public class Property	   // must be public for DataPluginSettings
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	public partial class WebMenu
	{
		List<int> Steps;								// 100 times actual values

		/// <summary>
		/// DisplayGrid contents
		/// </summary>
		public static List<Values> simValues;

		// add properties and settings to simValues; initialize Steps
		// if a property move among
		bool Populate(List<string>props, List<string> vals, List<string> stps, int start, int stopCount)
		{
			bool pch = false;
			for (int c = 0; c < props.Count; c++)
			{
				if (0 == props[c]?.Length)
				{
					props.RemoveAt(c);
					if (c < vals.Count)
						vals.RemoveAt(c);
					if (c < stps.Count)
						stps.RemoveAt(c);
					c--;
					pch = true;
					continue;
				}

				// populate DisplayGrid ItemsSource
				// WebMenu.ini contents may not match saved car properties
				// default value from .ini
				int index = Settings.properties.FindIndex(i => i.Name == props[c]);

				if (index != c)
					pch = true;
				string ini = (c < vals.Count) ? vals[c] :  (0 > index) ? "0" : Settings.properties[index].Value;
				// use Settings.properties value, if it exists, else from .ini
				string setting = (0 > index) ? ini: Settings.properties[index].Value;

				simValues.Add(new Values
				{
					Name = props[c],
					Default = ini,			// replaced by JSON values
					Current = setting,
					Previous = setting
				});
				Steps.Add((c < stps.Count) ? (int)(100 * float.Parse(stps[c])) : 10);
			}

			// has simValues property list changed from Settings?
			stopCount += start;
			if (stopCount != simValues.Count)
				pch = true;
			if (pch)
				set = true;
			return pch;
		}
	}		// class WebMenu
}
