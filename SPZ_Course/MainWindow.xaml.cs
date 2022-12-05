using System;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Timers;

namespace SPZ_Course
{
    public partial class MainWindow : Window
    {
        Timer tmr = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ManualScan_Button_Click(object sender, RoutedEventArgs e)
        {
            NetworkScan();
        }

        private void Scan_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ScanRepetitionTextBox.Text != string.Empty)
            {
                tmr.Elapsed += (sender, args) => NetworkScan();
                tmr.AutoReset = true;
                tmr.Interval = int.Parse(ScanRepetitionTextBox.Text) * 1000;
                tmr.Start();
                StopBtn.IsEnabled = true;
                StartBtn.IsEnabled = false;
                ScanRepetitionTextBox.IsEnabled = false;
            }
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            tmr.Stop();
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
            ScanRepetitionTextBox.IsEnabled = true;
        }

        private void NetworkScan()
        {
            this.Dispatcher.Invoke(() =>
            {
                OutputTextBox.Text = "";
                StringBuilder bld = new();

                Process netUtility = new();
                netUtility.StartInfo.FileName = "arp.exe";
                netUtility.StartInfo.CreateNoWindow = true;
                netUtility.StartInfo.Arguments = "-a";
                netUtility.StartInfo.RedirectStandardOutput = true;
                netUtility.StartInfo.UseShellExecute = false;
                netUtility.StartInfo.RedirectStandardError = true;
                netUtility.Start();

                StreamReader streamReader = new(netUtility.StandardOutput.BaseStream, netUtility.StandardOutput.CurrentEncoding);

                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith("  "))
                    {
                        var Itms = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (Itms.Length == 3 && (int.Parse(Itms[0].Split('.')[3]) > 1) && (int.Parse(Itms[0].Split('.')[3]) < 255) && (int.Parse(Itms[0].Split('.')[0]) == 192))
                        {
                            try
                            {
                                bld.Append("IP: " + Itms[0] + " - " + Dns.GetHostEntry(Itms[0].Trim()).HostName + "\r\n");
                            }
                            catch (Exception e)
                            {
                                bld.Append(Itms[0] + " - " + e.Message + "\r\n");
                            }
                        }
                    }
                }

                streamReader.Close();

                OutputTextBox.Text += "Host: " + Dns.GetHostName() + "\r\n";
                OutputTextBox.Text += bld.ToString();
                OutputTextBox.Text += "\r\n\r\n";
                OutputTextBox.Text += $"Scan complete at: { DateTime.Now.ToString("h:mm:ss tt") }";
            });
        }
    }
}
