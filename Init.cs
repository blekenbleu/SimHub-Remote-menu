using SimHub.Plugins;
using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class OKSHmenu : IPlugin
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
			CurrentCar = null;			// otherwise whatever was set before game change
			// restore Properties from settings
			Settings = this.ReadCommonSettings<DataPluginSettings>(
												"GeneralSettings", () => new DataPluginSettings());

			// restore previously saved car properties
			SettingsProps = new List<Property> {};			// deep copy
			foreach(Property p in Settings.properties)
				if (null != p.Name && null != p.Value)
					SettingsProps.Add(new Property() { Name = p.Name, Value = p.Value });

			Steps = new List<int>() {};		// for Populate()

			// property and setting names, default values and steps from OKSHpm.ini
			string pts, ds = pluginManager.GetPropertyValue(pts = Myni + "properties")?.ToString();
			string vts, vs = pluginManager.GetPropertyValue(vts = Myni + "values")?.ToString();
			string sts, ss = pluginManager.GetPropertyValue(sts = Myni + "steps")?.ToString();
			if ((!(null == ds && (0 == Settings.pcount || OOpa($"per-car properties not found"))))
			 && (!(null == vs && OOpa($"'{vts}' not found")))
			 && (!(null == ss && OOpa($"'{sts}' not found")))
			   )
			{
				// OKSHpm.ini defines per-car Properties
				List<string> CarProps = new List<string>(ds.Split(','));
				pCount = CarProps.Count;						// these are per-car
				List<string> values = new List<string>(vs.Split(','));
				List<string> steps = new List<string>(ss.Split(','));
				if (pCount != values.Count || pCount != steps.Count)
					OOpa($"{pCount} per-car properties;  "
						+$"{values.Count} values;  {steps.Count} steps");
				Populate(CarProps, values, steps);
			}
			if (Settings.pcount != simValues.Count)
			{
				set = true;
				Settings.pcount = simValues.Count;
			}

			// OKSHpm.ini also optionally defines per-game Properties
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
				List<string> Sprops = new List<string>(dss.Split(','));
				List<string> values = new List<string>(vss.Split(','));
				List<string> steps = new List<string>(sss.Split(','));
				if (Sprops.Count != values.Count || Sprops.Count != steps.Count)
					OOpa($"{Sprops.Count} gameprops;  {values.Count} gamevals;"
									+ $"  {steps.Count} gamesteps");
				gCount = (Sprops.Count < values.Count) ? Sprops.Count : values.Count;
				if (gCount > steps.Count)
					gCount = steps.Count + pCount;
				else gCount += pCount;
				Populate(Sprops, values, steps);
			}
			if (Settings.gcount != simValues.Count - Settings.pcount) {
				set = true;
				Settings.gcount = simValues.Count - Settings.pcount;
			}			

			// OKSHpm.ini also optionally defines global settings
			string pgts = Myni + "settings";
			string dgs = pluginManager.GetPropertyValue(pgts)?.ToString();
			string vgts = Myni + "setvals";
			string vgs = pluginManager.GetPropertyValue(vgts)?.ToString();
			string sgts = Myni + "setsteps";
			string sgs = pluginManager.GetPropertyValue(sgts)?.ToString();
			bool noglob = (0 == Settings.gDefaults.Count) && OOpa($"global properties not found");

			if	((!(null == dgs && noglob))
			 &&  (!(null == vgs && OOpa($"'{vgts}' not found")))
			 &&  (!(null == sgs && OOpa($"'{sgts}' not found")))
				)
			{
				List<string> Gprops = new List<string>(dgs.Split(','));
				List<string> values = new List<string>(vgs.Split(','));
				List<string> steps = new List<string>(sgs.Split(','));
				if (Gprops.Count != values.Count || Gprops.Count != steps.Count)
					OOpa($"{Gprops.Count} settings;  {values.Count} setvals;"
									+ $"  {steps.Count} setsteps");
				Populate(Gprops, values, steps);
			}

			if (Settings.gDefaults.Count != simValues.Count - (Settings.gcount + Settings.pcount))
			{
				Settings.gDefaults = new List<Property>() {};
				set = true;
			}

			if (0 == simValues.Count)
			{
				OOpa("Missing or invalid " + Myni
					 + "properties from NCalcScripts/OKSHpm.ini");
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
					OOpa($"Init() Load({path}): " + Msg);
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

			// Declare an event and corresponding action
			Actions();
			// triggered by ExternalScript.CarChange event
			this.AddAction("ChangeProperties",			(a, b) => CarChange(
					pluginManager.GetPropertyValue("CarID")?.ToString(),
					pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString(),
					false
				)
			);

			Info($"Init():  simValues.Count = {simValues.Count}");
		}	// Init()
	}		// class OKSHmenu
}
