using SimHub.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu : IPlugin
	{
		int CarPropCount = 0, GamePropCount = 0;
		bool OOpa(string msg)   // defer MessageBox.Show() until GetWPFSettingsControl()
		{
			Msg += msg + "\n";
			return true;
		}

		/// <summary>
		/// Called once at SimHub startup, then at game changes
		/// On exit, simValues, data and Settings reflect properties defined from Myni.* properties,
		/// obtained by SimHub from e.g. NCalcScripts/WebMenu.ini
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
		{
			List<string> CarProp = new List<string> {}, GameProp = new List<string> {};

			// forget previous game
			Gname = CurrentCar = null;			// CarChange() will set
			once = true;
			Steps = new List<int>() {};			// for Populate()
			simValues = new List<Values>();

			// restore Properties from settings
			// https://github.com/blekenbleu/SimHub-Remote-menu/blob/main/Properties.md
			Settings = this.ReadCommonSettings<DataPluginSettings>(
												"GeneralSettings", () => new DataPluginSettings());
			if (0 == Settings?.game.Length && 0 < pluginManager?.GameName.Length)
				Settings.game = pluginManager.GameName;

			// Load existing slim-formatted JSON file;  Populate() may grab values from it
			// its values for configured properties are supposed more current than .ini
			if (set  = Load(path = pluginManager.GetPropertyValue(Myni + "file")?.ToString()))
				OOpa($"Load({path}): " + Msg);

			Parse_ini(pluginManager, ref CarProp, ref GameProp);	// Populate() simValues

			if (0 == simValues.Count)
			{
				OOpa("Missing or invalid " + Myni
					 + "properties from NCalcScripts/WebMenu.ini");
				return;
			}

			// Recover default global values from Settings or JSON
			for (int gd = GamePropCount; gd < simValues.Count; gd++)
			{
				int Index = Settings.gDefaults.FindIndex(s => s.Name == simValues[gd].Name);

				if (0 <= Index)
					simValues[gd].Default = Settings.gDefaults[Index].Value;
				else if ((!set)		// fallback:  perhaps was previously per-car or -game
					  && 0 <= (Index = data.pList.FindIndex(s => s == simValues[gd].Name))
					  && 0 <= (gndx = data.gList.FindIndex(g => g.cList[0].Name == Settings.game))
					  && gndx < data.gList[gndx].cList[0].vList.Count)
						simValues[gd].Default = data.gList[gndx].cList[0].vList[Index];
			}

			string sl = pluginManager.GetPropertyValue(Myni + "slider")?.ToString();

			if (0 == sl?.Length)
				slider = 0;
			else if (0 > (slider = simValues.FindIndex(i => i.Name == sl)))
				slider = 0;

			// at this point, simValues has all properties from .ini
			// with Previous and Current values from matching Settings
			// and Defaults from matching Settings or JSON
			bool update = false;

			if (set)												// bad Load()?
			{
				data = new PluginList()								// @ slim.cs line 20
				{
					Plugin = "WebMenu",
					gList = new List<GameList>() { },				// @ slim.cs line 15
					pList = CarProp.Concat(GameProp).ToList()		// per-car, then -game property names
                };
			}
			else if (GamePropCount != data.pList.Count)
				update = true;
			else for (int i = 0; i < GamePropCount && !update; i++)
				if (simValues[i].Name != data.pList[i])
					update = true;

			if (update)
			{
				UpdateSlim();
				data.pList = CarProp.Concat(GameProp).ToList();
			}

			update = Settings.properties?.Count != simValues.Count
					|| Settings.gDefaults?.Count != simValues.Count - GamePropCount;
			for (int i = 0; i < Settings.gDefaults?.Count && !update; i++)
				if (simValues[i + GamePropCount].Name != Settings.gDefaults[i].Name)
					update = true;
			for (int i = 0; i < simValues.Count && !update; i++)
				if (simValues[i].Name != Settings.properties[i].Name)
					update = true;
			if (update) // recreate Settings from simValues; may not get saved...
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
			Info($"Init():  simValues.Count = {simValues.Count}");
		}	// Init()
	}		// class WebMenu
}
