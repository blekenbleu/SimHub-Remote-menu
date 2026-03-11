using System.Windows;
using System.Windows.Controls;

/* XAML DataContext:  Binding source
 ;	https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-specify-the-binding-source?view=netframeworkdesktop-4.8
 ;	https://www.codeproject.com/articles/126249/mvvm-pattern-in-wpf-a-simple-tutorial-for-absolute
 ;	alternatively, DataContext in XAML	https://dev.to/mileswatson/a-beginners-guide-to-mvvm-using-c-wpf-241b
 */

namespace blekenbleu.SimHub_Remote_menu
{
	/// <summary>
	/// Interaction code for Control.xaml
	/// </summary>
	public partial class Control : UserControl
	{
		static WebMenu OK;
		internal static ViewModel Model;			// reference XAML controls
		internal byte Selection;					// changes only in WebMenu.Select() on UI thread
		internal static string version = "1.83";

		public Control() {							// called before simValues are initialized
			Model = new ViewModel(this);
			InitializeComponent();
			DataContext = Model;					// StaticControl events change Control.xaml binds
			changed = Earn = false;					// Control.midi.cs
		}

		public Control(WebMenu plugin) : this()
		{
			OK = plugin;							// Control.xaml button events call WebMenu methods
			dg.ItemsSource = WebMenu.simValues;	// bind XAML DataGrid
			if (0 < WebMenu.Settings.midiDevs.Count)
				MIDI.Resume(Model, this);
		}

		private void Hyperlink_RequestNavigate(object sender,
									System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		// highlights selected property cell
		internal void Selected()					// crashes if called from other threads
		{
			if ((dg.Items.Count > Selection) && (dg.Columns.Count > 2))
			{
				//Select the item.
				dg.CurrentCell = new DataGridCellInfo(dg.Items[Selection], dg.Columns[1]);
				dg.SelectedCells.Clear();
				dg.SelectedCells.Add(dg.CurrentCell);
				HttpServer.SSEscroll(Selection);
			}
		}

		// xaml DataGrid:  Loaded="DgSelect"
		private void DgSelect(object sender, RoutedEventArgs e)
		{
			Selected();
		}

		internal static void ClickHandle(string butName)	// used by ButEvent(), Process(MidiMessage)
		{
			Model.MidiStatus = " ";
			switch(butName)
			{
				case "b0":
					OK.Select(false);
					break;
				case "b1":
					OK.Select(true);
					break;
				case "b2":
					OK.Ment(1);
					break;
				case "b3":
					OK.Ment(-1);
					break;
				case "b4":
					OK.Swap();
					break;
				case "b5":
					OK.SetDefault();
					break;
				case "SB":
					OK.SliderButtton();
					break;
				default:
                    WebMenu.Msg = "ClickHandle(): unconfigured click '{butName)'";
					OK.OOpsMB();	// tested 1 Mar 2026
					break;
			}
		}
	}
}
