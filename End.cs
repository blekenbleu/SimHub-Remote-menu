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
				int i;

				set = true;									// save Current values
				Settings.properties = new List<Property> {};
				Settings.game = Gname;
				Settings.carid = CurrentCar;
				Settings.gDefaults = new List<Property> {};
				for(i = 0; i < simValues.Count; i++)
				{
 					if (0 < simValues[i].Current?.Length)
						Settings.properties.Add(new Property()
						{ Name  = string.Copy(simValues[i].Name),
						  Value = string.Copy(simValues[i].Current)
						});
					if (i >= GamePropCount && 0 < simValues[i].Default?.Length)
						Settings.gDefaults.Add(new Property()
						{ Name  = string.Copy(simValues[i].Name),
				  		  Value = string.Copy(simValues[i].Default)
						});
				}

				if (write)	// capture per-game Default changes
					data.gList[gndx].cList[0].Vlist = GameDefaults();	// Json.cs
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
