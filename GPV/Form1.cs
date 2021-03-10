using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        gamepad Rendering = new snes();
        Dictionary<int, string> buttonmap = new Dictionary<int, string>();

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
                    loadmapping();
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

        private void loadmapping()
        {
            label11.Text = "Cleared map";
            buttonmap = new Dictionary<int, string>();
            if (Directory.Exists("maps") == false)
            {
                Directory.CreateDirectory("maps");
            }
            if (Directory.Exists("maps/" + comboBox1.Text) == false)
            {
                Directory.CreateDirectory("maps/" + comboBox1.Text);
            }
            if (File.Exists("maps/"+ comboBox1.Text+"/"+comboBox3.Text+".map") == true)
            {
                string[] lines = File.ReadAllLines("maps/" + comboBox1.Text + "/" + comboBox3.Text + ".map");
                foreach(string A in lines)
                {
                    string[] bits = A.Split('=');
                    if (bits.Length == 2)
                    {
                        if(int.TryParse(bits[0],out int buttonid) == true)
                        {
                            buttonmap.Add(buttonid, bits[1]);
                        }
                    }
                }
                label11.Text = "Map loaded from disk";
            }
        }

        private void savemapping()
        {
            if (Directory.Exists("maps") == false)
            {
                Directory.CreateDirectory("maps");
            }
            if (Directory.Exists("maps/" + comboBox1.Text) == false)
            {
                Directory.CreateDirectory("maps/" + comboBox1.Text);
            }
            if (File.Exists("maps/" + comboBox1.Text + "/" + comboBox3.Text + ".map") == true)
            {
                File.Delete("maps/" + comboBox1.Text + "/" + comboBox3.Text + ".map");
            }
            List<string> lines = new List<string>();
            foreach(KeyValuePair<int,string> mid in buttonmap)
            {
                lines.Add("" + mid.Key.ToString() + "=" + mid.Value);
            }
            label11.Text = "Map saved to disk";
            File.WriteAllLines("maps/" + comboBox1.Text + "/" + comboBox3.Text + ".map", lines);
        }

        int redraws = 0;

        Bitmap LastDraw = null;

        public void render(int[] pushedbuttons)
        {
            Graphics background = null;
            Bitmap bg = new Bitmap(960, 540, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            if (Rendering.backgroundart != null)
            {
                bg = new Bitmap(Rendering.backgroundart, new Size(960, 540));
            }
            background = Graphics.FromImage(bg);
            foreach (int a in pushedbuttons)
            {
                if (buttonmap.ContainsKey(a) == true)
                {
                    if (Rendering.buttons.ContainsKey(buttonmap[a]) == true)
                    {
                        background.DrawImage(Rendering.buttons[buttonmap[a]], 0, 0, 960, 540);
                    }
                }
            }
            redraws++;
            label7.Text = redraws.ToString();
            if (LastDraw != null)
            {
                LastDraw.Dispose();
                LastDraw = null;
            }
            LastDraw = bg;
            pictureBox1.BackgroundImage = LastDraw;
        }

        private void updateDraw()
        {
            string A = "";
            string addon = "";
            int loop = 0;
            List<int> pushedbuttons = new List<int>();
            foreach (bool B in lastState.Buttons)
            {
                if (B == true)
                {
                    pushedbuttons.Add(loop);
                    A = A + addon;
                    A = A + loop.ToString();
                    addon = " ";
                }
                loop++;
            }
            if(asking_q == true)
            {
                if(pushedbuttons.Count == 1)
                {
                    ask_q_detected = pushedbuttons[0];
                    label10.Text = "Button: " + ask_q_detected.ToString();
                }
            }
            else
            {
                render(pushedbuttons.ToArray());
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
                string A = string.Join("|", state.Buttons);
                string B = string.Join("|", lastState.Buttons);
                if (A != B)
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {
            ask_q = 0;
            showask();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ask_q--;
            if (ask_q < 0) ask_q = 0;
            showask();
        }

        private void showask()
        {
            asking_q = true;
            panel1.Visible = true;
            ask_q_detected = -1;
            label10.Text = "?";
            label9.Text = Rendering.buttons.Keys.ElementAt(ask_q);
            timer1.Enabled = true;

        }

        int ask_q = 0;
        int ask_q_detected = -1;
        bool asking_q = false;

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (ask_q_detected != -1)
            {
                if (buttonmap.ContainsKey(ask_q_detected) == false)
                {
                    buttonmap.Add(ask_q_detected, Rendering.buttons.Keys.ElementAt(ask_q));
                }
                else
                {
                    buttonmap[ask_q_detected] = Rendering.buttons.Keys.ElementAt(ask_q);
                }

                ask_q++;
                if (ask_q >= Rendering.buttons.Count)
                {
                    panel1.Visible = false;
                    asking_q = false;
                    panel1.Visible = false;
                    ask_q_detected = -1;
                    savemapping();
                }
                else
                {
                    showask();
                }
            }
            else
            {
                Shake(this);
            }
        }
        private static void Shake(Form form)
        {
            var original = form.Location;
            var rnd = new Random(1337);
            const int shake_amplitude = 10;
            for (int i = 0; i < 10; i++)
            {
                form.Location = new Point(original.X + rnd.Next(-shake_amplitude, shake_amplitude), original.Y + rnd.Next(-shake_amplitude, shake_amplitude));
                System.Threading.Thread.Sleep(20);
            }
            form.Location = original;
        }
    }

    public abstract class gamepad
    {
        public Bitmap backgroundart = null;
        public Dictionary<string, Bitmap> buttons = new Dictionary<string, Bitmap>();
    }

    public class snes : gamepad
    {
        public snes()
        {
            backgroundart = GPV.Properties.Resources.snes_background;
            buttons.Add("up", GPV.Properties.Resources.snes_up);
            buttons.Add("down", GPV.Properties.Resources.snes_down);
            buttons.Add("left", GPV.Properties.Resources.snes_left);
            buttons.Add("right", GPV.Properties.Resources.snes_right);
            buttons.Add("a", GPV.Properties.Resources.snes_a);
            buttons.Add("b", GPV.Properties.Resources.snes_b);
            buttons.Add("x", GPV.Properties.Resources.snes_x);
            buttons.Add("y", GPV.Properties.Resources.snes_y);
            buttons.Add("L", GPV.Properties.Resources.snes_L);
            buttons.Add("R", GPV.Properties.Resources.snes_R);
            buttons.Add("start", GPV.Properties.Resources.snes_start);
            buttons.Add("select", GPV.Properties.Resources.snes_select);
        }
    }



}
