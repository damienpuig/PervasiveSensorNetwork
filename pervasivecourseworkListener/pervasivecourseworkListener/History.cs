using Newtonsoft.Json;
using Sider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pervasivecourseworkListener
{
    public partial class History : Form
    {
        public RedisClient baseClient { get; set; }
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public IEnumerable<Value> Values { get; set; }
        public string SelectedNode { get; set; }

        public History(RedisClient client)
        {
            Values = new List<Value>();
            InitializeComponent();;
            baseClient = client;
            var nodes = baseClient.Keys("*");
            availableNodeControl.Items
                .AddRange(nodes);
        }

        public void Fresh(string[] results)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(results);
            Values = results.Select((entry, r) => { return JsonConvert.DeserializeObject<Value>(entry); });

            label4.Text = results.Count() + " results founded in Redis.";
            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            var query = availableNodeControl.SelectedItem.ToString();
            SelectedNode = query;
            var results = baseClient.LRange(query, 0, int.Parse(comboBox1.SelectedItem.ToString()));
            if (results != null) listBox1.Invoke(new Action<string[]>(Fresh), (object)results);
            button4.Enabled = true;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null) button2.Enabled = true;
            else button2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var NodeGroups = Values.GroupBy(item => new { item.nodeId, item.Type },
                                     (item, group) => new NodeGroup()
                                     {
                                         Id = item.nodeId,
                                         Type = item.Type, 
                                         Values = group.OrderBy(v => v.Stamp).ToList()
                                     }).ToList();

            var GraphFrame = new Graph(NodeGroups);
            GraphFrame.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var thread = Task.Run(() =>
                {
                    var query = Values.Where(v => v.Val == 0).ToList();
                    query.ForEach((value) =>
                        {
                            baseClient.LRem(SelectedNode, 1, value.Serialize());
                        });
                }).ContinueWith(t => MessageBox.Show("Wrong values deleted"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var value = listBox1.SelectedItem.ToString();
            baseClient.LRem(SelectedNode, 1, value);
            listBox1.Items.Remove(value);
            listBox1.Refresh();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null) button3.Enabled = true;
            else button3.Enabled = false;
        }
    }
}
