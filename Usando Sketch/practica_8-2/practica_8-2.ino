#include <Math.h>
#include <Servo.h>

const int xpin = A0;
const int ypin = A1;
const int zpin = A2;

double a_rad,a_deg, tempFilt=0, alpha=0.5;

Servo myservo;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  myservo.attach(9);  
}

void loop() {
  // put your main code here, to run repeatedly:
  double x, y, z;
  x= rescale((double)analogRead(xpin));
  y= rescale((double)analogRead(ypin));
  z= rescale((double)analogRead(zpin));

  delay(100);

  //Calcula el angulo en radianes entre los ejes Z y Y (en unidades g) 
  tempFilt = alpha*(atan2(z,y))+((1-alpha)*tempFilt) ;

  a_rad = tempFilt ;

  //atan2 va de 0 a π y de 0 a –π. Esto cambia ese rango de 0 a 2π.

  if (a_rad < 0) 
    a_rad = 2*PI + a_rad;
  
  a_deg = (a_rad *180/PI);

  if (a_deg>=0 && a_deg<=150)
    myservo.write(a_deg);
}

//Transforma la salida del ADC de 10 bits (0 - 1024) a valores de g (9.81 m/s^2)
double rescale(double c){

  int maxV33ref = 676; //maximo valor de voltaje del acelerometro 3.3V

  double c_g = map(c,0,maxV33ref,-5000,5000); //mapea la salida del acelerometro: 0V - 3.3V a: -5g a 5g 
  c_g = c_g/1000; 
  return c_g;

}
