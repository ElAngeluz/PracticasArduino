/*
 * File:    practica9DSCC
 * Grupo: 
 */

//Ultrasonido
int trigPin = 15;  //PIN A1 en el Arduino
int echoPin = 14;  //PIN A0 en el Arduino
float v=331.5+0.6*24; //Estimación de la velocidad del sonido, 24 es el valor de temperatura en Centigrados. Cambiar este valor de temperatura de ser necesario.

//Tiempo de espera en ms entre cada iteración del lazo principal, esto implica una frecuencia de muestreo de 4 Hz
const int tiempoEspera = 250;

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

    pinMode(trigPin, OUTPUT);
    pinMode(echoPin, INPUT);
}

void loop(void) {

  float d;

  d = distanceCM();

  Serial.println(d);

  delay(tiempoEspera);
}

