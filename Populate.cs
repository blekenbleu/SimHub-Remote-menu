using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
		List<int> Steps;								// 100 times actual values

		/// <summary>
		/// DisplayGrid contents
		/// </summary>
		public static List<Values> simValues;
		internal List<string> iniDefaults;				// ini values for missing defaults

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
				iniDefaults.Add((c < vals.Count) ? vals[c] : "0");

				// populate DisplayGrid ItemsSource
				// WebMenu.ini contents may not match saved car properties
				// default value from .ini
				int index = Settings.Name.FindIndex(i => i == iniProps[c]);

				string ini = (c < vals.Count) ? vals[c] :  (0 > index || index >= Settings.defaults.Count) ? "0" : Settings.defaults[index];
				// use Settings.properties value, if it exists, else from .ini
				string setting = (0 > index) ? ini: Settings.Value[index];

				simValues.Add(new Values
				{
					Name = iniProps[c],
					Default = ini,			// replaced by JSON values for c < GamePropCount
					Current = setting,
					Previous = setting		// replaced by JSON values for c < GamePropCount
				});
				Steps.Add((c < stps.Count) ? (int)(100 * float.Parse(stps[c])) : 10);
			}
		}
	}		// class WebMenu
}
