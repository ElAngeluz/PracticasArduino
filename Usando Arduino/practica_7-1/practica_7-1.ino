const int lightsensorPin = 2;
const int redledPin=12;
const int greenledPin =13;

int sensorState = 0;

void setup() {
  // put your setup code here, to run once:
  pinMode(redledPin, OUTPUT);
  pinMode (greenledPin, OUTPUT);
  pinMode (lightsensorPin,INPUT); //fotoresistor como entrada
}

void loop() {
  // put your main code here, to run repeatedly:
  sensorState = digitalRead (lightsensorPin);

  if (sensorState == HIGH)
 {
    digitalWrite (redledPin, HIGH);
    digitalWrite (greenledPin, LOW);
 }
 else
 {
    digitalWrite (redledPin, LOW);
    digitalWrite (greenledPin, HIGH);
 }
}
