using System.Collections.Generic;
using System.Windows;

namespace blekenbleu.SimHub_Remote_menu
{
	/// <summary>
	/// MIDI interaction code for Control.xaml
	/// </summary>
	public partial class Control
	{
		// index xaml event strings by devMessages
		internal static SortedList<int, string> click = new SortedList<int, string>() {};
		static int recent, forget;					// MidiDev messages with data2 masked out
		internal static bool busy;
		static bool button, changed, _learn = false;			// state variables
		static string again = " ";

		static bool Earn
		{
			get { return _learn; }
			set {
					_learn = value;
					Model.Forget = _learn ? Visibility.Visible : Visibility.Hidden;
				}
		}

		internal static void Add(int recent, string bName)
		{
			click.Add(recent, bName);
			changed = true;
		}

		internal static bool Stop()
		{
			if (changed)
			{
				WebMenu.Settings.midiDevs = new List<MidiDev>() {};
				for (int j = 0; j < click.Count; j++)
				{
					int key = click.Keys[j];
					int i = 0x07000000 & key;
					i >>= 24;
					WebMenu.Settings.midiDevs.Add(new MidiDev()
					{
						butName = click.Values[j],
						devName = NAudio.Midi.MidiIn.DeviceInfo(i).ProductName,
						devMessage = key
					});
				}
			}
			return changed;
		}
		void ListClick(string bName)	// checks for slider or buttons
		{
			if (0 == recent)
				Model.MidiStatus = "\nMIDI input missing";
			else if (click.ContainsKey(recent))
			{
				Model.MidiStatus = $"\nMIDI {recent:X8} already click {click[recent]};  first Forget it";
				forget = recent;
			}
			else if (click.ContainsValue(bName) && again != bName)
			{
				Model.MidiStatus = $"\n'{bName}' already in click list; click again to also add {recent:X8}\n -or- Forget to remove current {bName}";
				again = bName;
			}
			else {
				Add(recent, bName);
				Model.MidiStatus = $"\n'{bName}' {recent:X8} added to click list";
				again = "";
				forget = recent = 0;
			}
		}

		// https://github.com/blekenbleu/SimHub-Remote-menu/blob/MIDI/Channel.md#midi-device-name-handling
		// https://learn.microsoft.com/en-us/dotnet/api/system.collections.sortedlist?view=netframework-4.8
		// https://www.hobbytronics.co.uk/wp-content/uploads/2023/07/9_MIDI_code.pdf
		void Learn(string bName)	// associate MIDI messages with xaml button events
		{
			if ( "bf" == bName)					// Forget click?
			{
				if (0 == click.Count || (0 == forget && "" == again))
					Model.MidiStatus = "\nNo listed clicks to Forget";
				else if (0 == forget && "" != again && click.ContainsValue(again))
				{
					forget = click.Keys[click.IndexOfValue(again)];
					Model.MidiStatus = $"\nclick Forget again to remove {again} for {forget:X8}";
				}
				else if (!click.ContainsKey(forget))
				{
					if (0 < forget)
						Model.MidiStatus = $"\n MIDI {forget:X8} not in click list";
					forget = 0;
				}
				else {
					Model.MidiStatus = $"\nremoving MIDI {forget:X8} for {click[forget]}...";
					click.Remove(forget);
					changed = true;
					forget = 0;
					again = "";
				}
			}
			else if (!button)
				Model.MidiStatus = "\nMIDI control >>only<< for slider;  ignored";
			else ListClick(bName);
		}

		void NotEarn()				// handle "bm" == butName
		{
			Earn = !Earn;
			if (!Earn)
			{
				forget = 0;
				again = "";
			}
			Model.MidiStatus = (Earn && MIDI.Start(Model, this)) ?  "\n\twaiting for MIDI input" : " ";
		}

		// Handle Control Change (0xB0), Patch Change (0xC0) and Bank Select (0xB0) channel messages
		// https://github.com/naudio/NAudio/blob/master/NAudio.Midi/Midi/MidiEvent.cs#L24
		// https://www.hobbytronics.co.uk/wp-content/uploads/2023/07/9_MIDI_code.pdf
		internal static void ProcessMIDI(int MidiMessage)  // called by async Task Channel.ReadAsync()
		{
			busy = true;
/*			NAudio bytes are reversed from e.g. MidiView and WetDry:  Status byte is least significant..
 ;			var channel = 0x0F & MidiMessage;		// most likely always 0 for real control surfaces
 ;			var d1 = (MidiMessage >> 8) & 0xff;		// e.g. CC number
 ;			var d2 = (MidiMessage >> 16) & 0xff;		// data value
 ;			var dev = (MidiMessage >> 24) & 0x0f;	// NAudio-detected MIDI device list index
 */
			int channel_type = 0xF0 & MidiMessage;
			if (0xB0 > channel_type || 0xD0 < channel_type)
			{
				Model.MidiStatus = $"\nProcess({MidiMessage:X8}) ignored";
				busy = false;
				return;
			}

			int latest = 0x0700FFFF & MidiMessage;
			int mVal = 127 & (MidiMessage >> 16);
			if (Earn) {
				if (0xB0 != (0xFF00F0 & MidiMessage))	// ignore CC button releases
				{
					if (recent != latest)
					{
						button = 0 == mVal % 127;
						recent = latest;
					}
					else if (0 != mVal % 127)
						button = false;
					if (click.ContainsKey(recent))
					{
						Model.MidiStatus = $"\nProcess({MidiMessage:X8}) MIDI already in click list for {click[recent]};  Forget?";
						forget = recent;
					}
					else Model.MidiStatus = $"\nclick in UI to learn for ({MidiMessage:X8})";
				}
			}
			else if (click.ContainsKey(latest))
			{
				if ("SL" == click[latest])					// handle values, including 0, for slider
				{
					OK.FromSlider(mVal/1.27);				// MIDI value as if from slider, except 0-127
					OK.ToSlider();							// update WPF slider position
				}
				else if (0xB0 != (0x7F00F0 & MidiMessage))	// ignore CC button 0 values
					ClickHandle(click[latest]);
			}
			else Model.MidiStatus = $"\nMIDI({latest:X8}) not learned";
			busy = false;
		}
	}
}
