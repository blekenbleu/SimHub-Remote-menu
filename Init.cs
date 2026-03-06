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
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
		{

			CurrentCar = null;	   	   // otherwise whatever was set before game change
			once = true;
			simValues = new List<Values>();

			// restore Properties from settings
			// https://github.com/blekenbleu/SimHub-Remote-menu/blob/main/Properties.md
			Settings = this.ReadCommonSettings<DataPluginSettings>(
												"GeneralSettings", () => new DataPluginSettings());

			// restore previously saved properties
			List<string> SettingsDefaults = new List<string> {};
			if (0 < pluginManager?.GameName.Length)
			{
				Gname = pluginManager.GameName;		// we got game
				if (Gname != Settings?.game)
					set = true;
			}
			foreach(Property p in Settings.properties)
				if (null == p.Name || null == p.Value ||
					0 == p.Name.Length || 0 == p.Value.Length)
				{
					set = true;
					break;
				}
				
			Steps = new List<int>() {};	 // for Populate()
			List<string> CarProps, GameProps;

			// property and setting names, default values and steps from WebMenu.ini
			string pts, ds = pluginManager.GetPropertyValue(pts = Myni + "properties")?.ToString();
			string vts, vs = pluginManager.GetPropertyValue(vts = Myni + "values")?.ToString();
			string sts, ss = pluginManager.GetPropertyValue(sts = Myni + "steps")?.ToString();

			if ((!(null == ds && (0 == Settings.pcount || OOpa($"per-car properties not found"))))
			 && (!(null == vs && OOpa($"'{vts}' not found")))
			 && (!(null == ss && OOpa($"'{sts}' not found")))
			   )
			{
				// WebMenu.ini defines per-car Properties
				CarProps = new List<string>(ds.Split(','));
				List<string> values = new List<string>(vs.Split(','));
				List<string> steps = new List<string>(ss.Split(','));
				if (CarProps.Count != values.Count || CarProps.Count != steps.Count)
					OOpa($"{CarProps.Count} per-car properties;  "
						+$"{values.Count} values;  {steps.Count} steps");
				Populate(CarProps, values, steps, 0, Settings.pcount);
			} else CarProps = new List<string>() {};
			CarPropCount = simValues.Count;

			// WebMenu.ini also optionally defines per-game Properties
			string ptts = Myni + "gameprops";
			string dss = pluginManager.GetPropertyValue(ptts)?.ToString();
			string vtts = Myni + "gamevals";
			string vss = pluginManager.GetPropertyValue(vtts)?.ToString();
			string stts = Myni + "gamesteps";
			string sss = pluginManager.GetPropertyValue(stts)?.ToString();
			if ((!(null == dss && (0 == Settings.gcount || OOpa($"per-game properties not found"))))
			 && (!(null == vss && OOpa($"'{vtts}' not found")))
			 && (!(null == sss && OOpa($"'{stts}' not found")))
			)
			{
				GameProps = new List<string>(dss.Split(','));
				List<string> values = new List<string>(vss.Split(','));
				List<string> steps = new List<string>(sss.Split(','));
				if (GameProps.Count != values.Count || GameProps.Count != steps.Count)
					OOpa($"{GameProps.Count} gameprops;  {values.Count} gamevals;"
									+ $"  {steps.Count} gamesteps");
				Populate(GameProps, values, steps, Settings.pcount, Settings.gcount);
			} else GameProps = new List<string>() {};
			GamePropCount = simValues.Count;

			// WebMenu.ini also optionally defines global settings
			string pgts = Myni + "settings";
			string dgs = pluginManager.GetPropertyValue(pgts)?.ToString();
			string vgts = Myni + "setvals";
			string vgs = pluginManager.GetPropertyValue(vgts)?.ToString();
			string sgts = Myni + "setsteps";
			string sgs = pluginManager.GetPropertyValue(sgts)?.ToString();

			if	((!(null == dgs && 0 == Settings.gDefaults.Count))
			 &&  (!(null == vgs && OOpa($"'{vgts}' not found")))
			 &&  (!(null == sgs && OOpa($"'{sgts}' not found")))
			)
			{
				List<string> GlobalProps = new List<string>(dgs.Split(','));
				List<string> values = new List<string>(vgs.Split(','));
				List<string> steps = new List<string>(sgs.Split(','));
				if (GlobalProps.Count != values.Count || GlobalProps.Count != steps.Count)
					OOpa($"{GlobalProps.Count} settings;  {values.Count} setvals;"
									+ $"  {steps.Count} setsteps");
				Populate(GlobalProps, values, steps, Settings.pcount + Settings.gcount,
						Settings.properties.Count - (Settings.pcount + Settings.gcount));
			}

			// Load existing JSON, using slim format
			// JSON values for configured properties are supposed more current than .ini
			if (Load(path = pluginManager.GetPropertyValue(Myni + "file")?.ToString()))
			{
				OOpa($"Load({path}): " + Msg);
				set = true;
				data = new PluginList()
				{
					Plugin = "WebMenu",
					gList = new List<GameList>() { },				// GameList @ slim.cs line 16
					// property names
					pList = CarProps.Concat(GameProps).ToList()        // per-car, then per-game
                };
			}

			if (0 == simValues.Count)
			{
				OOpa("Missing or invalid " + Myni
					 + "properties from NCalcScripts/WebMenu.ini");
				return;
			}

			// Recover default global values from Settings for matching simValues
			for (int gd = 0; gd < Settings.gDefaults.Count; gd++)
			{
				int Index = simValues.FindIndex(s => s.Name == Settings.gDefaults[gd].Name);
				if (0 <= Index)
					simValues[Index].Default = Settings.gDefaults[gd].Value;
			}

			string sl = pluginManager.GetPropertyValue(Myni + "slider")?.ToString();

			if (0 == sl?.Length)
				slider = 0;
			else if (0 > (slider = simValues.FindIndex(i => i.Name == sl)))
				slider = 0;

			// at this point, simValues has all properties from .ini with .ini defaults,
			// previous Settings defaults and json values
			bool update = false;

			if (GamePropCount != data.pList.Count)
				update = true;
			else for (int i = 0; i < GamePropCount && !update; i++)
				if (simValues[i].Name != data.pList[i])
					update = true;

			if (update)
			{
				// sort changed properties into potentially empty data PluginList
				List<GameList> newList = new List<GameList> {};
				for (int g = 0; g < data.gList.Count; g++)
				{
					GameList gl = new GameList() { cList = new List<CarL>() { } };
					// cList[0] is game name, then default values
					for (int c = 0; c < data.gList[g].cList.Count; c++)
						gl.cList.Add(new CarL()
						{	Name = data.gList[g].cList[c].Name,
							Vlist = new List<string> {}
						});
					newList.Add(gl);
				}

				int which = -1, where = -1;

				for (int i = 0; i < GamePropCount; i++)	// sort collected properties
				{
					int pi = data.pList.FindIndex(s => s == simValues[i].Name);

					// seek values for each simValues[i].Name
					if (0 <= pi)
					{
						which = 0;
						where = pi;
					}
					else if (0 <= (pi = Settings.gDefaults.FindIndex(s => s.Name == simValues[i].Name)))
					{					// previously a global
						which = 1;
						where = pi;
					}
					else {				// snag .ini defaults
						which = 2;
						where = i;
					}

					if (0 == which)
						for (int g = 0; g < data.gList.Count; g++)
							for (int c = 0; c < data.gList[g].cList.Count; c++)
								newList[g].cList[c].Vlist.Add(data.gList[g].cList[c].Vlist[where]);
					else if (1 == which)
						for (int g = 0; g < data.gList.Count; g++)
							for (int c = 0; c < data.gList[g].cList.Count; c++)
								newList[g].cList[c].Vlist.Add(Settings.gDefaults[where].Value);
					else for (int g = 0; g < data.gList.Count; g++)
						for (int c = 0; c < data.gList[g].cList.Count; c++)
							newList[g].cList[c].Vlist.Add(simValues[i].Default);
				}
				data.gList = newList;
				data.pList = CarProps.Concat(GameProps).ToList();

				set = true;
			}

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
