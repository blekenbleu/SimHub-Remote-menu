using System.ComponentModel;
using System.Text;
using System.Windows;

/*
 ; Model-View-ViewModel (MVVM)
 ; https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/?view=netdesktop-8.0
 ; https://www.c-sharpcorner.com/article/datacontext-autowire-in-wpf/
 ; https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern
 ; https://scottlilly.com/c-design-patterns-mvvm-model-view-viewmodel/
 */
namespace blekenbleu.SimHub_Remote_menu
{
	/// <summary>
	/// define a class with Model-view-viewmodel pattern for dynamic UI
	/// </summary>
	public class ViewModel : INotifyPropertyChanged
	{
		readonly Control Ctrl;				// Ctrl.Dispatcher.Invoke(Ctrl.Selected())
		public ViewModel(Control C)
		{
			Ctrl = C;
		}

		// One event handler for all property changes
		public event PropertyChangedEventHandler PropertyChanged;

		// events to raise
		readonly PropertyChangedEventArgs Bevent = new PropertyChangedEventArgs("ButtonVisibility");
		readonly PropertyChangedEventArgs Cevent = new PropertyChangedEventArgs("ChangedVisibility");
		readonly PropertyChangedEventArgs Fevent = new PropertyChangedEventArgs("Forget");
		readonly PropertyChangedEventArgs Nevent = new PropertyChangedEventArgs("SliderProperty");
		readonly PropertyChangedEventArgs Sevent = new PropertyChangedEventArgs("SelectedProperty");
		readonly PropertyChangedEventArgs SVevent = new PropertyChangedEventArgs("SliderVisibility");
		readonly PropertyChangedEventArgs SLevent = new PropertyChangedEventArgs("SliderValue");
		readonly PropertyChangedEventArgs Tevent = new PropertyChangedEventArgs("Text");


        private Visibility _bvis = Visibility.Hidden;	// until carID and game are defined
		public Visibility ButtonVisibility				// must be public for XAML Binding
		{
			get { return _bvis; }
			set
			{
				if (_bvis != value)
				{
					_bvis = value;
					PropertyChanged?.Invoke(this, Bevent);
				}
			}
		}

		private Visibility _cvis = Visibility.Hidden;	// until carID and game are defined
		public Visibility ChangedVisibility				// must be public for XAML Binding
		{
			get { return _cvis; }
			set
			{
				if (_cvis != value)
				{
					_cvis = value;
					PropertyChanged?.Invoke(this, Cevent);
				}
			}
		}

		private Visibility _fvis = Visibility.Hidden;	// except when learning MIDI
		public Visibility Forget				// must be public for XAML Binding
		{
			get { return _fvis; }
			set
			{
				if (_fvis != value)
				{
					_fvis = value;
					PropertyChanged?.Invoke(this, Fevent);
				}
			}
		}

		private double _sval = 40;
		public double SliderValue		// must be public for XAML Binding
		{
			get { return _sval; }
			set
			{
				if (_sval != value)
				{
					_sval = value;
					PropertyChanged?.Invoke(this, SLevent);
					HttpServer.SSEslide(SliderValue, SliderProperty);
				}
			}
		}

		private Visibility _svis = Visibility.Hidden;
		public Visibility SliderVisibility		// must be public for XAML Binding
		{
			get { return _svis; }
			set
			{
				if (_svis != value)
				{
					_svis = value;
					PropertyChanged?.Invoke(this, SVevent);
				}
			}
		}

		private string _selected_Property = "unKnown";
		public string SelectedProperty			// must be public for XAML Binding
		{
			get { return _selected_Property; }

			set
			{
				if (value != _selected_Property)
				{
					_selected_Property = value;
					PropertyChanged?.Invoke(this, Sevent);
					// update xaml DataGrid from another thread
					Ctrl.Dispatcher.Invoke(() => Ctrl.Selected());
				}
			}
		}

		private string _midi_status = " ";
		public string MidiStatus			// must be public for XAML Binding
		{
			get { return _midi_status; }

			set
			{
				if (value != _midi_status)
				{
					_midi_status = value;
					Text = StatusText + value;
				}
			}
		}

		private string _slider_Property = "";
		public string SliderProperty			// must be public for XAML Binding
		{
			get { return _slider_Property; }

			set
			{
				if (value != _slider_Property)
				{
					_slider_Property = value;
					PropertyChanged?.Invoke(this, Nevent);
					SliderVisibility = Visibility.Visible;
					HttpServer.SSEslide(SliderValue, SliderProperty);
				}
			}
		}

		public static string SSEtext(bool data)
		{
			StringBuilder sb = new StringBuilder(_Text);
			sb.Replace("\n", "<br>");
			if (data)
				sb.Insert(0, "data: ");
			sb.Append("\n");
			return sb.ToString();
		}

		private static string _Text = staticText;
		public string Text			// must be public for XAML Binding
		{
			get { return _Text; }

			set
			{
				if (value != _Text)
				{
					_Text = value;
					string s = SSEtext(true);

                    HttpServer.SSErespond(s);
					PropertyChanged?.Invoke(this, Tevent);
				}
			}
		}

		static internal readonly string staticText = "To enable, launch game or Replay";
		private string _statusText = staticText;
		public string StatusText			// must be public for XAML Binding
		{
			get { return _statusText; }

			set
			{
				if (value != _statusText)
				{
					_statusText = value;
					Text = value + _midi_status;
				}
			}
		}
    }		// public class ViewModel
}
