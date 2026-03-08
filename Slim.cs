using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace blekenbleu.SimHub_Remote_menu
{
// New slim JSON structure ------------------------------------------
// !!! These must all be declared public for JsonConvert.SerializeObject() !!!
	public class CarL
	{
		public string Name;				// CarID (game name for Carl[0])
		public List<string> vList;		// property values	(game defaults for Carl[0])
	}

	public class GameList																					//	gl
	{
		public List<CarL> cList;		// cList[0] is game Name + default per-car, then per-game property values
	}

	public class PluginList																					//	data
	{
		public string Plugin;			// NCalcScripts/WebMenu.ini identifies itself as "WebMenu.file"
		public List<string> pList;		// per-car, then per-game, property names, from WebMenu.ini				oldpList
										// global property names, defaults and current values are in Settings
		public List<GameList> gList;																		//	newList
	}

	public partial class WebMenu
	{
		PluginList data;

		bool Boop(string s)
		{
			Msg = s;
			return true;
		}

		// load Slim .json and reconcile with CurrentCar-specific simValues from NCalcScripts/WebMenu.ini
		// return true if path fails or unrecoverable JSON
		// .ini may have added, deleted or moved properties among per-car, per-game and global
		// .json may be e.g. obsolete format, out-of-date or bad because code bugs.
		internal bool Load(string path)	// from NCalcScripts\WebMenu.ini 'WebMenu.file'
		{
			if (!File.Exists(path))
				return Boop(" does not exist");

			// this fails if GamesList is not all public
			data = JsonConvert.DeserializeObject<PluginList>(File.ReadAllText(path));
			if (null == data || 0 == data.pList?.Count || 0 == data.gList?.Count)
				return Boop(" failed DeserializeObject()");

			// Now, can only return false, meaning some data with which to work
			string s = "";
			int nullcarID = 0, nullcList = 0;

			if ("WebMenu" != data?.Plugin) {
				s += "'{data?.Plugin}' != 'WebMenu'";
				data.Plugin = "WebMenu";	// user has at least been warned...
			}

			for (int i = 0; i < data.gList.Count; i++)					// all games
			{
				if (2 > data.gList[i].cList?.Count)
				{
					nullcList++;
					data.gList.RemoveAt(i--);
					continue;
				}

				for (int c = 0; c < data.gList[i].cList.Count; c++)	// all cars in game
					if (0 == data.gList[i].cList[c].Name?.Length)
					{
						nullcarID++;
						data.gList[i].cList.RemoveAt(c--);
					}
			}

			if (0 < nullcList)
			{
				if (0 < s.Length)
					s += "\n\t";
				s += $"{nullcList} bad car Lists";
			}
			if (0 < nullcarID)
			{
				if (0 < s.Length)
					s += "\n\t";
				s += $"{nullcarID} empty carIDs";
			}	
			if (0 < s.Length)
				OOpa($"Slim.Load({path}):  " + s);

			return false;
		}	// Load()

		void SettingsFrom_simValues(string game, string carid)
		{
			Settings.properties = new List<Property> {};
			Settings.game = game;
			Settings.carid = carid;
			Settings.gDefaults = new List<Property> {};
			Settings.pcount = CarPropCount;
			Settings.gcount = GamePropCount - CarPropCount;

			for (int i = 0; i < simValues.Count; i++)
			{
 				if (0 < simValues[i].Current?.Length)
					Settings.properties.Add(new Property()
					{ Name  = string.Copy(simValues[i].Name),
					  Value = string.Copy(simValues[i].Current)
					});
				if (i >= GamePropCount && 0 < simValues[i].Default?.Length)
					Settings.gDefaults.Add(new Property()
					{ Name  = string.Copy(simValues[i].Name),
			  		  Value = string.Copy(simValues[i].Default)
					});
			}
		}
	}		// class Slim
}
