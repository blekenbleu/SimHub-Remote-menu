using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	/// <summary>
	/// Settings class, make sure it can be correctly serialized using JSON.net
	/// </summary>
	public class DataPluginSettings
	{
		// global default property values - not stored in .json
		public List<Property> gDefaults = new List<Property>() {};

		// Init() restores as simValues Previous values
		public List<Property> properties = new List<Property>()
		{
			// each current Value stored as string of integer 10x actual value
			// most recent per-car, then per-game, then global values saved by `End()`
		};

		public int pcount = 0;	// per-car property count in properties
		public int gcount = 0;	// per-game property count in properties
		public string game;
		public string carid;

		public List<MidiDev> midiDevs = new List<MidiDev>() {};
	}
}
