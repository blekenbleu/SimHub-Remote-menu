using SimHub.Plugins;
using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
		/// <summary>
		/// Called at plugin manager stop, close/dispose anything needed here !
		/// Plugins are rebuilt at game changes
		/// </summary>
		/// <param name="pluginManager"></param>
		public void End(PluginManager pluginManager)
		{
			if (0 < Gname.Length && 0 < CurrentCar?.Length && (SaveSlim() || set)) {		// Save settings
				SettingsFrom_simValues(Gname, CurrentCar);

				if (write)	// capture per-game Default changes
					data.gList[gndx].cList[0].vList = GameDefaults();	// Json.cs
			}

			if (MIDI.Stop() || set)			// .ini mismatches Settings or game run
				this.SaveCommonSettings("GeneralSettings", Settings);

			if (write && 0 < CurrentCar?.Length)
			{
				// SerializeObject requires public data and GamesList
				string sjs = Newtonsoft.Json.JsonConvert.SerializeObject(data,
							 Newtonsoft.Json.Formatting.Indented);
				if (0 == sjs.Length || "{}" == sjs)
					OOps("End():  Json Serializer failure");
				else System.IO.File.WriteAllText("R:\\Temp\\WebMenu.json", sjs);	// path, sjs);
			}
		}
	}
}
