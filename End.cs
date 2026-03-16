using SimHub.Plugins;

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
			if (0 < Gname?.Length && 0 < CurrentCar?.Length && ((UpdateGame()) || set)) 	// Save settings
				set = set || SettingsFrom_simValues(Gname, CurrentCar);

			if (MIDI.Stop(this) || set)			// .ini mismatches Settings or game run
			{
				this.SaveCommonSettings("GeneralSettings", Settings);
/*
				string sjs = Newtonsoft.Json.JsonConvert.SerializeObject(Settings,
							 Newtonsoft.Json.Formatting.Indented);
				if (0 == sjs.Length || "{}" == sjs)
                    OOps("End(Settings):  Json Serializer failure");
                else System.IO.File.WriteAllText("R:\\Temp\\Settings.json", sjs);
 */
			}

			if (write && 0 < CurrentCar?.Length)
			{
				// SerializeObject requires public data and GamesList
				string sjs = Newtonsoft.Json.JsonConvert.SerializeObject(data,
							 Newtonsoft.Json.Formatting.Indented);
				if (0 == sjs.Length || "{}" == sjs)
					OOps("End(data):  Json Serializer failure");
//				else System.IO.File.WriteAllText("R:\\Temp\\WebMenu.json", sjs);
				else System.IO.File.WriteAllText(path, sjs);
			}
		}
	}
}
