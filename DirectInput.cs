using SimHub.Plugins;
using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
		int joy1;

		internal void InitDI()
		{
			joy1 = 0;
		}

		internal void InitDI(PluginManager pluginManager)
		{
			if (-20 < --joy1)
			{
				List<string> foo = pluginManager.GetAllPropertiesNames().FindAll(s => s.StartsWith("JoystickPlugin."));
				if (0 < foo.Count)
				{
					Msg = $"InitDI():  {foo.Count} JoystickPlugin properties";
					joy1 = 0;
				}
			} else if (-50 > joy1)
				joy1 = 0;
		}

		internal void RunDI(PluginManager pluginManager)
		{
		}
	}
}
