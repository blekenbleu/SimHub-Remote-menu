using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
		List<int> Steps;						// 100 times actual values
		List<string> iniDefaults;				// values for missing defaults

		/// <summary>
		/// DisplayGrid dg ItemsSource contents
		/// </summary>
		public static List<Values> simValues;

		// add properties and settings to simValues from Settings and .ini; initialize Steps
		// iniProps Names from .ini are definitive;  matching Settings values are assumed most recent
		// reconcile property Name moves among per-car, per-game and default
		void Populate(List<string>iniProps, List<string> vals, List<string> stps)
		{
			for (int c = 0; c < iniProps.Count; c++)
			{
				if (0 == iniProps[c]?.Length)	// missing property Name?
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

				// populate simValues for DisplayGrid ItemsSource
				// WebMenu.dflt contents may mismatch those in Settings
				// use Settings.properties value, if it exists, else default
				string setting, dflt;
				// try matching Name from .ini
				int index = Settings.Name.FindIndex(i => i == iniProps[c]);

				if(0 <= index)
				{
					dflt = index < Settings.defaults.Count ? Settings.defaults[index] : iniDefaults[c];
					setting = index < Settings.Value.Count ? Settings.Value[index] : dflt;
				} else setting = dflt = iniDefaults[c];

				simValues.Add(new Values
				{
					Name = iniProps[c],
					Default = dflt,			// replaced by JSON values for c < GamePropCount in CarChange()
					Previous = setting,		// replaced by Current in CarChange()
					Current = setting		// replaced by JSON values in Init() and CarChange()
				});
				Steps.Add((c < stps.Count) ? (int)(100 * float.Parse(stps[c])) : 10);
			}
		}
	}		// class WebMenu
}
