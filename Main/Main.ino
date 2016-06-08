/*
  Name:		Sketch1.ino
  Created:	3/13/2016 6:43:19 PM
  Author:	hejtmy
*/
#include <Keyboard.h>
#define numberOfButtons 4

String serialInput;
int timeout = 25;
bool connected = false;
bool photoresistorUse = false;
bool pulsing = false;
//speed for the delay factor
int speed = 20;

//pin setup
int pulsePin = 13;
int photoresistorPin = 0;

char* buttonColours[numberOfButtons] = {"RED","BLUE","YELLOW","GREEN"};
int buttonPins[numberOfButtons] = {7, 6, 5, 4};
//ascii chars representing
char buttonKeys[numberOfButtons] = {'a','d','j','l'};

int photoresistorThreshold = 500;

char untilChar = '\!';

// the setup function runs once when you press reset or power the board
void setup() {
  Serial.begin(9600);
  Serial.setTimeout(timeout);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB
  }
  //initialise buttons
  for(int i = 0; i < numberOfButtons;i++){
    pinMode(buttonPins[i], INPUT_PULLUP);
  }
}

// the loop function runs over and over again until power down or reset
void loop() {
  serialInput = Serial.readStringUntil(untilChar);
  if (serialInput != "") {
    if (serialInput == "WHO") {
      LettingKnow();
    }
    if (connected) {
      ListenForOrders();
    }
  }
  if (connected){
    if (photoresistorUse) {
      PhotoresistorAction();
    }
    ButtonsAction();
  }
}
void LettingKnow() {
  float time = millis();
  while (true) {
    serialInput = Serial.readStringUntil(untilChar);
    if (serialInput == "DONE") {
      Connect();
      break;
    }
    Serial.println("ARDUINO");
    if (millis() - time > 1000) {
      Serial.println("TIME IS UP");
      break;
    }
    delay(speed);
  }
}
void Connect(){
  Keyboard.begin();
  connected = true;
}
void Disconnect(){
  connected = false;
  Keyboard.end();
}
void Restart(){
  Disconnect();
}
void ListenForOrders() {
  if (serialInput != "") {
    if (serialInput == "RESTART") {
      Restart();
    }
    if (serialInput == "DISCONNECT") {
      Disconnect();
    }
    if (serialInput == "PULSE+") {
      StartPulse();
      SendDone();
    }
    if (serialInput == "PULSE-") {
      CancelPulse();
      SendDone();
    }
    if (serialInput == "BLINK") {
      Blink();
      SendDone();
    }
    if (serialInput == "PHOTO+") {
      StartPhotoresistor();
      SendDone();
    }
    if (serialInput == "PHOTO-") {
      StopPhotoresistor();
      SendDone();
    }
    if (serialInput == "PHOTO-CALIBRATE") {
      CalibratePhotoresistor();
      SendDone();
    }
  }
}
void SendDone(){
  Serial.println("DONE");
}
void Blink(){
  digitalWrite(13, HIGH);
  delay(100);
  digitalWrite(13, LOW);
}
void StartPhotoresistor(){
  photoresistorUse = true;
  CalibratePhotoresistor();
}
void StopPhotoresistor(){
  photoresistorUse = false;
}
void CalibratePhotoresistor(){
  //should do differently?
  photoresistorThreshold = analogRead(photoresistorPin);
}
void PhotoresistorAction(){
  if(digitalRead(7) == LOW){
    Serial.println(analogRead(photoresistorPin));
    static bool alreadyReacted = false;
    if (analogRead(photoresistorPin) > photoresistorThreshold) {
        if(!alreadyReacted){
          alreadyReacted = true;
          //do sth
        }
    } else {
      alreadyReacted = false;
    }
  }
}
void ButtonsAction(){
  for(int i=0; i < numberOfButtons; i++){
    if(digitalRead(buttonPins[i]) == LOW){
      Keyboard.write(buttonKeys[i]);
      Serial.println(buttonColours[i]);
    }
  }
}
void StartPulse(){
  pulsing = true; // no fuctionality yet
  digitalWrite(pulsePin, HIGH);
}
void CancelPulse(){
  pulsing = false; // no fuctionality yet
  digitalWrite(pulsePin, LOW);
}
