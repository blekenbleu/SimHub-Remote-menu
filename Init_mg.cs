using System.Collections.Generic;
using System.ComponentModel;

namespace blekenbleu.SimHub_Remote_menu
{
	// programatically define XAML DataGrid `mg` columns;  see Value.cs
	public class MMvalues : INotifyPropertyChanged
	{
		private string _MidiIn = "MidiIn", _word = "Word", _button = "Button";
		public event PropertyChangedEventHandler PropertyChanged;
		private readonly PropertyChangedEventArgs Bevent = new PropertyChangedEventArgs("Button");
		private readonly PropertyChangedEventArgs Mevent = new PropertyChangedEventArgs("MidiIn");
		private readonly PropertyChangedEventArgs Wevent = new PropertyChangedEventArgs("Word");

		public string MidiIn
		{
			get { return _MidiIn; }
			set
			{
				if (string.Compare(_MidiIn, value) != 0)
				{
					_MidiIn = value;
					PropertyChanged?.Invoke(this, Mevent);
				}
			}
		}
		public string Word
		{
			get { return _word; }
			set
			{
				if (string.Compare(_word, value) != 0)
				{
					_word = value;
					PropertyChanged?.Invoke(this, Wevent);
				}
			}
		}
		public string Button
		{
			get { return _button; }
			set
			{
				if (string.Compare(_button, value) != 0)
				{
					_button = value;
					PropertyChanged?.Invoke(this, Bevent);
				}
			}
		}
	}

	public partial class Control
	{
		public static List<MMvalues> MidiMenu;

		void Init_mg()
		{
			MidiMenu = new List<MMvalues> {};
			for (int i = 0; i < OK.Settings.midiDevs.Count; i++)
				MidiMenu.Add (MIDI.MMvalue(OK.Settings.midiDevs[i]));
			
			mg.ItemsSource = MidiMenu;
		}
	}
}
