using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Web;

namespace Tanciky {
    public partial class ConnectForm : Form {
        Dictionary<string, string> servers = new Dictionary<string, string>();

        public ConnectForm() {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e) {
            Form1.isServer = true;
            Form1.serverName = textBox2.Text;
            Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            Form1.IP = textBox1.Text;
            Form1.isServer = false;
            Close();
        }

        private void ConnectForm_Load(object sender, EventArgs e) {
            foreach (NetworkInterface ni in 
                NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (UnicastIPAddressInformation ip in 
                    ni.GetIPProperties().UnicastAddresses) {
                    if (!ip.IsDnsEligible) {
                        if (ip.Address.AddressFamily == 
                            AddressFamily.InterNetwork) {
                            try {
                                TcpClient c = new TcpClient();
                                var result = 
                                    c.BeginConnect(ip.Address.ToString(), 
                                    Form1.port - 1, null, null);
                                var success = 
                                    result.AsyncWaitHandle.WaitOne(
                                        TimeSpan.FromSeconds(1));
                                if (!success) {
                                    continue;
                                }
                                c.EndConnect(result);
                                c.NoDelay = true;
                                c.SendTimeout = 999999999;
                                byte[] data = Encoding.ASCII.GetBytes("synam");
                                NetworkStream ns = c.GetStream();
                                Console.WriteLine("sending");
                                ns.Write(data, 0, data.Length);
                                Console.WriteLine("sent");
                                ns.Close();
                                c.Close();
                                TcpListener s = new TcpListener(IPAddress.Any, 
                                    Form1.port + 2);
                                s.Start(1);
                                Console.WriteLine("listening");
                                while (!s.Pending()) ;
                                Console.WriteLine(s.Pending());
                                c = s.AcceptTcpClient();
                                c.NoDelay = true;
                                c.ReceiveTimeout = 999999999;
                                ns = c.GetStream();
                                byte[] dataBytes = new byte[1];
                                string rdat;
                                int i;
                                string rec = "";
                                while ((i = ns.Read(dataBytes, 0, dataBytes.Length)) != 0) {
                                    rdat = Encoding.ASCII.
                                        GetString(dataBytes, 0, i);
                                    rec += rdat;
                                }
                                Console.WriteLine(rec);
                                c.Close();
                                s.Stop();
                                servers.Add(rec, ip.Address.ToString());
                                listBox1.Items.Add(servers.Last().Key);
                            } catch (SocketException ex) {
                                Console.WriteLine("An exception ocurred: {0}", 
                                    ex.ToString());
                            } finally {

                            }
                        }
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox1.SelectedIndex >= 0)
                textBox1.Text = servers[listBox1.SelectedItem.ToString()];
        }
    }
}
