using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace MacSpoofer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            string macAddress = GetMacAddress();
            textBox1.Text = macAddress;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            string newMacAddress = GenerateRandomMacAddress();
            if (SpoofMacAddress(newMacAddress))
            {
                MessageBox.Show("MAC Address changed successfully to " + newMacAddress);
                textBox1.Text = newMacAddress;
            }
            else
            {
                MessageBox.Show("Failed to change MAC Address. Run as administrator.");
            }
        }

        private string GetMacAddress()
        {
            var macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

            return macAddress ?? "No MAC Address found";
        }

        private string GenerateRandomMacAddress()
        {
            Random rand = new Random();
            byte[] macAddr = new byte[6];
            rand.NextBytes(macAddr);
            macAddr[0] = (byte)(macAddr[0] & (byte)254); 

            return string.Join(":", macAddr.Select(b => b.ToString("X2")));
        }

        private bool SpoofMacAddress(string newMacAddress)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("netsh", $"interface set interface \"Ethernet\" mac={newMacAddress.Replace(":", "")}");
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                Process.Start(psi)?.WaitForExit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RestartNetworkAdapter(NetworkInterface adapter)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", $"interface set interface \"{adapter.Name}\" admin=disable");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            Process.Start(psi)?.WaitForExit();

            psi = new ProcessStartInfo("netsh", $"interface set interface \"{adapter.Name}\" admin=enable");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            Process.Start(psi)?.WaitForExit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
