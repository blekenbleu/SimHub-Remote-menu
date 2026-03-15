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
		internal static string version = "1.93";
		//internal ContentControl MyControl = new ContentControl();

		public Control() {							// called before simValues are initialized
			Model = new ViewModel(this);
			DataContext = Model;					// StaticControl events change Control.xaml binds
			changed = Earn = false;					// Control.midi.cs
			InitializeComponent();
		}

		public Control(WebMenu plugin) : this()
		{
			OK = plugin;							// Control.xaml Button events call WebMenu methods
			dg.ItemsSource = WebMenu.simValues;		// bind XAML DataGrid
			if (0 < OK.Settings.midiDevs.Count)
			{
				MIDI.Resume(Model, this, OK);
				Init_mg();							// Midi Learn display
			}
			else Model.MidiStatus += "\nno MIDI configured";
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
	}
}
