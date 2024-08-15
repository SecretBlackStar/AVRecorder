using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Recorder
{
    public partial class Form2 : Form
    {
        public Form2(string message)
        {
            InitializeComponent();
            lbl_Msg.Text = message;
            Console.WriteLine(message);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        public void setMsg(string msg)
        {
            lbl_Msg.Text = msg;
        }
    }
}
