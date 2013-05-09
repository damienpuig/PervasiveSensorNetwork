// SENDER
#include <CmdMessenger.h>
#include <avr/sleep.h>
#include <avr/wdt.h>
#define sleepTime  2                       //number of 8 second sleep cycles
 
volatile byte wdt=0;                       //used to cound number of sleep cycles
 
String nodeLum = "\"Luminosity\"";
String nodeTemp = "\"Temperature\"";
int analogValueLuminosity;
int analogValueTemperature;
int led = 13;
 
String message;
int nodeId = 1;
int Sync_Coordinator = 5000;
 
char field_separator = '-';
char command_separator = ';';
CmdMessenger cmdMessenger = CmdMessenger(Serial, field_separator, command_separator);
 
 
// Commands we send from the Arduino to be received on the PC
enum
{
  kACK              = 1,
  kDATAREQUEST      = 2,
  kDATAANSWER       = 3,
  kSLEEPREQUEST     = 4,
  kNOTIFYAWAKE      = 5,
  kSEND_CMDS_END,
};
 
 
void ChangeSleepCallback()
{
  cmdMessenger.sendCmd(kACK, "ChangeSleep Received on Node 1");
  delay(100);
  while ( cmdMessenger.available() )
  {
    char buf[350] = { '\0' };
    cmdMessenger.copyString(buf, 350);
    if(buf[0])
    {
      //sleepTime = atoi(buf);
      cmdMessenger.sendCmd(kACK, buf);
      delay(100);
    }
      
  }
}
 
void DataRequestCallback()
{
 
    analogValueLuminosity = analogRead(1);
    analogValueTemperature = Thermister(analogRead(5));
 
    char charBuf[350];
    
    message = formatdata(nodeId, analogValueLuminosity, nodeLum);
    message.toCharArray(charBuf, 350);
    cmdMessenger.sendCmd(kDATAANSWER, charBuf);
    delay(Sync_Coordinator);
    
    message = formatdata(nodeId, analogValueTemperature, nodeTemp);
    message.toCharArray(charBuf, 350);
    cmdMessenger.sendCmd(kDATAANSWER, charBuf);
    delay(Sync_Coordinator);
}
 
void NotifyAwakeRequest()
{
    cmdMessenger.sendCmd(kNOTIFYAWAKE, "Node 1 awaked");
}
 
// ------------------ D E F A U L T  C A L L B A C K S -----------------------
void hello_end_device()
{
  cmdMessenger.sendCmd(kACK, "Hello from node 1");
    delay(100);
}
 
void unknownCmd()
{
  //nothing
}
 
 
void setup()
{
  pinMode(led, OUTPUT);
  Serial.begin(9600);
  
  
 
  cmdMessenger.discard_LF_CR(); // Useful if your terminal appends CR/LF, and you wish to remove them
  cmdMessenger.print_LF_CR();   // Make output more readable whilst debugging in Arduino Serial Monitor
  
  // Attach default / generic callback methods
  cmdMessenger.attach(unknownCmd);
  cmdMessenger.attach(kSLEEPREQUEST, ChangeSleepCallback);
  cmdMessenger.attach(kDATAREQUEST, DataRequestCallback);
 
 
  hello_end_device();
  
  delay(1000);
 
  Serial.begin(9600);
 
  setup_watchdog();                        // set prescaller and enable interrupt                  
  set_sleep_mode(SLEEP_MODE_PWR_DOWN);     // sleep mode is set here Power Down uses the least current
                                           // system clock is turned off, so millis won't be reliable!
  delay(10);
}
 
void loop()
{
  digitalWrite(led, HIGH); //turn led on while awake
  delay(3000);
  NotifyAwakeRequest();
  delay(Sync_Coordinator);
  cmdMessenger.feedinSerialData();
  system_sleep(); 
     
}
 
String formatdata(int id, int value, String nodeType)
{
    return String("{ \"nodeId\":" +  String(nodeId) + ", \"type\":" +  nodeType +  ", \"value\":" + String(value) +" }");
}
 
double Thermister(int RawADC) {
 double Temp;
 Temp = log(((10240000/RawADC) - 10000));
 Temp = 1 / (0.001129148 + (0.000234125 + (0.0000000876741 * Temp * Temp ))* Temp );
 Temp = Temp - 273.15;
 return Temp;
}
 
void system_sleep() {
  digitalWrite(led, LOW); //turn led on while awake
  ADCSRA |= (0<<ADEN);                     // disable ADC
  sleep_enable();                          // enable sleeping
  sleep_mode();                            // activate system sleep
  // sleeping ... 
  // first action after leaving WDT Interrupt Vector:
  if (wdt > sleepTime) {                  // sleep for this number times 8 seconds
    sleep_disable();                       // disable sleep  
    ADCSRA |= (1<<ADEN);                   // switch ADC on    
    wdt = 0;                               // reset watchdog counter   
    }else{
    system_sleep();                        // go back to sleep until it's time
  }
}
 
void setup_watchdog() {
cli(); //disable global interrupts
  MCUSR = 0x00;  //clear all reset flags 
  //set WD_ChangeEnable and WD_resetEnable to alter the register
  WDTCSR |= (1<<WDCE) | (1<<WDE);   // this is a timed sequence to protect WDTCSR
  // set new watchdog timeout value to 1024K cycles (~8.0 sec)
  WDTCSR = (1<<WDP3) | (1<<WDP0);
  //enable watchdog interrupt
  WDTCSR |= (1<<WDIE);    
sei(); //enable global interrupts
}
 
// Watchdog Interrupt Service Routine. 
// Very first thing after sleep wakes with WDT Interrupt
ISR(WDT_vect) {
  wdt++;  // increment the watchdog timer
}
