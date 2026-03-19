[*back*](README.md#plan)  
## WebMenu DirectInput (joysticks)

To avoid conflicts with other SimHub access to DirectInput devices,  
instead monitor corresponding property changes in `DataUpdate()`,  
which licensed SimHub calls 60 times per second while running games or replays.

- SimHub reads DirectInput using its Controllers plugin
	- axes properties are exposed as `JoystickPlugin.*`
	- button properties exposed as `InputStatus.JoystickPlugin.*`
- SimHub concatenates DirectInput device names and property names using underscores, e.g.
	- `InputStatus.JoystickPlugin._VKBsim_Gladiator_EVO_R___B12`
	- `JoystickPlugin._VKBsim_Gladiator_EVO_R___1_AccelerationSlider0`

In that last example, device name is `_VKBsim_Gladiator_EVO_R___1`  
and axis property name is `B12`:
```
string property = "JoystickPlugin._VKBsim_Gladiator_EVO_R___1_AccelerationSlider0";
int axisstart = 1 + property.LastIndexOf('_');
int devstart = 1 + property.LastIndexOf('.', axisstart);
```

`DataUpdate()` will, `if (learnDI)`,
- first, extract:
```
List<string> foo = pluginManager.GetAllPropertiesNames().FindAll(s => s.StartsWith("JoystickPlugin."));
```
	- store current value for each item
	- then, on subsequent invocations loop thru the list, looking for mismatches
	- for each mismatch, invoke `SemaphoreQueue()`
		- which subsequently sorts those payloads
- if/when DirectInput properties have been learned,
	- `DataUpdate()` scans learned button and axis lists, invoking `SemaphoreQueue()` for each value change
	
