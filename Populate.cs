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
		List<Property> SettingsProps;					// non-null Settings entries
		List<int> Steps;								// 100 times actual values

		/// <summary>
		/// DisplayGrid contents
		/// </summary>
		public static List<Values> simValues;

		// add properties and settings to simValues; initialize Steps
		// if a property move among
		void Populate(List<string>props, List<string> vals, List<string> stps)
		{
			for (int i = 0; i < props.Count; i++)
				if (0 == props[i]?.Length)
				{
					props.RemoveAt(i);
					if (i < vals.Count)
						vals.RemoveAt(i);
					if (i < stps.Count)
						stps.RemoveAt(i);
					i--;
				}

			for (int c = 0; c < props.Count; c++)
			{
				// populate DisplayGrid ItemsSource
				// WebMenu.ini contents may not match saved car properties
				// default value from .ini
				int index = SettingsProps.FindIndex(i => i.Name == props[c]);

				string ini = (c < vals.Count) ? vals[c] :  (0 > index) ? "0" : SettingsProps[index].Value;
				// use SettingsProps value, if it exists, else from .ini
				string setting = (0 > index) ? ini: SettingsProps[index].Value;

				simValues.Add(new Values
				{
					Name = props[c],
					Default = ini,			// replaced by JSON values
					Current = setting,
					Previous = setting
				});
				Steps.Add((c < stps.Count)  ? (int)(100 * float.Parse(stps[c])) : 10);
			}
		}
	}		// class WebMenu
}
