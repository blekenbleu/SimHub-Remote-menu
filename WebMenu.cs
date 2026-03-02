using GameReaderCommon;
using SimHub.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace blekenbleu.SimHub_Remote_menu
{
	[PluginDescription("JSONio properties displayed in HTTP table")]
	[PluginAuthor("blekenbleu")]
	[PluginName("WebMenu")]
	public partial class WebMenu : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		public static DataPluginSettings Settings;
		public string NewCar = "false";

		internal static string Msg = "";

		static int pCount = 0;					// append per-game settings after pCount
		static int gCount = 0;					// append global settings after gCount
		static readonly string My = "WebMenu.";	// breaks Ini if not preceding
												// configuration source
		static readonly string Myni = "DataCorePlugin.ExternalScript." + My;

		bool set = false, once = true;
		string CurrentCar;
		string Gname = "";
		int slider = -1;						// simValues index for configured JSONIO.properties
		int gndx = -1, cndx = -1;						// current car data.gList indices
		string path;									// JSON file location
		readonly double[] SliderFactor = new double[] { 0, 0 };
		List<Property> SettingsProps;					// non-null Settings entries
		List<int> Steps;								// 100 times actual values
		bool write = false;								// slim should not change

		/// <summary>
		/// DisplayGrid contents
		/// </summary>
		public static List<Values> simValues = new List<Values>();		// must be initialized before Init()

		/// <summary>
		/// Plugin-specific wrapper for SimHub.Logging.Current.Info();
		/// </summary>
		internal static bool Info(string str)
		{
			SimHub.Logging.Current.Info(WebMenu.My + str);   // bool Info()
			return true;
		}

		void OOps(string str)
		{
			Msg = str;
			OOpsMB();				// may [WatchDog] Abnormal Inactivity dump
		}

		/// <summary>
		/// Plugin manager instance
		/// </summary>
		public PluginManager PluginManager { get; set; }

		/// <summary>
		/// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
		/// </summary>
		public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

		/// <summary>
		/// Short plugin title to show in left menu. Return null to use the PluginName attribute.
		/// </summary>
		public string LeftMenuTitle => "WebMenu " + Control.version;

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (plugins SHOULD NOT USE)
        /// This method is on the critical path, must execute as fast as possible and avoid throwing any error
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frames.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			string cid, gid;
			
			if (null != (cid = data?.NewData?.CarId) && cid != CurrentCar
             && null != (gid = pluginManager?.GameName))
				CarChange(cid, gid);                // disable popup
		}

		/// <summary>
		/// Called at plugin manager stop, close/dispose anything needed here !
		/// Plugins are rebuilt at game changes
		/// </summary>
		/// <param name="pluginManager"></param>
		public void End(PluginManager pluginManager)
		{
			SaveSlim();			// set write for changes
			// Save settings
			if (0 < Gname.Length && write) {
				int i;

				set = true;	// End(): save Current values
				Settings.properties = new List<Property> {};
				Settings.game = Gname;
				Settings.carid = CurrentCar;
				for(i = 0; i < simValues.Count; i++)
					if (null != simValues[i].Name &&  null != simValues[i].Current)
						Settings.properties.Add(new Property()
						{ Name  = simValues[i].Name,
						  Value = simValues[i].Current
						});

				Settings.gDefaults = new List<Property> {};
				for(i = gCount; i < simValues.Count; i++)
					if (null != simValues[i].Name &&  null != simValues[i].Default)
						Settings.gDefaults.Add(new Property()
						{ Name  = simValues[i].Name,
					  	  Value = simValues[i].Default
						});

				// capture per-game Default changes
				data.gList[gndx].cList[0].Vlist = DefaultCopy();
			}

			set = set || MIDI.Stop();
			if (set)	// .ini mismatches Settings or game run
				this.SaveCommonSettings("GeneralSettings", Settings);

//			HttpServer.Stop();
			simValues = new List<Values>();

			if (!write)				// End()
				return;

            // this fails if data and GamesList are not all public
            string sjs = Newtonsoft.Json.JsonConvert.SerializeObject(data,
						 Newtonsoft.Json.Formatting.Indented);
			if (0 == sjs.Length || "{}" == sjs)
				OOps("End():  Json Serializer failure");
			else System.IO.File.WriteAllText(path, sjs);
		}	// End()

		internal void OOpsMB()
		{
			Info(Msg);						// prefixes WebMenu.My 
			Control.Model.StatusText = Msg;
            System.Windows.Forms.MessageBox.Show(Msg, "WebMenu");
			Msg = "";
		}

		/// <summary>
		/// Returns settings control or null if not required
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <returns>UserControl instance</returns>
		Control View;										// instance of Control.xaml.cs Control()
		public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
		{
			HttpServer.Start(View = new Control(this));		// invoked *after* Init()
			SliderButtton();								// depends on View instance

			if (0 < Msg.Length) 							// pop-up for Init() issues
			{
				Info("Init():  " + Msg);
				Msg += "\n" + ViewModel.staticText;
				Control.Model.StatusText = Msg;
				System.Windows.Forms.MessageBox.Show(Msg, "WebMenu.Init()");
				Msg = "";
			}
			return View;
		}

		// add properties and settings to simValues; initialize Steps
		// if a property move among
		void Populate(List<string>props, List<string> vals, List<string> stps)
		{
			for (int c = 0; c < props.Count; c++)
			{
				// populate DisplayGrid ItemsSource
				// WebMenu.ini contents may not match saved car properties
				// default value from .ini
				int Index = SettingsProps.FindIndex(i => i.Name == props[c]);
				string ini = (c < vals.Count) ? vals[c] : (0 <= Index) ? SettingsProps[Index].Value : "0";
				// use SettingsProps value, if it exists, else from .ini
				string setting = (0 <= Index) ? SettingsProps[Index].Value : ini;

				simValues.Add(new Values {	Name = props[c],
											Default = ini,			// replaced by JSON values
											Current = setting,
											Previous = setting });
				Steps.Add((c < stps.Count)  ? (int)(100 * float.Parse(stps[c]))
											: 10);
			}
		}
	}		// class WebMenu
}

