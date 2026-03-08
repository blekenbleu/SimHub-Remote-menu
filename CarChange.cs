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

			gndx = data.gList.FindIndex(g => g.cList[0].Name == gnew);
			if (0 > gndx)
			{
				gndx = data.gList.Count;
				data.gList.Add(new GameList
				{
					cList = new List<CarL>
					{ new CarL
						{ Name = gnew,
						  vList = GameDefaults()
						}
					}
				});
			}
			game = data.gList[gndx];

			Msg = "Current Car: " + cname;
			if (0 < Gname.Length && SaveSlim())		// do not save first instance
				Msg += $";  {CurrentCar} saved";
			ml = Msg.Length;

			for (i = 0; i < simValues.Count; i++)	// copy Current to previous
				SetPrevious(i, simValues[i].Current);

			cndx = game.cList.FindIndex(c => c.Name == cname);

			if (0 > cndx)							// new car?
			{
				NewCar = "true";
				for (i = CarPropCount; i < GamePropCount; i++)
					SetDefault(i);					// perhaps altered since .ini
			}
			else
			{										// existing car
				NewCar = "false";
				if (cname != Settings.carid)		// previous car?
					for (i = 0; i < CarPropCount; i++)
						SetCurrent(i, game.cList[cndx].vList[i]);
				if (null == CurrentCar)				// first in this game instance?
				{									// restore game defaults
					for (i = 0; i < CarPropCount; i++)
						SetDefault(i);
					for(; i < GamePropCount; i++)
						SetCurrent(i, SetDefault(i, game.cList[0].vList[i]));
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
