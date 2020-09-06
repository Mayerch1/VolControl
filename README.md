# VolControl
This software manipulates the settings of Voicemeeter, like volume sliders for different audio lanes or the mute-status of your microphone.
The input can be any HID-device like a Joystick, a Wheel or your Arduino acting as HID-controller.

In addition to handling existing joystick-products, this repo holds the code for a custom-build button box.

---

## ButtonBox
The custom is build using an Arduino. The assembly of the hardware is described on hackster.io.
The code for my own button-box is found in [ButtonBox](/ButtonBox). The layout and circuit is shown in [circuit.fzz](circuit.fzz) (Frizting).

---

## VolControl
The VolControl software is running on your Windows PC as a Windows application. It offers a tray icon to indicate its own status and will show notification whenever a device is added/removed (hot-plug).

The Application is configured with the file `Settings.json`, which should be located in the same directory as the executable.




## Installation
[Download](/releases) the latest version of [VolControl](/releases) and unpack the VolControl.zip into your installation location.

Next open your startup folder by hitting `windows+r` and typing `shell::startup` followed by your return key.
Now add a shortcut to VolControl.exe into this startup folder.

(`ctrl+right-click` the executable and click `Create shortcut`, next move the shortcut into the startup folder).

---

## Settings.json Format
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
		"ptt_button": 1,
		"mute_toggle": 2,
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
       "placeholder": "replace with valid entry or remove" 
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
       