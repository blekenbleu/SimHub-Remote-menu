using SimHub.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu : IPlugin
	{
		int CarPropCount = 0, GamePropCount = 0;
		List<string> CarProp, GameProp; 

		bool OOpa(string msg)		// defer MessageBox.Show() until GetWPFSettingsControl()
		{
			Msg += msg + "\n";
			return true;
		}

		/// <summary>
		/// Called at SimHub startup, then at game changes
		/// On exit, simValues, data and Settings reflect properties defined from Myni.* properties,
		/// obtained by SimHub from e.g. NCalcScripts/WebMenu.ini
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
		{
			CurrentCar = "";		// otherwise whatever was set before game change
			once = true;
			Steps = new List<int> { };
			simValues = new List<Values> {};

			// restore Properties from settings
			// https://github.com/blekenbleu/SimHub-Remote-menu/blob/main/Properties.md
			Settings = this.ReadCommonSettings("GeneralSettings", () => new DataPluginSettings());
			// Load existing slim-formatted JSON file;  Populate() may grab values from it
			// its values for configured properties are supposed more current than .ini
			if (write = Load(path = pluginManager.GetPropertyValue(Myni + "file")?.ToString()))
				OOpa($"Load({path}): " + Msg);

			// Populate() simValues, CarProps, GameProps, iniDefaults from Settings and .ini
			Parse_ini(pluginManager, ref CarProp, ref GameProp);

			if (0 == simValues.Count)
			{
				OOpa("Missing or invalid " + Myni
					 + "properties from NCalcScripts/WebMenu.ini");
				return;
			}

            // now, reconcile JSON data with simValues
            if (0 < pluginManager?.GameName.Length)
		 	{
				if (Settings.game != pluginManager.GameName)
				{
					Settings.carid = "";					// don't force a carid into GameName
					Settings.game = pluginManager.GameName;
				}
				gndx = data.gList.FindIndex(g => g.cList[0].Name == pluginManager.GameName);
			}
			else gndx = -1;

            if (0 <= gndx)						// Recover Current and Default simValues from JSON
				for (int v = 0; v < simValues.Count; v++)
			{
				int Index;

				if (0 > (Index = data.pList.FindIndex(s => s == simValues[v].Name)))
					continue;
				
				if (Index < data.gList[gndx].cList[0].vList.Count)
					simValues[v].Default = data.gList[gndx].cList[0].vList[Index];

				if (Index < data.gList[gndx].rList?.Count)
					simValues[v].Current = data.gList[gndx].rList[Index];
			}

			string sl = pluginManager.GetPropertyValue(Myni + "slider")?.ToString();

			if (0 == sl?.Length)
				slider = 0;
			else if (0 > (slider = simValues.FindIndex(i => i.Name == sl)))
				slider = 0;

			// at this point, simValues has all properties from .ini
			// updated from matching Settings or JSON
			// Now, possibly update Settings or JSON from simValues
			bool update = false;

			if (write)												// bad Load()?
			{
				data = new PluginList()								// @ slim.cs line 20
				{
					Plugin = "WebMenu",
					pList = new List<string> {},
					gList = new List<GameList>() {}					// @ slim.cs line 15
				};
				update = true;
			}
			else if (GamePropCount != data.pList.Count)
				update = true;
			else
			{
				for (int i = 0; i < GamePropCount; i++)
					if (simValues[i].Name != data.pList[i])
					{
						update = true;
						break;
					}

				for (int i = 0; i < data.gList.Count && !update; i++)
					if (data.gList[i].rList?.Count != GamePropCount)
						update = true;
			}
			if (update)												// all GameLists may be out of sync
			{
				UpdateSlim();
				data.pList = CarProp.Concat(GameProp).ToList();		// per-car, then -game property names
			}

			// now reuse update to sort Settings
			update = Settings.Name?.Count != simValues.Count
					|| Settings.defaults?.Count != simValues.Count - GamePropCount;
			for (int i = 0; i < Settings.defaults?.Count && !update; i++)
				if (simValues[i + GamePropCount].Name != Settings.defaults[i])
					update = true;
			for (int i = 0; i < simValues.Count && !update; i++)
				if (simValues[i].Name != Settings.Name[i])
					update = true;

			if (update) // synch Settings from simValues; may not get saved...
				SettingsFrom_simValues(Settings.game, Settings.carid);

			// Declare available properties
			// SimHub properties by AttachDelegate get evaluated "on demand"
			foreach (Values p in simValues)
				this.AttachDelegate(p.Name, () => p.Current);

			this.AttachDelegate("Selected", () => Control.Model.SelectedProperty);
			this.AttachDelegate("New Car", () => NewCar);
			this.AttachDelegate("Game",   () => Gname);
			this.AttachDelegate("Car",	 () => CurrentCar);
			this.AttachDelegate("Msg",	() => Msg);
			Actions();
			Info($"Init():  simValues.Count = {simValues.Count}" + MIDI.Init() + HttpServer.Init());
		}	// Init()
	}		// class WebMenu
}
