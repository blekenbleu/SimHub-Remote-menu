using SimHub.Plugins;
using System.Collections.Generic;
using System.ComponentModel;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu : IPlugin
	{
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
			int CarPropCount, GamePropCount;
			CurrentCar = null;       	   // otherwise whatever was set before game change
			once = true;
			simValues = new List<Values>();
			// restore Properties from settings
			Settings = this.ReadCommonSettings<DataPluginSettings>(
												"GeneralSettings", () => new DataPluginSettings());

			// restore previously saved properties
			SettingsProps = new List<Property> {};			// deep copy
			foreach(Property p in Settings.properties)
				if (null != p.Name && null != p.Value)
					SettingsProps.Add(new Property() { Name = p.Name, Value = p.Value });

			Steps = new List<int>() {};     // for Populate()

            // property and setting names, default values and steps from WebMenu.ini
            string pts, ds = pluginManager.GetPropertyValue(pts = Myni + "properties")?.ToString();
			string vts, vs = pluginManager.GetPropertyValue(vts = Myni + "values")?.ToString();
			string sts, ss = pluginManager.GetPropertyValue(sts = Myni + "steps")?.ToString();
			List<string> CarProps;

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
				Populate(CarProps, values, steps);
			} else CarProps = new List<string>() {};
            if (Settings.pcount != (CarPropCount = simValues.Count))
			{
				set = true;
				Settings.pcount = simValues.Count;
			}

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
				List<string> GameProps = new List<string>(dss.Split(','));
				List<string> values = new List<string>(vss.Split(','));
				List<string> steps = new List<string>(sss.Split(','));
				if (GameProps.Count != values.Count || GameProps.Count != steps.Count)
					OOpa($"{GameProps.Count} gameprops;  {values.Count} gamevals;"
									+ $"  {steps.Count} gamesteps");
				gCount = (GameProps.Count < values.Count) ? GameProps.Count : values.Count;
				if (gCount > steps.Count)
					gCount = steps.Count + CarProps.Count;
				else gCount += CarProps.Count;
				Populate(GameProps, values, steps);
			}
			if (Settings.gcount != simValues.Count - Settings.pcount) {
				set = true;
				Settings.gcount = simValues.Count - Settings.pcount;
			}			

			// WebMenu.ini also optionally defines global settings
			string pgts = Myni + "settings";
			string dgs = pluginManager.GetPropertyValue(pgts)?.ToString();
			string vgts = Myni + "setvals";
			string vgs = pluginManager.GetPropertyValue(vgts)?.ToString();
			string sgts = Myni + "setsteps";
			string sgs = pluginManager.GetPropertyValue(sgts)?.ToString();
			bool noglob = (0 == Settings.gDefaults.Count); // && OOpa($"global properties not found");

			if	((!(null == dgs && noglob))
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
				Populate(GlobalProps, values, steps);
			}

			if (Settings.gDefaults.Count != simValues.Count - (Settings.gcount + Settings.pcount))
			{
				Settings.gDefaults = new List<Property>() {};
				set = true;
			}

			if (0 == simValues.Count)
			{
				OOpa("Missing or invalid " + Myni
					 + "properties from NCalcScripts/WebMenu.ini");
				return;
			}

			// Recover default global values from Settings
			// for properties which remain global since previous game instance.
			{
				int gd, scount = SettingsProps.Count;

				for (gd = 0; gd < Settings.gDefaults.Count; gd++)
				{
					int Index = simValues.FindIndex(s => s.Name == Settings.gDefaults[gd].Name);
					if (Index >= gCount)	// still global?
						simValues[Index].Default = Settings.gDefaults[gd].Value;
				}

				string sl = pluginManager.GetPropertyValue(Myni + "slider")?.ToString();

				if (null != sl)
					slider = simValues.FindIndex(i => i.Name == sl);
			}

			// at this point, simValues has all properties from .ini,
			// with original .ini default and previous property values
			// still-configured from most recent game instance
			// Load existing JSON, using slim format
			// JSON values for still-configured properties are supposed more current than .ini
			if (Load(path = pluginManager.GetPropertyValue(Myni + "file")?.ToString()))
			{
				if (0 < Msg.Length)
					OOpa($"Load({path}): " + Msg);
				Data();										// Slim.cs
			}

			// Declare available properties
			// SimHub properties by AttachDelegate get evaluated "on demand"
			foreach (Values p in simValues)
				this.AttachDelegate(p.Name, () => p.Current);

			this.AttachDelegate("Selected", () => Control.Model.SelectedProperty);
			this.AttachDelegate("New Car", () => NewCar);
			this.AttachDelegate("Car", () => CurrentCar);
			this.AttachDelegate("Game", () => Gname);
			this.AttachDelegate("Msg", () => Msg);
			Actions();
			Info($"Init():  simValues.Count = {simValues.Count}");
		}	// Init()
	}		// class WebMenu
}
