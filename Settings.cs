using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	/// <summary>
	/// Settings class, make sure it can be correctly serialized using JSON.net
	/// </summary>
	public class DataPluginSettings
	{
		// Init() restores as simValues Previous values
		public List<string> Name = new List<string>() {};
		public List<string> Value = new List<string>()
		{
			// each current Value stored as string of integer 10x actual value
			// most recent per-car, then per-game, then Current global values
			// saved by `SettingsFrom_simValues()`
		};

		// global default property values - not stored in .json
		public List<string> defaults = new List<string>() {};

		public int pcount = 0;	// per-car property count in properties
		public int gcount = 0;	// per-game property count in properties
		public string game;
		public string carid;

		public List<MidiDev> midiDevs = new List<MidiDev>() {};
	}
}
