using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace icmpGraph
{
    public partial class Form1 : Form
    {
        public Dictionary<string, List<double>> ips = new Dictionary<string, List<double>>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar2.Value = 5;
            comboBox1.Text = "Bright";
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string regex = @"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$";
                Match match = Regex.Match(textBox1.Text, regex);

                if (match.Success && match.Groups.Count == 5)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        if (Int32.Parse(match.Groups[i].Value) < 0 || Int32.Parse(match.Groups[i].Value) > 255)
                        {
                            debug.verbose($"Octet {i} invalid: [{match.Groups[i].Value}]");
                            return;
                        }
                    }
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        if (listView1.Items[i].Text == textBox1.Text)
                        {
                            return;
                        }
                    }

                    listView1.Items.Add(textBox1.Text);
                    ips.Add(textBox1.Text, new List<double>());
                    textBox1.Clear();
                    listView1.Columns[0].Width = -2;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                debug.verbose("Staring ping...");
                button1.Text = "Stop";
                
                textBox1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;

                ips.Clear();
                chart1.Series.Clear();
                List<string> tmpNames = new List<string>();
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    tmpNames.Add(listView1.Items[i].Text);
                    listView1.Items[i].SubItems.Clear();
                }

                listView1.Items.Clear();
                for (int i = 0; i < tmpNames.Count; i++)
                {
                    listView1.Items.Add(tmpNames[i]);
                    ips.Add(tmpNames[i], new List<double>());
                }

                timer1.Enabled = true;

                return;
            }

            if (button1.Text == "Stop")
            {
                debug.verbose("Stopping ping...");
                button1.Text = "Start";
                timer1.Enabled = false;
               
                textBox1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;

                return;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = $"Interval: {trackBar1.Value} ms";
            timer1.Interval = trackBar1.Value;
            debug.verbose($"Setting interval: [{trackBar1.Value}]");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping();

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (chart1.Series.Count != listView1.Items.Count)
                {
                    chart1.Series.Add(listView1.Items[i].Text);
                    chart1.Series[chart1.Series.Count - 1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart1.Series[chart1.Series.Count - 1].BorderWidth = trackBar2.Value;
                }
                
                debug.verbose($"Sending ping to [{listView1.Items[i].Text}]");
                PingReply reply = pinger.Send(listView1.Items[i].Text);

                debug.verbose($"Ping status: [{reply.Status.ToString()}]");

                if (reply.Status == IPStatus.Success)
                {
                    int count = 1;
                    double last, min, max, first, mean = 0;
                    

                    last = (double)reply.RoundtripTime;

                    if (ips.ContainsKey(listView1.Items[i].Text))
                    {
                        List<double> tmpList = ips[listView1.Items[i].Text];
                        count = tmpList.Count + 1;
                        
                        if (tmpList.Count == 0)
                        {
                            first = last;
                            min = last;
                            max = last;
                            mean = last;
                        } else
                        {
                            first = tmpList.First();
                            min = tmpList.Min();
                            max = tmpList.Max();
                            mean = tmpList.Average();
                        }

                        tmpList.Add(last);

                        debug.verbose($"Last:  [{last}] ms");
                        debug.verbose($"Min:   [{min}] ms");
                        debug.verbose($"Mean:  [{mean:0.000}] ms");
                        debug.verbose($"Max:   [{max}] ms");
                        debug.verbose($"First: [{first}] ms");
                        debug.verbose($"Count: [{count}]");

                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = listView1.Items[i].Text;
                        lvi.SubItems.Add($"{last}");
                        lvi.SubItems.Add($"{min}");
                        lvi.SubItems.Add($"{mean:0.000}");
                        lvi.SubItems.Add($"{max}");
                        lvi.SubItems.Add($"{first}");
                        lvi.SubItems.Add($"{count}");

                        listView1.Items[i] = lvi;

                        chart1.Series[listView1.Items[i].Text].Points.Add(last);
                    }

                    continue;
                }
            }

            pinger.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            ips.Clear();
            chart1.Series.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var item in listView1.SelectedIndices)
            {
                listView1.Items[(int)item].Remove();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "Bright":
                    chart1.Palette = ChartColorPalette.Bright;
                    break;
                case "Bright Pastel":
                    chart1.Palette = ChartColorPalette.BrightPastel;
                    break;
                case "Grayscale":
                    chart1.Palette = ChartColorPalette.Grayscale;
                    break;
                case "Excel":
                    chart1.Palette = ChartColorPalette.Excel;
                    break;
                case "Light":
                    chart1.Palette = ChartColorPalette.Light;
                    break;
                case "Pastel":
                    chart1.Palette = ChartColorPalette.Pastel;
                    break;
                case "Earth Tones":
                    chart1.Palette = ChartColorPalette.EarthTones;
                    break;
                case "Semi Transparent":
                    chart1.Palette = ChartColorPalette.SemiTransparent;
                    break;
                case "Berry":
                    chart1.Palette = ChartColorPalette.Berry;
                    break;
                case "Chocolate":
                    chart1.Palette = ChartColorPalette.Chocolate;
                    break;
                case "Fire":
                    chart1.Palette = ChartColorPalette.Fire;
                    break;
                case "Sea Green":
                    chart1.Palette = ChartColorPalette.SeaGreen;
                    break;
                default:
                    break;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            foreach (var ser in chart1.Series)
            {
                ser.BorderWidth = trackBar2.Value;
            }
            label4.Text = $"{trackBar2.Value}\nThickness:";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp|TIFF Image|*.tif|GIF Image|*.gif";
            saveFileDialog.Title = "Save Chart Image";
            saveFileDialog.FileName = "icmp chart.png";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                debug.verbose($"Saving chart image as: [{saveFileDialog.FileName}] with extension: [{Path.GetExtension(saveFileDialog.FileName)}]");
                switch(Path.GetExtension(saveFileDialog.FileName).ToLower())
                {
                    case ".png":
                        chart1.SaveImage(saveFileDialog.FileName, ImageFormat.Png);
                        break;
                    case ".jpg":
                        chart1.SaveImage(saveFileDialog.FileName, ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        chart1.SaveImage(saveFileDialog.FileName, ImageFormat.Bmp);
                        break;
                    case ".tif":
                        chart1.SaveImage(saveFileDialog.FileName, ImageFormat.Tiff);
                        break;
                    case ".gif":
                        chart1.SaveImage(saveFileDialog.FileName, ImageFormat.Gif);
                        break;
                    default:
                        break;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV File|*.csv";
            saveFileDialog.Title = "Save CSV File";
            saveFileDialog.FileName = "icmp data.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, "");
                foreach (KeyValuePair<string, List<double>> kvp in ips)
                {
                    string addr = kvp.Key;
                    File.AppendAllText(saveFileDialog.FileName, $"{addr},");
                    foreach (double t in kvp.Value)
                    {
                        File.AppendAllText(saveFileDialog.FileName, $"{t},");
                    }
                    File.AppendAllText(saveFileDialog.FileName, Environment.NewLine);
                }
            }
        }
    }
}
