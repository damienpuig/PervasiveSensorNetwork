using Newtonsoft.Json;
using Sider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pervasivecourseworkListener
{
    public class RedisListener
    {
        public const string DNTEMPLATE = "discoverynetwork";
        public const string ERRORTEMPLATE = "Error.{0}";
        

        public const string LUMINOSITY = "Luminosity";
        public const string TEMPERATURE = "Temperature";
        public const string ERROR = "Error";

        public RedisClient baseClient { get; set; }
        public SerialPort port { get; set; }
        public string received { get; set; }
        public List<Value> RawsVal { get; set; }
        public string Action { get; set; }
        public bool ActionMode { get; set; }
        public Stopwatch time { get; set; }

        public const string REQUESTTEMPLATE = "{0}-{1};";
        public enum requestType
        {
            kURDN = 6,
            kURSLEEP = 7,
        }

        public RedisListener()
        {
            time = new Stopwatch();
            baseClient = new RedisClient();
            RawsVal = new List<Value>();
        }

        public void Listen(string name, int Baudrate)
        {
            port = new SerialPort(name, 9600, Parity.None, 8, StopBits.One);


            var redisListener = new ThreadStart(() => { time.Start(); port.DataReceived += port_DataReceived; port.Open(); });
            redisListener.Invoke();

            Console.WriteLine("started listening...");

            while (Action != "Q")
            {

                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("F -- Flush Redis DB");
                Console.WriteLine("Q -- Quit Listener");
                Console.WriteLine("SL -- Change Schedule value (By default 60 seconds)");
                Console.WriteLine("H -- Open the history windows of transactions");
                Console.WriteLine("---------------------------------------------------------------");

                Action = Console.ReadLine();
                Action.ToUpper();

                if (Action == "H")
                {
                    Console.WriteLine("Opening History...");
                    var historyThread = new ThreadStart(() =>
                    {
                        Application.Run(new History(baseClient));
                    });
                    historyThread.Invoke();
                }

                if (Action == "F")
                {
                    Console.WriteLine("Flushing Redis...");
                    baseClient.FlushDb();
                    Console.WriteLine("Flushing done!");
                }

                if (Action == "SL")
                {
                    ActionMode = true;
                    int val = 60; // By default 60 seconds
                    bool entry = false;

                    Console.WriteLine("------------ Change nodes Schedule -----------");
                    while (!entry)
                    {
                        Console.WriteLine("Enter new number of seconds: ");
                        entry = int.TryParse(Console.ReadLine(), out val);
                    }


                    Console.WriteLine("Requesting nodes... new Schedule, every {0} seconds", val);
                    var message = string.Format(REQUESTTEMPLATE, requestType.kURSLEEP, val);
                    port.WriteLine(message);
                    Console.WriteLine("Request sent!");
                }

                ActionMode = false;

            }
            port.Close();
        }

        //We fire an event if we receive an entry
        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("--------------------" + string.Format("{0:dd\\.hh\\:mm\\:ss}", time.Elapsed) + "--------------------");

            //Read the entry
            received = ((SerialPort)sender).ReadLine();

            //Print the entry
            Console.WriteLine(received);

            //Process the entry to Redis
            var result = AddToRedis(received);

            //Propagate the entry to Tier applications using Redis Publish/Subscribe.
            if (result != null) { FreshValue(result); PropagateOnChannels(result); }

        }

        public void FreshValue(Value result)
        {
            if (!ActionMode) { Console.WriteLine(string.Format("Node: {0} - Type: {1} - Value: {2}   /// stamped at {3}", result.nodeId, result.Type, result.Val, result.Stamp.ToLongTimeString())); }
        }

        //Publication to other applications.
        public void PropagateOnChannels(Value value)
        {
            //Channel declaration.
            string channel;

         /* Check if the value is 0 , or if the type is null
            We set the channel value:
            public const string LUMINOSITY = "Luminosity";
            public const string TEMPERATURE = "Temperature";
            public const string ERROR = "Error"; */
            if (value.Val == 0 || string.IsNullOrWhiteSpace(value.Type)) { channel = ERROR; }
            else{ channel = (value.Type == LUMINOSITY) ? LUMINOSITY : TEMPERATURE; }

            //We publish the result on the channel
            baseClient.Publish(channel, value.Serialize());

            //UI entry
            Console.WriteLine("--------------------" + string.Format("PUBLISHED ON CHANNEL: {0}", channel) + "--------------------");
        }


        //Add a result to Redis (The value parameter is the Json exposed by the coordinator)
        public Value AddToRedis(string value)
        {
            try
            {
                //The key of the entry depends on the value of the entry. If the value is 0, the key is of type ERROR.
                string key;

                //Object deserialisation
                var result = JsonConvert.DeserializeObject<Value>(value);

                //Entry stamped
                result.Stamp = DateTime.Now;

                //Check if the value is 0.
                if (result.Val == 0) { key = string.Format(ERRORTEMPLATE, result.nodeId);}
                else { key = result.nodeId; RawsVal.Add(result); }

                //Redis Database insert. We do a Left Push to insert the entry in a ordered list linked to the given key, here node 1, 2, ...
                baseClient.LPush(key, result.Serialize());

                //Get the entry previously inserted.
                var entry = baseClient.LRange(key, 0, RawsVal.Count).First();

                // return the object of type Value
                return JsonConvert.DeserializeObject<Value>(entry);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
