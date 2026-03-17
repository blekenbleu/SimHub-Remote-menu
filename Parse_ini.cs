using SimHub.Plugins;
using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
    public partial class WebMenu : IPlugin
    {
        // reconcile WebMenu.ini and Settings
        void Parse_ini(PluginManager pluginManager, ref List<string> CarProps, ref List<string> GameProps)
		{
			// property and setting names, default values and steps from WebMenu.ini
			string ds = pluginManager.GetPropertyValue(Myni + "properties")?.ToString();
			string vts, vs = pluginManager.GetPropertyValue(vts = Myni + "values")?.ToString();
			string sts, ss = pluginManager.GetPropertyValue(sts = Myni + "steps")?.ToString();

			iniDefaults = new List<string> {};
			if ((!(null == ds && OOpa($"per-car properties not found")))
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
			CarPropCount = simValues.Count;

			// WebMenu.ini also optionally defines per-game Properties
			string dss = pluginManager.GetPropertyValue(Myni + "gameprops")?.ToString();
			string vtts = Myni + "gamevals";
			string vss = pluginManager.GetPropertyValue(vtts)?.ToString();
			string stts = Myni + "gamesteps";
			string sss = pluginManager.GetPropertyValue(stts)?.ToString();
			if ((!(null == dss && OOpa($"per-game properties not found")))
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
				Populate(GameProps, values, steps);
			} else GameProps = new List<string>() {};
			GamePropCount = simValues.Count;

			// WebMenu.ini also optionally defines global settings
			string dgs = pluginManager.GetPropertyValue(Myni + "settings")?.ToString();
			string vgts = Myni + "setvals";
			string vgs = pluginManager.GetPropertyValue(vgts)?.ToString();
			string sgts = Myni + "setsteps";
			string sgs = pluginManager.GetPropertyValue(sgts)?.ToString();

			if (0 < dgs?.Length
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
		}
	}
}
