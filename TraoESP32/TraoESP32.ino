#include <Arduino.h>
#include <WiFi.h>
#include <WiFiMulti.h>
#include <WiFiClientSecure.h>
#include <WebSocketsClient.h>
#include <arduino-timer.h>

#define USE_SERIAL Serial

union {
  struct {
    uint8_t x1[3];
    uint8_t cmd;
    uint8_t x2[4];
    uint8_t crc;
    uint8_t x3[3];
  };
  struct {
    uint8_t x4[4];
    float value;
    uint8_t x5[4];
  };
  struct {
    uint8_t x6[3];
    uint8_t buffer[6];
    uint8_t x7[3];
  };
} plotPackage;
WiFiMulti WiFiMulti;
WebSocketsClient webSocket;
auto timer = timer_create_default();
Timer<> default_timer;

float rad = 0;
bool plotEnable = true;

void cmddump(uint8_t *mem, uint32_t len) {
  if(mem[0] != 0xF0 || mem[len - 1] != 0xA0) return;
  if(mem[1] == 0x01)
  {
  }
  else if(mem[1] == 0x02)
  {
  }
  else if(mem[1] == 0x03)
  {
  }
  else if(mem[1] == 0x04)
  {
    if(len == 7)
    {
      uint32_t speed = mem[2] + (mem[3] << 8) + (mem[4] << 16) + (mem[5] << 24);
      // TempSerial.print("AT+SETSPEED=%d\r", speed);
    }
  }
  else if(mem[1] == 0x05)
  {
    plotEnable = true;
  }
  else if(mem[1] == 0x06)
  {
    plotEnable = false;
  }
}

bool sendPlotData(void *) 
{
  if(webSocket.isConnected())
  {
    rad += 0.01;
    plotPackage.value = sin(rad) * 300;
    webSocket.sendBIN(plotPackage.buffer, sizeof(plotPackage.buffer));
    return true;    
  }
  else
  {
    return false;
  }
}

auto sendPlotDataTask = timer.every(50000, sendPlotData);
uint8_t startMesage[] = {0xF0, 0x01, 0xAA};

void webSocketEvent(WStype_t type, uint8_t * payload, size_t length) {

	switch(type) {
		case WStype_DISCONNECTED:
      timer.cancel(sendPlotDataTask); 
			break;
		case WStype_CONNECTED:
			webSocket.sendBIN(startMesage, sizeof(startMesage));
      sendPlotDataTask = timer.every(10, sendPlotData);
			break;
		case WStype_TEXT:
			break;
		case WStype_BIN:
			cmddump(payload, length);
			break;
		case WStype_ERROR:			
		case WStype_FRAGMENT_TEXT_START:
		case WStype_FRAGMENT_BIN_START:
		case WStype_FRAGMENT:
		case WStype_FRAGMENT_FIN:
			break;
	}

}

void setup() {
  timer.cancel(sendPlotDataTask); 
	plotPackage.cmd = 0xF0;
  plotPackage.crc = 0xAA;
	USE_SERIAL.begin(115200);

	//USE_SERIAL.setDebugOutput(true);

	WiFiMulti.addAP("F11_A10", "88888888");

	//WiFi.disconnect();
	while(WiFiMulti.run() != WL_CONNECTED) {    
	  USE_SERIAL.print(".");
		delay(100);
	}
 
  USE_SERIAL.print("IP address:\t");
  IPAddress myIP = WiFi.localIP();
  USE_SERIAL.println(myIP);
  
	webSocket.begin("192.168.68.164", 5144, "/ws/sensor");
	webSocket.onEvent(webSocketEvent);
	//webSocket.setAuthorization("user", "Password");
	webSocket.setReconnectInterval(5000);

}

void loop() {
  webSocket.loop();
  timer.tick();
}
