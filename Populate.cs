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
				if (string.IsNullOrEmpty(iniProps[c]))	// missing property Name?
					continue;

				// populate simValues for DisplayGrid ItemsSource
				// WebMenu.dflt contents may mismatch those in Settings
				string defaultValue = c < vals.Count && !string.IsNullOrEmpty(vals[c]) ? vals[c] : "0";
				// try matching Name from .ini
				int index = Settings.Name.FindIndex(n => n == iniProps[c]);
				bool exists = index >= 0 && index < Settings.Value.Count && !string.IsNullOrEmpty(Settings.Value[index]);
				// use Settings.properties value, if it exists, else defaultValue
				string setting = exists ? Settings.Value[index] : defaultValue;
				int step = c < stps.Count ? (int)(100 * float.Parse(stps[c])) : 10;
				
				iniDefaults.Add(defaultValue);
				simValues.Add(new Values
				{
					Name = iniProps[c],
					Default = defaultValue,	// replaced by JSON values for c < GamePropCount in CarChange()
					Previous = setting,		// replaced by Current in CarChange()
					Current = setting		// replaced by JSON values in Init() and CarChange()
				});
				Steps.Add(step);
			}
		}
	}		// class WebMenu
}
