using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public class MMvalues
	{
		public string MidiIn;
		public string word;
		public string button;
	}

	public partial class Control
	{
		public List<MMvalues> MidiMenu;

		void Init_mg()
		{
			MidiMenu = new List<MMvalues> {};
			for (int i = 0; i < OK.Settings.midiDevs.Count; i++)
				MidiMenu.Add (MIDI.MMvalue(OK.Settings.midiDevs[i]));
			
			mg.ItemsSource = MidiMenu;
		}
	}
}
