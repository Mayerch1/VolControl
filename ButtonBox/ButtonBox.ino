// /*https://github.com/MHeironimus/ArduinoJoystickLibrary*/
// /*https://github.com/dmadison/HID_Buttons*/

#ifndef TEENSYDUINO
#include <Joystick.h>  // Use MHeironimus's Joystick library

Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_GAMEPAD,
	10, 0,                  // Button Count, Hat Switch Count
	true, true, true,    // No X, Y, or Z axes
	true, true, true,    // No Rx, Ry, or Rz
	false, false,           // No rudder or throttle
	false, false, false);   // No accelerator, brake, or steering

#endif

#include <HID_Buttons.h>  // Must import AFTER Joystick.h


#define POTI_1_PIN A0
#define POTI_2_PIN A1

#define SWITCH_MUTE 16
#define BUTTON_MODE 10

const uint8_t ButtonNumber = 1;


JoystickButton muteButton(0);
JoystickButton modeButton(1);



void setup() {
	pinMode(POTI_1_PIN, INPUT);
    pinMode(POTI_2_PIN, INPUT);

    pinMode(SWITCH_MUTE, INPUT_PULLUP);
    pinMode(BUTTON_MODE, INPUT_PULLUP);

	Joystick.begin();
}

void loop() {
    static uint8_t mode_pressed_last = 0;
    static uint8_t mode = 0;

    uint16_t pot1, pot2;
    uint8_t mic_mute;
    uint8_t mode_pressed;


    pot1 = 1024 - analogRead(POTI_1_PIN);
    pot2 = 1024 - analogRead(POTI_2_PIN);

    mic_mute = !digitalRead(SWITCH_MUTE);
    mode_pressed = !digitalRead(BUTTON_MODE);


	// mode based button mappings
    switch(mode){
        case 0:
            Joystick.setXAxis(pot1);
            Joystick.setYAxis(pot2);
            break;
        case 1:
            Joystick.setZAxis(pot1);
            Joystick.setRxAxis(pot2);
            break;
        case 2:
            Joystick.setRyAxis(pot1);
            Joystick.setRzAxis(pot2);
            break;
    }


    // universal button mappings
    muteButton.set(mic_mute);
    modeButton.set(mode_pressed);


    // toggle mode switch
    if(mode_pressed_last != mode_pressed && mode_pressed){
        mode = mode == 2 ? 0 : mode + 1;
    }


    // update last-state vars
    mode_pressed_last = mode_pressed;
}


