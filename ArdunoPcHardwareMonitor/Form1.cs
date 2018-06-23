using System;
using System.Windows.Forms;
using System.IO.Ports;
using OpenHardwareMonitor.Hardware;

namespace ArdunoPcHardwareMonitor
{
    public partial class Form1 : Form
    {
        
            Computer c = new Computer()
            {
                GPUEnabled = true,
                CPUEnabled = true,
                RAMEnabled = true
            };

            float gpu_temp, cpu_temp, gpu_load, cpu_load,gpu_memory,ram_usage_percent,ram_used,ram_avilable;
            string OUTPUT,cpu,gpu,ram;
            bool sent = true;
            private SerialPort port = new SerialPort();
            public Form1()
            {
                InitializeComponent();
                Init();
            }

        private void Init()
        {
            try
            {
                notifyIcon1.Visible = false;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Handshake = Handshake.None;
                port.RtsEnable = true;
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    comboBox1.Items.Add(port);
                }
                port.BaudRate = 9600;
 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            disconnect();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Status();
        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.PortName = comboBox1.Text;
                    port.Open();
                    timer1.Interval = Convert.ToInt32(comboBox2.Text);
                    timer1.Enabled = true;
                    toolStripStatusLabel1.Text = "Sending data...";
                    label2.Text = "Connected";
                    port.WriteLine("[CLS]");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
            try
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            c.Open();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
                if (FormWindowState.Minimized == this.WindowState)
                {
                    notifyIcon1.Visible = true;
                    try
                    {
                        notifyIcon1.ShowBalloonTip(500, "Arduino PC Hardware Monitor", toolStripStatusLabel1.Text, ToolTipIcon.Info);
                        this.Hide();
                     }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                   
                }



        }

        private void Status()
        {



            foreach (var hardwadre in c.Hardware)
            {
               
                if (hardwadre.HardwareType == HardwareType.GpuNvidia || hardwadre.HardwareType == HardwareType.GpuAti)
                {
                    gpu = hardwadre.Name;
                    hardwadre.Update();
                    foreach (var sensor in hardwadre.Sensors)
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Equals("GPU Core"))
                        {
                          
                            gpu_temp = sensor.Value.GetValueOrDefault();
                            
                        }
                        else if(sensor.SensorType == SensorType.Load && sensor.Name.Equals("GPU Core"))
                        {
                            gpu_load = sensor.Value.GetValueOrDefault();
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name.Equals("GPU Memory"))
                        {
                            gpu_memory = sensor.Value.GetValueOrDefault();
                        }

                }

                if (hardwadre.HardwareType == HardwareType.CPU)
                {
                    cpu = hardwadre.Name;
                    hardwadre.Update();
                    foreach (var sensor in hardwadre.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Equals("CPU Package"))
                        {
                            cpu_temp = sensor.Value.GetValueOrDefault();


                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name.Equals("CPU Total"))
                        {
                            cpu_load = sensor.Value.GetValueOrDefault();
                        }
                    }


                }

                if (hardwadre.HardwareType == HardwareType.RAM)
                {
                    hardwadre.Update();
                    ram = hardwadre.Name;
                    foreach (var sensor in hardwadre.Sensors)
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Equals("Memory"))
                        {
                            ram_usage_percent = sensor.Value.GetValueOrDefault();


                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name.Equals("Used Memory"))
                        {
                            ram_used = sensor.Value.GetValueOrDefault();
                        }
                        else if(sensor.SensorType == SensorType.Data && sensor.Name.Equals("Available Memory"))
                        {
                            ram_avilable = sensor.Value.GetValueOrDefault();
                        }

                }

            }
            try
            {
                OUTPUT = "[CPU " + cpu + "][    Temp : "+cpu_temp+ "`C ][    Load : " + cpu_load.ToString("F") + "% ][GPU " + gpu.Replace("NVIDIA","").Trim()+ "][    Temp : " + gpu_temp + "`C ][    Core : " + gpu_load.ToString("F") + "% ][    Memory : " + gpu_memory.ToString("F") + "% ][RAM " + ram + "][    Usage : " + ram_usage_percent.ToString("0.00") + "% ][    Used : " + ram_used.ToString("F") + "GB ][    Avilable : " + ram_avilable.ToString("F") + "GB ][END]";
                //port.Encoding = System.Text.Encoding.GetEncoding(1252);
                port.WriteLine(OUTPUT);
                //dd(OUTPUT);
            }
            catch (Exception ex)
            {
                timer1.Stop();
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = "Arduino's not responding...";
            }
        }

        private void disconnect()
        {
            try
            {
                port.WriteLine("DIS");
                System.Diagnostics.Debug.WriteLine("DIS");
                port.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label2.Text = "Disconnected";
            timer1.Enabled = false;
            toolStripStatusLabel1.Text = "Connect to Arduino...";
        }

        private void dd(String a)
        {
            System.Diagnostics.Debug.WriteLine(a);
        }
    }
}
