using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace blekenbleu.SimHub_Remote_menu
{
// New slim JSON structure ------------------------------------------
// !!! These must all be declared public for JsonConvert.SerializeObject() !!!
	public class CarL
	{
		public string Name { get; set; }		// CarID (game name for Carl[0])
		public List<string> Vlist { get; set; }	// property values	(game defaults for Carl[0])
	}

	public class GameList
	{
		public List<CarL> cList;		// cList[0] is game Name + default per-car, then per-game property values
	}

	public class GamesList
	{
		public string Plugin;			// NCalcScripts/OKSHpm.ini identifies itself as "OKSHpm.file"
		public List<string> pList;		// per-car, then per-game property names, from OKSHpm.ini
		public List<GameList> gList;
	}

	public partial class OKSHmenu
	{
		GamesList data;

		// called in End()
		public void Data()
		{
			data = new GamesList()
			{
				Plugin = "OKSHpm",
				gList = new List<GameList>() { },	// GameList @ slim.cs line 16
				// property names
				pList = new List<string> { }		// per-car, then per-game
			};
			for (int i = 0; i < gCount; i++)
				data.pList.Add(simValues[i].Name);
		}

		// Reconcile .json values with simValues based on .ini and Settings
		private List<string> Reconcile(List<string> vList, int car)
		{
			List<string> New = new List<string> {};
			// car[0] is per-game car default and per-game property values
			int count = (0 == car) ? gCount : pCount;

			if (count > simValues.Count)
				count = simValues.Count;		// it happens 19 Feb 2025

			for (int i = 0; i < count; i++)
			{
				int Index =  data.pList.FindIndex(j => j == simValues[i].Name);

				if (-1 == Index || Index >= vList.Count)
					New.Add(simValues[i].Default);
				else New.Add(vList[Index]);	// reuse as many as possible
			}
			return New;
		}

		// load Slim .json and reconcile with CurrentCar-specific simValues from NCalcScripts/OKSHpm.ini
		// return true if path fails or unrecoverable JSON
		// .ini may have added, deleted or moved properties among per-car, per-game and global
		// .json may be e.g. obsolete format, out-of-date or bad because code bugs.
		internal bool Load(string path)
		{
			if (!File.Exists(path))
				return true;

			var foo = File.ReadAllText(path);
			// this fails if GamesList is not all public
			data = JsonConvert.DeserializeObject<GamesList>(foo);
			if (null == data || null == data.pList || null == data.gList)
				return true;

			// Now, can only return false, meaning data fully reconciled to simValues

			if (null == data.Plugin || "OKSHpm" != data.Plugin) {
				OOpa($"Slim.Load({path}) data.Plugin: '{data.Plugin}' != 'OKSHpm'");
				data.Plugin = "OKSHpm";	// user has at least been warned...
			}

			int nullcarID = 0;
			int pcount = pCount;
			int gcount = gCount;
			int i, g, c;

			if (gcount != data.pList.Count)
				i = -1;
			else for (i = 0; i < data.pList.Count; i++)
				if (data.pList[i] != simValues[i].Name)
					break;

			if (i == gcount)
				for (g = 0; g < data.gList.Count; g++)
					if (null != data.gList[g].cList)
					{
						if (data.gList[g].cList.Count < 2 || data.gList[g].cList[0].Vlist.Count != gcount)
						{ i--; break; }
						else for (c = 0; c < data.gList[g].cList.Count; c++)
								if (data.gList[g].cList[c].Vlist.Count != ((0 == c) ? gcount : pcount))
								{ i--; g = data.gList.Count; break; }
					}

			if (i != gcount)
			// repopulate car properties according to simValues
			{
				if (!set)	// already warned
					OOpa($"Slim.Load({path}):  pList mismatch");
				if (i != pcount)
					for (i = 0; i < data.gList.Count; i++)					// all games
					{
						if (null == data.gList[i].cList)
							continue;
						for (c = 0; c < data.gList[i].cList.Count; c++)	// all cars in game
							if (null == data.gList[i].cList[c].Name)
							{
								nullcarID++;
								data.gList[i].cList.RemoveAt(c--);
							}
							else data.gList[i].cList[c].Vlist = Reconcile(data.gList[i].cList[c].Vlist, c);
					}
				data.pList = new List<string> {};
				for (i = 0; i < gcount; i++)
					data.pList.Add(simValues[i].Name);
			}
			if (0 < nullcarID)
				OOpa($"Slim.Load({path}): {nullcarID} null carIDs");

			if (data.gList.Count < 1 || data.gList[0].cList.Count < 2)
				OOpa($"Slim.Load({path}): empty data.gList");

			return false;
		}	// Load()
	}		// class Slim
}
