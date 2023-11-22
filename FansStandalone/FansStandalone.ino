#include <FanController.h>

#define RELAIS_TOGGLE 14
#define POTI_PIN A0 /* A0 */
#define POTI_SWITCH 7

#define PWM_1_PIN 3
#define PWM_2_PIN 9
#define PWM_3_PIN 10

/* dummies */
#define SENSOR_THRESHOLD 1000
#define UNUSED_GDIO_1 4

const int min_switch_delay = 1000;
FanController fan(UNUSED_GDIO_1, SENSOR_THRESHOLD, PWM_1_PIN);

void setup() { 
  pinMode(RELAIS_TOGGLE, OUTPUT);
  pinMode(POTI_PIN, INPUT);
  pinMode(POTI_SWITCH, INPUT_PULLUP);

  pinMode(PWM_2_PIN, OUTPUT);                               // Set digital pin 9 (D9) to an output
  pinMode(PWM_3_PIN, OUTPUT);                              // Set digital pin 10 (D10) to an output

  /* timer for pin 9/10 */
  TCCR1A = _BV(COM1A1) | _BV(COM1B1) | _BV(WGM11);  // Enable PWM outputs for OC1A and OC1B on digital pins 9, 10
  TCCR1B = _BV(WGM13) | _BV(WGM12) | _BV(CS11);     // Set fast PWM and prescaler of 8 on timer 1
  ICR1 = 99;                                       // Set the PWM frequency to 20kHz (16MHz / (8 * (99 + 1)))
  OCR1A = 0;                                      // Set duty-cycle to 0% on D9
  OCR1B = 0;                                      // Set duty-cycle to 0% on D10

  fan.begin();

  digitalWrite(RELAIS_TOGGLE, LOW);

  // Serial.begin(9600);
  // while(!Serial){delay(5);}
}


void set3(int duty){
  // map duty cycle [0..100%] onto range of timer [0..99]
  duty = map(duty, 0, 100, 0, 99);
  fan.setDutyCycle(duty);
}

void set9(int duty){
  // map duty cycle [0..100%] onto range of timer [0..99]
  duty = map(duty, 0, 100, 0, 99);
  OCR1A = duty; // Set duty-cycle of pin 9
}

void set10(int duty){
  // map duty cycle [0..100%] onto range of timer [0..99]
  duty = map(duty, 0, 100, 0, 99);
  OCR1B = duty; // Set duty-cycle of pin 10
}


void set_fans(int pct){
  static uint8_t last_relais_target = LOW; /* init will open relais */
  static uint32_t last_switch = 0;
  
  uint8_t relais_target;
  uint32_t now = millis();
  uint32_t delta = now-last_switch;

  if(pct < 5){
    set3(0);
    set9(0);
    set10(0);  
    relais_target = LOW;
  }
  else{
    set3(pct);
    set9(pct);
    set10(pct);
    relais_target = HIGH;
  }

  if(relais_target != last_relais_target && delta > min_switch_delay){
    last_switch = now;
    last_relais_target = relais_target;
    digitalWrite(RELAIS_TOGGLE, relais_target);
  }
  Serial.println(delta);
}


int avg_poti(){
  const int LEN = 50;
  static int avg[LEN] = {0};
  static int i=0;
  int new_value = analogRead(POTI_PIN);

  avg[i++] = new_value;
  i %= LEN;

  float value = 0;
  for(int i=0; i<LEN; i++){
    value += avg[i] / (float)LEN;
  }
  return (int)value;
}


void loop() {
    int value = avg_poti();
    // int sw = digitalRead(POTI_SWITCH);

    int pct = map(value, 0, 1023, 0, 100);
    set_fans(pct);

    delay(25);
}


