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
		void Populate(List<string>iniProps, List<string> vals, List<string> stps, int start, int stopCount)
		{
			for (int c = 0; c < iniProps.Count; c++)
			{
				if (0 == iniProps[c]?.Length)
				{
					iniProps.RemoveAt(c);
					if (c < vals.Count)
						vals.RemoveAt(c);
					if (c < stps.Count)
						stps.RemoveAt(c);
					c--;
					continue;
				}

				// populate DisplayGrid ItemsSource
				// WebMenu.ini contents may not match saved car properties
				// default value from .ini
				int index = Settings.properties.FindIndex(i => i.Name == iniProps[c]);

				string ini = (c < vals.Count) ? vals[c] :  (0 > index) ? "0" : Settings.properties[index].Value;
				// use Settings.properties value, if it exists, else from .ini
				string setting = (0 > index) ? ini: Settings.properties[index].Value;

				simValues.Add(new Values
				{
					Name = iniProps[c],
					Default = ini,			// replaced by JSON values
					Current = setting,
					Previous = setting
				});
				Steps.Add((c < stps.Count) ? (int)(100 * float.Parse(stps[c])) : 10);
			}
		}
	}		// class WebMenu
}
