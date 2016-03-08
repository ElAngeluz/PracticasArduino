#include "ESP8266.h"
#include <SoftwareSerial.h>

#define SSID        "CTI_DOMO"
#define PASSWORD    "ct1esp0l15"

#define HOST_NAME   "api.thingspeak.com"
#define HOST_PORT   80

SoftwareSerial mySerial(3,2); /* RX:D2, TX:D3 */
ESP8266 wifi(mySerial);

//cabecera
String key = "QTY07M1JZZB9KC77";
String head = "GET /update?api_key=";

String keyTw = "EFLAD0IXWOMGZ29D";
String headTw = "GET /apps/thingtweet/1/statuses/update?api_key=";
String statusTw = "&status=";

//String tail = "HTTP/1.0\r\n\r\n";
String tail = "\r\n\r\n";

String command;
float d;

//Ultrasonido
int trigPin = 15;  //PIN A1 en el Arduino
int echoPin = 14;  //PIN A0 en el Arduino
float v=331.5+0.6*24; //Estimación de la velocidad del sonido, 24 es el valor de temperatura en Centigrados. Cambiar este valor de temperatura de ser necesario.


//Tiempo de espera en ms entre cada iteración del lazo principal, esto implica una frecuencia de muestreo de 4 Hz
const int tiempoEspera = 20000;

//Esta función retorna la distancia medida por el sensor en centimetros.
float distanceCM()
{
  //envia un pulso de 5 microsegundos en el pin trig
  digitalWrite(trigPin, LOW); //Mantiene LOW el pin por 3 us
  delayMicroseconds(3);
  digitalWrite(trigPin, HIGH); //Mantiene HIGH el pin por 5 us, efectivamente este es el pulso
  delayMicroseconds(5);
  digitalWrite(trigPin, LOW); //Regresa a LOW, desactiva el pulso

   //Escuchar eco. El pin echoPin envia un pulso cuya duración es proporcional al tiempo entre el pulso y el eco
   float tUs = pulseIn(echoPin, HIGH); //retorna la duración, en microsegundos del pulso en echoPin
   float t = tUs / 1000.0 / 1000.0 / 2; // convierte la duración a segundos y divide para dos (el eco va y viene la distancia medida)
   float d = t*v; //distancia es tiempo por velocidad, en metros
   return d*100; // en cm
}

void setup(void) {
    Serial.begin(9600);
    Serial.print("setup begin\r\n");

    Serial.print("FW Version:");
    Serial.println(wifi.getVersion().c_str());

    while(!wifi.setOprToStation()){
      Serial.print("to station + softap err\r\n");
    }
    Serial.print("to station + softap ok\r\n");
    
    //hasta que no consiga una direccion no sale
    while(!wifi.joinAP(SSID, PASSWORD)){
      Serial.println("Obteniendo ip..");
    }
    Serial.print("Join AP success\r\n");
    Serial.print("IP:");    
    Serial.println( wifi.getLocalIP().c_str()); //me retorna la direccion
    if (wifi.disableMUX()) {
        Serial.print("single ok\r\n");
    } else {
        Serial.print("single err\r\n");
    }    
    
    Serial.print("setup end\r\n");

    pinMode(trigPin, OUTPUT);
    pinMode(echoPin, INPUT);
}

void loop(void) {  
  d = distanceCM();  
  sendDistance(d);  
  delay(tiempoEspera);
}

void sendDistance(float d)
{
  String field = "&field1=";
  //field = "&field1=";
  if(!isnan(d))
  { 
    if(d<10)
    {
      String Mess= "#IoT #ESPOL #Prueba, Distancia: ";
      Serial.println("\nEnviando Alerta..");
      command = headTw+keyTw+statusTw+Mess+String(d,2)+tail;
      wifi.createTCP(HOST_NAME, HOST_PORT);
      const char * data = command.c_str();
      Serial.print(data);
      wifi.send((const uint8_t *) data, command.length());
      wifi.releaseTCP();
    }else{
      Serial.println("Enviando..");
      command = head+key+field+String(d,2)+tail;
      wifi.createTCP(HOST_NAME, HOST_PORT);
      const char * data = command.c_str();
      Serial.print(data);
      wifi.send((const uint8_t *) data, command.length());
      wifi.releaseTCP();
    }
  }
}
