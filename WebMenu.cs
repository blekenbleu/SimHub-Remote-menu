using GameReaderCommon;
using SimHub.Plugins;
using System.Windows.Media;

namespace blekenbleu.SimHub_Remote_menu
{
	[PluginDescription("JSONio properties displayed in HTTP table")]
	[PluginAuthor("blekenbleu")]
	[PluginName("WebMenu")]
	public partial class WebMenu : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		public DataPluginSettings Settings;
        public string NewCar = "false";

		internal static string Msg = "";

		// breaks Ini if not preceding configuration source
		static readonly string My = "WebMenu.";
		static readonly string Myni = "DataCorePlugin.ExternalScript." + My;

		bool set = false, once = true, write = false;
		string CurrentCar;
		string Gname = "";
		string path;									// JSON file location
		int gndx = -1, cndx = -1;						// current car data.gList indices
		int slider = -1;								// assigned simValues index
		readonly double[] SliderFactor = new double[] { 0, 0 };

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
			string cid = data?.NewData?.CarId, gid = pluginManager?.GameName;
			
			if (0 < cid?.Length && cid != CurrentCar && 0 < gid?.Length && 0 < simValues.Count) 
				CarChange(cid, gid);                // disable popup
		}

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
				System.Windows.Forms.MessageBox.Show(Msg, "WebMenu.Init()");
			} else Msg = $"{Settings.game} {Settings.carid}";
			Control.Model.StatusText = Msg + "\n" + ViewModel.staticText;
			Msg = "";
			return View;
		}
	}		// class WebMenu
}

