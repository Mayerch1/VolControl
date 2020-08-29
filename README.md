VolControl

# ButtonBox

# VolControl
The VolControl is running on your windows PC as a windows application. It offers a tray icon to indicate its own status and will show notification whenever a device is added/removed (hot-plug).

The Application is configured with the file `Settings.json`, which should be located in the directory as the executable.

### Settings Format
All used devices are defined within a json list
```json
[
    {

    },
    {

    }
]
```

Each Element needs to hold a `guid` element, which identifies this device.
The `guid` can be obtained by running the [HIDList](/HIDList) program. This program simply outputs the id of e
very connected HID device.

The available options are
* mute_switch
* mute_toggle
* ptt_button

The values of those is the index of the required Button as 0-offset. (The Windows game controller settings show the buttons as 1-offset). 

```json
[
	{
		"guid": "80372341-0000-0000-0000-504944564944",
		"mute_switch": 0,
		"sliders": [
			{"index": 7, "button": "X"},
			{"index": 6,"button": "Y"},
			{"index": 0, "button": "Z"},
			{"index": 1, "button": "RotationX"},
			{"index": 2, "button": "RotationY"},
			{"index": 3, "button": "RotationZ"}
		]
	},
    {
        
    }
]
```

### Sliders
Sliders are configured within the `slider` list.
Each element of the slider holds an `index` which corresponds to the matching slider in Voicemeter (0-offset) and the `button` Key configures the used axis for this Slider.

Allowed Values for `button` is any axis of your HID-Device.

* AccelerationX
* AccelerationY
* AccelerationZ
* AngularAccelerationX
* AngularAccelerationY
* AngularAccelerationZ
* AngularVelocityX
* AngularVelocityY
* AngularVelocityZ
* ForceX
* ForceY
* ForceZ
* RotationY
* RotationX
* RotationZ
* TorqueX
* TorqueY
* TorqueZ
* VelocityX
* VelocityY
* VelocityZ
* X
* Y
* Z
       