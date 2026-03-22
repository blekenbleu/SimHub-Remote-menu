using SimHub.Plugins;
using System;
using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	internal class DI
	{
		internal string Name;
		internal int Value;
	}
	public partial class WebMenu
	{
		int joy1;
		List<DI> listDI = new List<DI> {};
		List<DI> learnDI = new List<DI> {};
		List<DI> learnDIb = new List<DI> { new DI { Name = "dummy", Value = 0 } };

		internal void InitDI()
		{
			joy1 = -1;
		}

		internal void InitDI(PluginManager pluginManager)
		{
			if (-20 < --joy1)
			{
				List<string> foo = pluginManager.GetAllPropertiesNames().FindAll(s => s.StartsWith("JoystickPlugin."));
				if (0 < foo.Count)
				{
					Info($"InitDI():  {foo.Count} JoystickPlugin properties");
					joy1 = foo.Count;
					for (int i = 0; i < foo.Count; i++)		// populate axis learning list
						learnDI.Add(new DI
						{
							Name = foo[i],
							Value = Convert.ToInt32(pluginManager.GetPropertyValue(foo[i]))
						});
				}
			} else if (-50 > joy1)
				joy1 = 0;
		}

		internal void RunDI(PluginManager pluginManager)
		{
			int foo;

			if (0 < learnDI.Count)							// scan list for value changes
			{
				for (int i = 0; i < learnDI.Count; i++)
					if (learnDI[i].Value != (foo = Convert.ToInt32(pluginManager.GetPropertyValue(learnDI[i].Name))))
					{
						learnDI[i].Value = foo;
						foo >>= 9;
						Info($"RunDI:  {learnDI[i].Name}, {foo:X8}");
					}
				for (int i = 1; i < learnDIb.Count; i++)
					if (learnDIb[i].Value != (foo = Convert.ToInt32(pluginManager.GetPropertyValue(learnDIb[i].Name))))
					{
						learnDIb[i].Value = foo;
						if (foo > 0)			// care only about presses, NOT releases
							Info($"RunDI:  {learnDIb[i].Name}, {127:X8}");
					}
			// JoyStick buttons are not visible until first actuated
				List<string> soo = pluginManager.GetAllPropertiesNames().FindAll(s => s.StartsWith("InputStatus."));
				if (soo.Count != learnDIb.Count)
					for (int i = 0; i < soo.Count; i++)
					{
						int index = learnDIb.FindIndex(s => s.Name == soo[i]);

						if (0 > index)
						{
							int button = Convert.ToInt32(pluginManager.GetPropertyValue(soo[i]));
							Info($"RunDI button:  {soo[i]}, {button:X8}");
							learnDIb.Add(new DI
							{
								Name = soo[i],
								Value = button
							});
						}
					}
			}
		}
	}
}
