#include <SoftwareSerial.h>
#include "AccelStepper.h"
AccelStepper stepper(1, 3, 6);  // pin 5 step, pin 4 dir


#define rxPin 2
#define txPin 5

// Set up a new SoftwareSerial object
SoftwareSerial mySerial = SoftwareSerial(rxPin, txPin);

//Setup var for run step

#define SteperEnable 8
#define SteperStep 3
#define SteperDir 6

bool enableFW = 0;
bool enablePR = 0;
int stps = 480;

bool enableRun = 0;

bool steperBusy = false;
int32_t steperSpeed = 1000;
int32_t steperSpeedTemp = 1000;

//DRV8825
int delayTime = 5000;  //Delay between each pause (uS)

void step(boolean dir, byte dirPin, byte stepperPin, int steps) {
  digitalWrite(SteperEnable, false);
  digitalWrite(dirPin, dir);
  delay(100);
  for (int i = 0; i < steps; i++) {
    digitalWrite(stepperPin, HIGH);
    delayMicroseconds(delayTime);
    digitalWrite(stepperPin, LOW);
    delayMicroseconds(delayTime);
  }
  digitalWrite(SteperEnable, true);
}

uint8_t Command_Process(Stream &_stream, char *sour, uint8_t len) {
  const char AT_OK[] = "\r\nOK\r\n";
  const char AT_ERROR[] = "\r\nERROR\r\n";
  const char RESET_PROMPT[] = "\r\nReset for new config!";
  uint16_t u16Temp;
  int i;
  //Serial.print("\r\nHandle:\r\n");
  //Serial.print(sour);

  if (memcmp(sour, "AT\r", 3) == 0) {  //AT echo command
    Serial.print("SoftUart: AT");
    _stream.print(AT_OK);
    Serial.println("SoftUart: ");
    Serial.print(AT_OK);
  } else if (memcmp(sour, "AT+STARTFW", 10) == 0) {
    enableFW = 1;
    _stream.print(AT_OK);
    Serial.println("SoftUart: AT+STARTFW");
    Serial.print(AT_OK);
  } else if (memcmp(sour, "AT+STARTPR", 10) == 0) {
    enablePR = 1;
    _stream.print(AT_OK);
    Serial.println("SoftUart: AT+STARTPR");
    Serial.print(AT_OK);
  } else if (memcmp(sour, "AT+SETSPEED=", 12) == 0) {
    steperSpeed = atoi(&sour[12]);
    _stream.print(steperSpeed);
    Serial.print("SoftUart: AT+SETSPEED=");
    Serial.println(steperSpeed);
    Serial.print(AT_OK);
  } else if (memcmp(sour, "AT+RUN", 6) == 0) {
    enableRun = 1;
    _stream.print(AT_OK);
    Serial.println("SoftUart: AT+RUN");
    Serial.print(AT_OK);
  } else if (memcmp(sour, "AT+STOP", 7) == 0) {
    stepper.setCurrentPosition(stps);
    _stream.print(AT_OK);
    Serial.println("SoftUart: AT+STOP");
    Serial.print(AT_OK);
  } else return 0;  //Neu khong co lenh nao duoc xu ly tra ve gia tri
  return 1;         //Mac dinh la lenh AT da duoc xu ly
}

void MyUART_Process() {
#define RX_MAX_LENGH 1023
  static char rx_buffer[RX_MAX_LENGH + 1];
  static int rx_index = 0;

  while (mySerial.available()) {  // get the new byte:
    char inChar = (char)mySerial.read();
    //Serial.print(inChar);
    //Chi nhan ky tu #10
    if (inChar != 10) {
      if (inChar != 13) {
        if (rx_index < RX_MAX_LENGH) {
          rx_buffer[rx_index] = inChar;
          ++rx_index;
        } else {
          //Gan ket thuc:
          inChar = 13;
        }
      }

      if (inChar == 13 && rx_index > 0) {  // Enter
        rx_buffer[rx_index] = NULL;
        //Serial.print("\r\nCOMMAND\r\n");
        Command_Process(mySerial, rx_buffer, rx_index);
        rx_index = 0;
      }
    }
  }  //End of while (Serial.available())
}


void setup() {

  //  pinMode(X_DIR, OUTPUT); pinMode(X_STP, OUTPUT);
  //
  //  pinMode(Y_DIR, OUTPUT); pinMode(Y_STP, OUTPUT);
  //
  //  pinMode(Z_DIR, OUTPUT); pinMode(Z_STP, OUTPUT);
  //
  //  pinMode(EN, OUTPUT);
  //
  //  digitalWrite(EN, LOW);
  Serial.begin(9600);
  Serial.println("Started hard Uart");

  // Define pin modes for TX and RX
  pinMode(rxPin, INPUT);
  pinMode(txPin, OUTPUT);

  // Set the baud rate for the SoftwareSerial object
  mySerial.begin(9600);
  mySerial.println("Started soft Uart");

  pinMode(SteperDir, OUTPUT);
  pinMode(SteperStep, OUTPUT);
  pinMode(SteperEnable, OUTPUT);
  digitalWrite(SteperEnable, HIGH);

  stepper.setMaxSpeed(5000000);
  //stepper.setAcceleration(4000);
}


void loop() {
  MyUART_Process();
  if (steperBusy) {
    if (stepper.currentPosition() != stps && stepper.currentPosition() != -stps) {
      // Run the motor forward at 200 steps/second until the motor reaches 400 steps (2 revolutions):
      stepper.runSpeed();
    } else {
      steperBusy = 0;
      digitalWrite(SteperEnable, HIGH);
    }
  } else {
    if (enableFW) {
      steperBusy = 1;
      steperSpeedTemp = steperSpeed;
      stepper.setCurrentPosition(0);
      stepper.setSpeed(steperSpeedTemp);
      digitalWrite(SteperEnable, LOW);
      enableFW = 0;
      enablePR = 0;
      enableRun = 0;
    } else if (enablePR) {
      steperBusy = 1;
      steperSpeedTemp = steperSpeed;
      stepper.setCurrentPosition(0);
      stepper.setSpeed(-steperSpeedTemp);
      digitalWrite(SteperEnable, LOW);
      enablePR = 0;
      enablePR = 0;
      enableRun = 0;
    } else if (enableRun) {
      steperBusy = 1;
      steperSpeedTemp = steperSpeed;
      stepper.setCurrentPosition(stps + 1);
      stepper.setSpeed(steperSpeedTemp);
      digitalWrite(SteperEnable, LOW);
      enablePR = 0;
      enablePR = 0;
      enableRun = 0;
    }
  }
}
