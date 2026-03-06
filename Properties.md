## WebMenu property handling
This paradigm is largely inherited from the [JSONio](https://github.com/blekenbleu/JSONio) plugin..  

There are 3 types of properties, determined by [WebMenu.ini](https://github.com/blekenbleu/SimHub-Remote-menu/blob/main/NCalcScripts/WebMenu.ini) content
- per-car
- per-game
- global  
Global properties are stored in SimHub Settings for plugins.  
`End()` writes other properties to e.g. `WebManu.json` during game changes;  

Users may change [WebManu.ini](https://github.com/blekenbleu/SimHub-Remote-menu/blob/main/NCalcScripts/WebMenu.ini) between games or plugin relaunches...

Each property will have 3 values, all numeric
- current - Per-car and Per-game 
- previous - one set saved in SimHub Settings, along with globals
- default - saved across games:  globals in Settings, others in `WebManu.json`

Given many possible games and cars, lots of settings may accumulate.  
Instead of storing dictionary of all properties internally to SimHub,  
non-global properties are saved in a .json file located per .ini `WebMenu.file`.  
This facilitates sharing settings among users e.g. sharing a ShakeIt configuration.

New (to WebMenu) cars and games inherit most recent property values...  
Property value changes during games update in-memory lists.  

A new car for new or unchanged game will use most recent values as Current.
- a new car for changed game will set that game's defaults as Current.
- Otherwise, *most-recent* values become **Previous**.

However, .ini and .json files may change at any time...  
- an NCalcScripts .ini file may be changed to indicate a different `WebMenu.file`.  
- Property names may change or move between types.  
- a different .json file (with the same or different name) may be swapped  
- SimHub's internally saved instance of global and most recent car property values  
	is unlikely to change between sessions.

WebMenu.Init() sorts incompatibilities among .ini, .json and internally saved properties  
to populate simValues and Steps.
- In all cases, .ini is considered definitive for *property type* classifications.
- .ini values become defaults for new properties
- internally saved instance *values* are considered definitive for global and current car
	- for example, if .ini moves e.g. `propertyA` from per-car to `global`,  
		then that car's game default value becomes the new global default.
		- Use the Webmenu UI to e.g. reset moved property default value to that in `Webmenu.ini`
	- global properties changed to per-game retain default Settings values
