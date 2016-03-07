#include <Servo.h>
int inPin =2;
int reading;

Servo myservo;

void setup()
{
  myservo.attach(9);
  Serial.begin(9600);
  pinMode(inPin, INPUT);
}

void loop()
{  
  if(Serial.available()>0)
  {
    int opcion = Serial.read();
    
    if (opcion == '0')
    {
      myservo.write(0);
    }
    else if (opcion == '1')
    {
      myservo.write(45);
    }
    else if (opcion == '2')
    {
      myservo.write(90);
    }
    else if (opcion == '3')
    {
      myservo.write(180);
    }
  }
  
}
