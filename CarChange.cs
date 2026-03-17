using System.Collections.Generic;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
/*--------------------------------------------------------------
 ;	  invoked for CarId changes
 ;--------------------------------------------------------------- */
		void CarChange(string cname, string gnew)
		{
			int i, ml = 0;
			GameList game = null;
			bool changed = false;

			gndx = data.gList.FindIndex(g => g.cList[0].Name == gnew);
			if (0 > gndx)
			{
				List<string> dlist = new List<string> {};
				for (int d = 0; d < GamePropCount; d++)
					dlist.Add(iniDefaults[d]);

				gndx = data.gList.Count;
				data.gList.Add(new GameList
				{
					cList = new List<CarL>
					{ new CarL
						{ Name = gnew,
						  vList = dlist
						}
					}
				});
			}
			game = data.gList[gndx];

			Msg = "Current Car: " + cname;
			if (0 < Gname.Length && UpdateGame())		// do not save first instance
			{
				Msg += $";  {CurrentCar} saved";
				changed = Changed();
				write = write || changed;
			}
			ml = Msg.Length;

			for (i = 0; i < simValues.Count; i++)	// copy Current to previous
				SetPrevious(i, simValues[i].Current);

			cndx = game.cList.FindIndex(c => c.Name == cname);

			if (0 > cndx)							// new car?
			{
				NewCar = "true";
				if (changed)
					for (i = CarPropCount; i < GamePropCount; i++)
						SetDefault(i);				// perhaps altered since .ini
			}
			else
			{										// existing car
				NewCar = "false";
				if (cname != Settings.carid)		// previous car?
					for (i = 0; i < CarPropCount; i++)
						SetCurrent(i, Settings.Value[i] = game.cList[cndx].vList[i]);
				if (0 == CurrentCar.Length)			// first in this game instance?
				{									// restore game defaults
					for (i = 0; i < CarPropCount; i++)
						Settings.defaults[i] = SetDefault(i);
					for(; i < GamePropCount; i++)
						SetCurrent(i, Settings.Value[i] = Settings.defaults[i] = SetDefault(i, game.cList[0].vList[i]));
				}
			}
			Settings.carid = CurrentCar = cname;
			Changed();

			if (ml < Msg.Length)
			{
				if (once)
				{
					OOpsMB();
					once = false;
				}
				return;
			}
			else Msg = "";
			Gname = gnew;
			Control.Model.StatusText = Gname + " " + CurrentCar;
			ToSlider();
			Control.Model.ButtonVisibility = System.Windows.Visibility.Visible;	// ready
		}
	}
}
