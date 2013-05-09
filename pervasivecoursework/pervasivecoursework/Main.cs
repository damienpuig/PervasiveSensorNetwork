using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sider;
using Newtonsoft.Json;
using System.IO.Ports;
using System.Threading;

namespace pervasivecoursework
{
    public partial class Main : Form, IObserver<Message<string>>
    {
        public const string LUMINOSITY = "Luminosity";
        public const string TEMPERATURE = "Temperature";
        public const string ERROR = "Error";
        public const string ERRORTEMPLATE = "Error.{0}";
        private const string TEMPLATE = "Node: {0} - Type: {1} - Value: {2}   /// stamped at {3}";
        public RedisClient Subscriber { get; set; }
        private IDisposable UnSubscriber;
        public string ProviderName { get; set; }
        public RedisClient Reader { get; set; }
        public List<Value> Errors { get; set; }

        public Main()
        {
            InitializeComponent();
            comboBox1.Items.Add(LUMINOSITY);
            comboBox1.Items.Add(TEMPERATURE);
            comboBox1.Items.Add(ERROR);
            Subscriber = new RedisClient();
            Reader = new RedisClient();
            Errors = new List<Value>();
        }

        public void FreshL1(string result)
        {
            listBox1.Items.Add(result);
        }
        public void FreshL2(List<string> results)
        {
            if (results.Count > 0) button3.Enabled = true;
            listBox2.Items.AddRange(results.ToArray());
            results.ForEach(v => Errors.Add(JsonConvert.DeserializeObject<Value>(v)));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }


        public virtual void Subscribe(IObservable<Message<string>> provider)
        {
            if (provider != null)
                UnSubscriber = provider.Subscribe(this);
        }

        public void ChangeChannel(string channel)
        {
            ProviderName = channel;
            this.Unsubscribe();
            listBox1.Items.Clear();
            Subscribe(Subscriber.Subscribe(channel));
            label2.Invoke(new Action(notify));
        }

        public void notify()
        {
            label2.Text = string.Format("You subscribed to the Redis channel: {0}", ProviderName); 
        }


        void IObserver<Message<string>>.OnCompleted()
        {
            listBox1.Invoke(new Action<string>(FreshL1), string.Format("The Provider has completed transmitting data through {0}.", ProviderName));
            this.Unsubscribe();
        }
        public virtual void Unsubscribe()
        {
           if(UnSubscriber != null) UnSubscriber.Dispose();
        }

        void IObserver<Message<string>>.OnError(Exception error)
        {
            listBox1.Invoke(new Action<string>(FreshL1), string.Format("{0}: The provider cannot be read data.", ProviderName));
        }

        void IObserver<Message<string>>.OnNext(Message<string> value)
        { 
            if (value.Body != null)
            {
                var jsonResultValue = JsonConvert.DeserializeObject<Value>(value.Body);
                var result = ProviderName == ERROR ?
                    string.Format(TEMPLATE, jsonResultValue.nodeId, jsonResultValue.Type, jsonResultValue.Val, jsonResultValue.Stamp.ToLongTimeString())
                    :
                    jsonResultValue.Serialize();

                listBox1.Invoke(new Action<string>(FreshL1), result);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                ChangeChannel(comboBox1.SelectedItem.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Errors.ForEach((value) =>
                {
                    Reader.LRem(string.Format(ERRORTEMPLATE, value.nodeId), 1, value.Serialize());
                });
            MessageBox.Show("Wrong values deleted");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            Errors.Clear();
            var errorKeys = Reader.Keys(string.Format(ERRORTEMPLATE, "*"));

            errorKeys.ToList().ForEach(key =>
                {
                    var values = Reader.LRange(key, 0, 100).ToList();
                    listBox2.Invoke(new Action<List<string>>(FreshL2), values);
                });
        }


    }
}
