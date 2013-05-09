#include <CmdMessenger.h>
 
// Mustnt conflict / collide with our message payload data. Fine if we use base64 library ^^ above
char field_separator = '-';
char command_separator = ';';
 
// Attach a new CmdMessenger object to the default Serial port
CmdMessenger cmdMessenger = CmdMessenger(Serial, field_separator, command_separator);
 
String messageReceived;
int led = 13;
int intervalCoordinatorCheck = 5000;
 
enum
{
  kACK              = 1,
  kDATAREQUEST      = 2,
  kDATAANSWER       = 3,
  kSLEEPREQUEST     = 4,
  kNOTIFYAWAKE      = 5,
  kURSLEEP          = 6,
  kSEND_CMDS_END,
};
 
 
 // ------------------ C A L L B A C K  M E T H O D S -------------------------
 void DataCallback()
{
  alert();
  cmdMessenger.sendCmd(kACK, "Data received on coordinator");
  delay(100);
   while ( cmdMessenger.available() )
  {
    char buf[350] = { '\0' };
    cmdMessenger.copyString(buf, 350);
    if(buf[0])
    {
      messageReceived = buf;
      Serial.println(messageReceived);
      delay(100);
    }
  }
}
 
 void SleepChangeCall()
{
  cmdMessenger.sendCmd(kACK, "new schedule on coordinator, from user");
  while ( cmdMessenger.available() )
  {
    char buf[350] = { '\0' };
    cmdMessenger.copyString(buf, 350);
    if(buf[0])
    {
      intervalCoordinatorCheck = atoi(buf);
       cmdMessenger.sendCmd(kSLEEPREQUEST, buf);
       delay(100);
    }
      
  }
}
 
 void AwakeRequestCallback()
{
  cmdMessenger.sendCmd(kACK, "Nodes awaked");
  cmdMessenger.sendCmd(kDATAREQUEST,"Ask for data");
}
 
// ------------------ D E F A U L T  C A L L B A C K S -----------------------
 
void Ack_root()
{
  // In response to ping. We just send a throw-away Acknowledgement to say "im alive"
  while ( cmdMessenger.available() )
  {
    char buf[350] = { '\0' };
    cmdMessenger.copyString(buf, 350);
    if(buf[0])
    {
       Serial.println(buf);
       delay(100);
    }
  }
}
 
void unknownCmd()
{ 
  //nothing
}
 
// ------------------ E N D  C A L L B A C K  M E T H O D S ------------------
 
 void hello_coordinator_device()
{
  cmdMessenger.sendCmd(kACK, "Hello from coordinator");
  delay(100);
}
 
 
void setup() {
pinMode(led, OUTPUT);
Serial.begin(9600);
 
  
 cmdMessenger.discard_LF_CR();
  cmdMessenger.print_LF_CR();
  cmdMessenger.attach(kACK, Ack_root);
  cmdMessenger.attach(unknownCmd);
  cmdMessenger.attach(kDATAANSWER, DataCallback);
  cmdMessenger.attach(kURSLEEP, SleepChangeCall);
  cmdMessenger.attach(kNOTIFYAWAKE, AwakeRequestCallback);
  
  hello_coordinator_device();
}
 
 
void loop() {
cmdMessenger.feedinSerialData();
delay(intervalCoordinatorCheck);
}
 
 
 
void alert()
{
  digitalWrite(led, HIGH);
delay(100);
digitalWrite(led, LOW);    
delay(100);     
digitalWrite(led, HIGH);
delay(100);
digitalWrite(led, LOW);    
delay(100);     
}
