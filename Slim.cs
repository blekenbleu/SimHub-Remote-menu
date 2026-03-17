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
		public List<string> rList;		// most recent per-car+game property values for this game
		public List<CarL> cList;		// cList[0] is game Name + Default per-car+game property values
										// other cList[] are CarID + current per-car property values
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

		// load Slim .json and reconcile with simValues from NCalcScripts/WebMenu.ini
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
			int nullcarID = 0, nullgList = 0;

			if ("WebMenu" != data?.Plugin) {
				s += "'{data?.Plugin}' != 'WebMenu'";
				data.Plugin = "WebMenu";	// user has at least been warned...
			}

			for (int i = 0; i < data.gList.Count; i++)					// all games
			{
				if (1 > data.gList[i].cList?.Count || 0 == data.gList[i].cList[0].Name?.Length)
				{
					nullgList++;
					data.gList.RemoveAt(i--);
					continue;
				}

				for (int c = 1; c < data.gList[i].cList.Count; c++)	// all cars in game
					if (0 == data.gList[i].cList[c].Name?.Length)
					{
						nullcarID++;
						data.gList[i].cList.RemoveAt(c--);
					}
			}

			if (0 < nullgList)
			{
				if (0 < s.Length)
					s += "\n\t";
				s += $"{nullgList} bad game Lists";
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

		bool SettingsFrom_simValues(string game, string carid)
		{
			bool change = 0 == Settings.carid.Length || carid != Settings.carid
					|| Settings.game != game || Settings.pcount != CarPropCount
					|| Settings.gcount != GamePropCount - CarPropCount;

			List<string> Name = new List<string> {};
			List<string> Value = new List<string> {};
			List<string> defaults = new List<string> {};

			for (int i = 0; i < simValues.Count; i++)
			{
				if (i >= Settings.Value.Count || i >= Settings.defaults.Count
				 || Settings.Value[i] != simValues[i].Current
				 || Settings.defaults[i] != simValues[i].Default)
					change = true;
				Name.Add(string.Copy(simValues[i].Name));
				Value.Add(string.Copy(simValues[i].Current));
				defaults.Add(string.Copy(simValues[i].Default));
			}

			if (change)
			{
				Settings.Name = Name;
				Settings.Value = Value;
				Settings.defaults = defaults;
				Settings.game = game;
				if (0 < carid.Length)
					Settings.carid = carid;
				Settings.pcount = CarPropCount;
				Settings.gcount = GamePropCount - CarPropCount;
			}
			return change;
		}
	}		// class Slim
}
