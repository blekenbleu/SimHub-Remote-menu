using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
    public partial class WebMenu
    {
		void UpdateSlim()
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
						vList = new List<string> {}
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
							newList[g].cList[c].vList.Add(data.gList[g].cList[c].vList[where]);
				else if (1 == which)
					for (int g = 0; g < data.gList.Count; g++)
						for (int c = 0; c < data.gList[g].cList.Count; c++)
							newList[g].cList[c].vList.Add(Settings.gDefaults[where].Value);
				else for (int g = 0; g < data.gList.Count; g++)
					for (int c = 0; c < data.gList[g].cList.Count; c++)
						newList[g].cList[c].vList.Add(simValues[i].Default);
			}
			data.gList = newList;
		}
	}
}
