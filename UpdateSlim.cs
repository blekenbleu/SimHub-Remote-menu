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
				GameList gl = new GameList() { cList = new List<CarL>() {}, rList = new List<string> {} };
				// cList[0] is game name, then default values
				for (int i = 0; i < data.gList[g].cList.Count; i++)
					gl.cList.Add(new CarL()
					{	Name = data.gList[g].cList[i].Name,
						vList = new List<string> {}
					});
				newList.Add(gl);
			}

			int which = -1, where = -1, gc = data.gList.FindIndex(s => s.cList[0].Name == Settings.game);
			List<int> n = new List<int> {};

			for (int i = 0; i < GamePropCount; i++)	// sort collected properties
			{
				int pi = data.pList.FindIndex(s => s == simValues[i].Name);
				
				n.Add(pi);
				// seek values for each simValues[i].Name
				if (0 <= pi)
				{
					which = 0;
					where = pi;
				}
				else if (0 <= (pi = Settings.Name.FindIndex(s => s == simValues[i].Name)))
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
						for (int j = 0; j < data.gList[g].cList.Count; j++)
							newList[g].cList[j].vList.Add(string.Copy(data.gList[g].cList[j].vList[where]));
				else if (1 == which)
					for (int g = 0; g < data.gList.Count; g++)
						for (int j = 0; j < data.gList[g].cList.Count; j++)
							newList[g].cList[j].vList.Add(string.Copy(Settings.defaults[where]));
				else for (int g = 0; g < data.gList.Count; g++)
					for (int j = 0; j < data.gList[g].cList.Count; j++)
						newList[g].cList[j].vList.Add(string.Copy(simValues[i].Default));
			}

			// set per-game most recent property value lists
			int c = data.gList[gc].cList.FindIndex(s => s.Name == Settings.carid);
			for (int i = 0; i < newList.Count; i++)
				for (int j = 0; j < GamePropCount; j++)
					newList[i].rList.Add(string.Copy(
						(0 <= n[j] && j < data.gList[n[j]].rList?.Count) ?
							data.gList[n[j]].rList[j] :
						((i == gc && 1 <= c) || 2 > newList[i].cList.Count) ?
							Settings.Value[j] : newList[i].cList[1].vList[j])
					);
			data.gList = newList;
		}
	}
}
