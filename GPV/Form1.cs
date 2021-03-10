using SharpDX.DirectInput;
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

namespace GPV
{

    public partial class Form1 : Form
    {
        DirectInput di = new DirectInput();
        Dictionary<int, DeviceInstance> mapped = new Dictionary<int, DeviceInstance>();
        DeviceInstance device = null;
        Joystick pad = null;

        public Form1()
        {
            InitializeComponent();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox2.Update();
            comboBox3.Update();
            IList<DeviceInstance> map = di.GetDevices();
            int index = 0;
            foreach(DeviceInstance i in map)
            {
                bool allowed = false;
                if (i.InstanceName.Contains("Joypad") == true)
                {
                    allowed = true;
                }
                if (i.InstanceName.Contains("Mouse") == true)
                {
                    allowed = false;
                }
                if (i.InstanceName.Contains("Keyboard") == true)
                {
                    allowed = false;
                }
                if (i.InstanceName.Contains("Headset") == true)
                {
                    allowed = false;
                }
                if (i.InstanceName.Contains("Hub") == true)
                {
                    allowed = false;
                }
                if(allowed == true)
                {
                    comboBox1.Items.Add(i.InstanceName);
                    mapped.Add(index, i);
                    index++;
                }
            }
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                button1.BackColor = Color.Red;
                if (comboBox1.Text != "")
                {
                    device = mapped[comboBox1.SelectedIndex];
                    pad = new Joystick(di, device.InstanceGuid);
                    pad.SetCooperativeLevel(this.Handle, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
                    button1.BackColor = Color.Blue;
                    pad.Acquire();
                    button1.BackColor = Color.Green;
                    timer1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                textBox2.Text = ex.Message;
            }
        }

        private void updateDraw()
        {
            string A = "";
            string addon = "";
            int loop = 0;
            foreach(bool B in lastState.Buttons)
            {
                if (B == true)
                {
                    A = A + addon;
                    A = A + loop.ToString();
                    addon = " ";
                }
                loop++;
            }
            /*
               not in use (but will be later)
            A = "";
            addon = "";
            foreach(int B in lastState.PointOfViewControllers)
            {
                if (B != -1)
                {
                    A = A + addon;
                    A = A + B.ToString();
                }
                addon = " ";
            }
            */
        }

        JoystickState lastState = null;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (button2.BackColor != Color.Green)
            {
                button2.BackColor = Color.Green;
            }
            else
            {
                button2.BackColor = Color.Blue;
            }
            if (button1.BackColor != Color.Green)
            {
                timer1.Enabled = false;
                button1.BackColor = Color.Red;
                pad = null;
                device = null;
                comboBox1.Text = "";
                comboBox1.SelectedIndex = -1;
                return;
            }
            try
            {
                pad.Poll();
                JoystickState state = pad.GetCurrentState();
                if (lastState == null)
                {
                    lastState = state;
                    updateDraw();
                    return;
                }
                if (state != lastState)
                {
                    lastState = state;
                    updateDraw();
                }
            }
            catch (Exception ex)
            {
                textBox2.Text = ex.Message;
                button1.BackColor = Color.Yellow;
            }



        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(comboBox2.Text, out int value) == true)
            {
                timer1.Interval = value;
            }
        }
    }
}
