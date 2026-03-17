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

			int c, gi = data.gList.FindIndex(s => s.cList[0].Name == Settings.game);
			List<int> n = new List<int> {};

			for (int i = 0; i < GamePropCount; i++)	// collect JSON properties
			{
				// seek values for each simValues[i].Name
				int g, pi = data.pList.FindIndex(s => s == simValues[i].Name);
				
				n.Add(pi);		// data.pList Name indices in simValues for per-game

				if (0 <= pi)	// found in JSON?
					for (g = 0; g < data.gList.Count; g++)
						for (c = 0; c < data.gList[g].cList.Count; c++)
							if (0 == c || i < CarPropCount)
								newList[g].cList[c].vList.Add(string.Copy(data.gList[g].cList[c].vList[pi]));

				// previously a global?
				else if (0 <= (pi = Settings.Name.FindIndex(s => s == simValues[i].Name)))
					for (g = 0; g < data.gList.Count; g++)
						for (c = 0; c < data.gList[g].cList.Count; c++)
							if (0 == c || i < CarPropCount)
								newList[g].cList[c].vList.Add(string.Copy(Settings.defaults[pi]));

				// new property: use .ini defaults
				else for (g = 0; g < data.gList.Count; g++)
						for (c = 0; c < data.gList[g].cList.Count; c++)
							if (0 == c || i < CarPropCount)
								newList[g].cList[c].vList.Add(string.Copy(simValues[i].Default));
			}

			// set per-game most recent property value lists
			// Settings.carid index into JSON
			c = 0 > gi || 0 == Settings.carid.Length ? -1 : data.gList[gi].cList.FindIndex(s => s.Name == Settings.carid);
			for (int i = 0; i < newList.Count; i++)
				for (int g = 0; g < GamePropCount; g++)
					newList[i].rList.Add(string.Copy(
						(0 <= n[g] && g < data.gList[n[g]].rList?.Count) ?
							data.gList[n[g]].rList[g] :
						((i == gi && 1 <= c) || 2 > newList[i].cList.Count) ?
							Settings.Value[g] : newList[i].cList[1].vList[g])
					);
			data.gList = newList;
		}
	}
}
