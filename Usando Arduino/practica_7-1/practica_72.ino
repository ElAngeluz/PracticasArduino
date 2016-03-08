#include <Servo.h>
int inPin =2;
int reading;

Servo myservo;

void setup()
{
  myservo.attach(9);
  pinMode(intPin, INPUT);
}

void loop()
{
  reading = digitalRead(inPin);
  if (reading == HIGH)
  {
    myservo.write(180);
    delay(15);
  }
  else
  {
    myservo.write(0);
    delay(15);
  }
}

