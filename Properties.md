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

`WebMenu.Init()` sorts incompatibilities among .ini, .json and internally saved properties  
to populate simValues and Steps.
- In all cases, .ini is considered definitive for *property type* classifications.
- .ini values become defaults for new properties
- internally saved instance *values* are considered definitive for global and current car
	- for example, if .ini moves e.g. `propertyA` from per-car to `global`,  
		then that car's game default value becomes the new global default.
		- Use the Webmenu UI to e.g. reset moved property default value to that in `Webmenu.ini`
	- global properties changed to per-game retain default Settings values

## file writing parsimony
With solid state drives having largely replaced spinning magnetic storage,  
unnecessary file writing accelerates their wear.  
Consequently, unless existing Settings and `WebMenu.file` are genuinely defective,  
those files need not be rewitten unless/until users change some property values  
from what can be recovered by `Init()` code.  Specifically, any
- Current property changes provoke writing both Settings (for Previous)  
   and `WebMenu.file` (for car- and game- specific) values.
- Global default property changes provoke writing only Settings  
- two `bool` flags:
	- `write` for `WebMenu.file` if default or per-car per-game values change  
	 or `Load()` if `WebMenu.file` failed.
	- `set` for Settings if global default or any current values change  
		- for `End()` to compare, `Init()` updates `Settings.gDefaults`
- Car changes can occur within a game session;&nbsp; consequently,  
  `CarChange()` must set `write` for per-car or per-game default  
  or current -per-car value changes
  from those set in `Plugin data` by `Init()`

### internals
- `WebMenu.Init()` uses `Populate()` to list `simValues`   
   with `Default`, `Current` and `Previous` values for configured property names.  
	- Names and initial default values are obtained from `NCalcScripts/WebMenu.ini`.
		- those initial default values are replaced by matches from `Settings.gDefaults`. 
- `WebMenu.Init()` also creates an updated `data PluginList`
	- if modified from loaded `Webmenu.json`, e.g. based on `NCalcScripts/WebMenu.ini`.
- `WebMenu.End()` will save `Settings` to SimHub if changed and SimHub has loaded some Car[s].
	- `Settings` to be written would be created from `simValues`.
- `WebMenu.End()` may also update `data PluginList` from `simValues`  
   and write to `Webmenu.json` if changed and SimHub has loaded some Car[s].
