using Sider;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pervasivecourseworkListener
{
    class Program
    {

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Start a redislistener instance
            var listener = new RedisListener();

            //Listen to a given port (here we statically defined the port COM6, using the baudrate 9600
            listener.Listen("COM6", 9600);
        }
    }
}
