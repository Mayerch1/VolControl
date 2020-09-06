// /*https://github.com/MHeironimus/ArduinoJoystickLibrary*/
// /*https://github.com/dmadison/HID_Buttons*/


#ifndef TEENSYDUINO
#include <Joystick.h>  // Use MHeironimus's Joystick library

Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_GAMEPAD,
	1, 0,                  // Button Count, Hat Switch Count
	true, true, true,    // No X, Y, or Z axes
	true, true, true,    // No Rx, Ry, or Rz
	false, false,           // No rudder or throttle
	false, false, false);   // No accelerator, brake, or steering

#endif

#include <HID_Buttons.h>  // Must import AFTER Joystick.h


#define POTI_1_PIN A0
#define POTI_2_PIN A1

#define SWITCH_MUTE 14
#define BUTTON_MODE 16

// rgb led uses pwm pins
#define RGB_RED 3
#define RGB_BLUE 5
#define RGB_GREEN 6

#define POTI_1_LED 7
#define POTI_2_LED 8
#define SWITCH_MUTE_LED 9


#define POT_0DB_DEADZONE_PCT 6
#define POT_0DB (uint16_t)(1023*0.75)

// 10/1024 is a deadzone of 0.9%, low enough
#define POT_JITTER_DEADZONE_ABS 10



JoystickButton muteButton(0);
JoystickButton modeButton(1);


void set_rgb_led(int r, int g, int b);



void setup() {
	pinMode(POTI_1_PIN, INPUT);
    pinMode(POTI_2_PIN, INPUT);

    pinMode(SWITCH_MUTE, INPUT_PULLUP);
    pinMode(BUTTON_MODE, INPUT_PULLUP);

    
    pinMode(RGB_RED, OUTPUT);
    pinMode(RGB_BLUE, OUTPUT);
    pinMode(RGB_GREEN, OUTPUT);

    // init as led off
    set_rgb_led(0, 0, 0);

    pinMode(POTI_1_LED, OUTPUT);
    pinMode(POTI_2_LED, OUTPUT);
    pinMode(SWITCH_MUTE_LED, OUTPUT);

	Joystick.begin();
}

void loop() {

    // limit update rate of oled
    static uint32_t last_update_ms = millis();
    static uint8_t is_update = 0;

    static uint8_t mode = 0;
    // keep last state to trigger oled update
    static uint16_t pot1_last = 0, pot2_last = 0;
    static uint8_t mic_active_last = 0;
    static uint8_t mode_pressed_last = 0;


    uint16_t pot1, pot2;
    uint8_t mic_active;
    uint8_t mode_pressed;
    uint32_t current_time_ms;



    /**************************************/
    /*          read signals              */
    /**************************************/

    // range goes [0,1024], but wiring is inverted
    pot1 = 1024 - analogRead(POTI_1_PIN);
    pot2 = 1024 - analogRead(POTI_2_PIN);

    // pull up resistor -> invert
    mic_active = !digitalRead(SWITCH_MUTE);
    mode_pressed = !digitalRead(BUTTON_MODE);



    /**************************************/
    /*       process signal input         */
    /**************************************/

    // 0 position of poti is at 3/4
    // => add artificial deadzone at 768 +- 10%

    uint16_t lower_deadzone = POT_0DB - (POT_0DB * POT_0DB_DEADZONE_PCT / 100);
    uint16_t upper_deadzone = POT_0DB + (POT_0DB * POT_0DB_DEADZONE_PCT / 100);

    if(pot1 >= lower_deadzone && pot1 <= upper_deadzone){
        pot1 = POT_0DB;
    }
    if(pot2 >= lower_deadzone && pot2 <= upper_deadzone){
        pot2 = POT_0DB;
    }



    /**************************************/
    /*   update last state/update flag    */
    /**************************************/
    
    // whenever a value changed
    // refresh the last_update timestamp


    if(mic_active == mic_active_last &&\
        (pot1 <= (pot1_last + POT_JITTER_DEADZONE_ABS) && pot1 >= (pot1_last - POT_JITTER_DEADZONE_ABS)) &&\
        (pot2 <= (pot2_last + POT_JITTER_DEADZONE_ABS) && pot2 >= (pot2_last - POT_JITTER_DEADZONE_ABS))){
        
        // eliminate the jitter
        // IMPORTANT: if the deadzone is 10, a change by 9 is ignored
        // however, two consecutive changes of 6 each, are counted as 12 at the second iteration
        // this prevents 'sticking' of the poti when moving it slowly
        pot1 = pot1_last;
        pot2 = pot2_last;
    }
    else{
        last_update_ms = millis();

        pot1_last = pot1;
        pot2_last = pot2;
        mic_active_last = mic_active;

        is_update = 1;
    }


    /**************************************/
    /*       publish to computer          */
    /**************************************/


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
    muteButton.set(mic_active);
    modeButton.set(mode_pressed);


    

    /**************************************/
    /*             update UI              */
    /**************************************/

    current_time_ms = millis();

    if((current_time_ms - last_update_ms) > 10000){
        set_rgb_led(0,0,0);
        digitalWrite(SWITCH_MUTE_LED, LOW);
        digitalWrite(POTI_1_LED, LOW);
        digitalWrite(POTI_2_LED, LOW);
    }
    else{
        switch(mode){
            case 0:
                set_rgb_led(1, 0, 0);
                break;
            case 1:
                set_rgb_led(0, 1, 0);
                break;
            case 2:
                set_rgb_led(0, 0, 1);
                break;
        }

        // update leds based on settings
        if(mic_active){
            digitalWrite(SWITCH_MUTE_LED, HIGH);
        }
        else{
            digitalWrite(SWITCH_MUTE_LED, LOW);
        }


        if(pot1 == POT_0DB){
            digitalWrite(POTI_1_LED, HIGH);
        }
        else{
            digitalWrite(POTI_1_LED, LOW);
        }

        if(pot2 == POT_0DB){
            digitalWrite(POTI_2_LED, HIGH);
        }
        else{
            digitalWrite(POTI_2_LED, LOW);
        }
    }


    /**************************************/
    /*      handle mode switch            */
    /**************************************/

    // toggle mode switch, force update oled in any case
    if(mode_pressed_last != mode_pressed && mode_pressed){
        mode = mode == 2 ? 0 : mode + 1;

        delay(150); // de-bounce

        // this counts as update aswell
        last_update_ms = millis();        
    }
    

    // update last-state vars
    mode_pressed_last = mode_pressed;
}



void set_rgb_led(int r, int g, int b){

    // common anode turns led off when HIGH
    // currently only binary addressing is used

    if(r){
        digitalWrite(RGB_RED, LOW);
    }
    else{
        digitalWrite(RGB_RED, HIGH);
    }

    if(g){
        digitalWrite(RGB_GREEN, LOW);
    }
    else{
        digitalWrite(RGB_GREEN, HIGH);
    }

    if(b){
        digitalWrite(RGB_BLUE, LOW);
    }
    else{
        digitalWrite(RGB_BLUE, HIGH);
    }
}