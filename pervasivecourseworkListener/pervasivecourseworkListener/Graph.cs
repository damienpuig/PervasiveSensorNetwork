using GraphLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pervasivecourseworkListener
{
    /* The given class will expose data coming from the history window, using a list of Node group. We show the NodeGroup Properties
     *  public string Id { get; set; }
     *  public string Type { get; set; }
     *  public List<Value> Values { get; set; }
     */
    public partial class Graph : Form
    {
        private const string GRAPHNAMETEMPLATE = "Graph node: {0},  type: {1}";
        public List<NodeGroup> NodesGroup { get; set; }

        public Graph(List<NodeGroup> nodesGroup)
        {
            this.Text = "Arduino graph system";
            InitializeComponent();
            NodesGroup = nodesGroup;

            display.Smoothing = System.Drawing.Drawing2D.SmoothingMode.None;

            //Build dynamically the graph from nodes
            CalcDataGraphs(NodesGroup);

            display.Refresh();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void RefreshGraph()
        {
            display.Refresh();
        }

        protected void CalcSinusFunction_2(DataSource src, int idx, List<Value> values)
        {
            values.OrderBy(v => v.Stamp);

            for (int i = 0; i < src.Length; i++)
            {
                src.Samples[i].x = i;

                src.Samples[i].y = values.ElementAt(i).Val;
            }
            src.OnRenderYAxisLabel = RenderYLabel;
        }

        private void ApplyColorSchema()
        {

            Color[] cols = { Color.DarkRed, 
                                         Color.DarkSlateGray,
                                         Color.DarkCyan, 
                                         Color.DarkGreen, 
                                         Color.DarkBlue ,
                                         Color.DarkMagenta,                              
                                         Color.DeepPink };

            for (int j = 0; j < NodesGroup.Count; j++)
            {
                display.DataSources[j].GraphColor = cols[j % 7];
            }
            display.BackgroundColorTop = Color.White;
            display.BackgroundColorBot = Color.LightGray;
            display.SolidGridColor = Color.LightGray;
            display.DashedGridColor = Color.LightGray;



        }

        protected void CalcDataGraphs(List<NodeGroup> Groups)
        {

            this.SuspendLayout();

            display.DataSources.Clear();
            display.SetDisplayRangeX(0, 400);

            //For each node, build a graph
            Groups.ForEach((group) =>
                {
                    var id = int.Parse(group.Id);
                    display.DataSources.Add(new DataSource());
                    display.DataSources[Groups.IndexOf(group)].Name = string.Format(GRAPHNAMETEMPLATE, group.Id, group.Type);
                    display.DataSources[Groups.IndexOf(group)].OnRenderXAxisLabel += RenderXLabel;
                    display.DataSources[Groups.IndexOf(group)].Length = group.Values.Count();
                    display.PanelLayout = PlotterGraphPaneEx.LayoutMode.TILES_VER;
                    display.DataSources[Groups.IndexOf(group)].AutoScaleY = false;
                    display.DataSources[Groups.IndexOf(group)].SetDisplayRangeY(0, 1000);
                    display.DataSources[Groups.IndexOf(group)].SetGridDistanceY(100);
                    display.DataSources[Groups.IndexOf(group)].OnRenderYAxisLabel = RenderYLabel;
                    CalcSinusFunction_2(display.DataSources[Groups.IndexOf(group)], Groups.IndexOf(group), group.Values);

                });



            ApplyColorSchema();

            this.ResumeLayout();
            display.Refresh();

        }

        private string RenderXLabel(DataSource s, int idx)
        {
         var rightNode =   NodesGroup.Where(n => s.Name == string.Format(GRAPHNAMETEMPLATE, n.Id, n.Type)).First();
         return string.Format("{0:d/M/yyyy HH:mm:ss}", rightNode.Values[idx].Stamp);
        }

        private string RenderYLabel(DataSource s, float value)
        {
            return string.Format("{0:0.0}", value);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            display.Dispose();

            base.OnClosing(e);
        }
    }
}
