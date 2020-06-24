using GameLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace Tanciky {
    public partial class Form1 : Form {
        public static string scene = "game";
        private Stopwatch sw = new Stopwatch();
        public Game game = new Game();
        public static List<Keys> press = new List<Keys>();
        public static int nextPlayerID = 1;

        static ConnectForm conForm = new ConnectForm();
        public static int port = 9876;

        public static bool isServer;
        public static string serverName;
        public static string IP;
        List<Tuple<IPEndPoint, int>> clients =
            new List<Tuple<IPEndPoint, int>>();

        TcpListener s = null;
        TcpClient c;

        public Form1() {
            conForm.ShowDialog();
            InitializeComponent();
            Application.Idle += AppIdle;
            DoubleBuffered = true;
            if (isServer) {
                s = new TcpListener(IPAddress.Any, port - 1);
                s.Start();
            } else {
                TryConnectToServer();
            }
        }

        private void TryConnectToServer() {
            if (MessageBox.Show("Connect " +
                    (IP == "" || IP == null ? "127.0.0.1" : IP) + "?",
                    "",
                    MessageBoxButtons.OKCancel) == DialogResult.OK) {
                s = new TcpListener(IPAddress.Any, port + 1);
                s.Start(1);
                LoginToServer();
            } else {
                Application.Exit();
            }
        }

        private void AppIdle(object sender, EventArgs e) {
            while (IsApplicationIdle()) {
                try {
                    float scalex = 600f / pictureBox1.Width;
                    float scaley = 600f / pictureBox1.Height;
                    float scale = Math.Max(scalex, scaley);
                    Game.mousepos = new Vec(pictureBox1.PointToClient(MousePosition));
                    if (scalex > scaley)
                        Game.mousepos.Y -= (pictureBox1.Height - pictureBox1.Width) / 2;
                    else if (scaley > scalex)
                        Game.mousepos.X -= (pictureBox1.Width - pictureBox1.Height) / 2;
                    Game.mousepos *= scale;
                    if (isServer) {
                        Game.Dt = sw.ElapsedMilliseconds * 0.001f;
                        sw.Restart();
                        game.Update(scene);
                        pictureBox1.Image = game.Render(scene);
                        SendInfoToClients();
                        ServerRecieve();
                    } else {
                        GameData recieved = RecieveFromServer();
                        if (recieved != null) {
                            pictureBox1.Image = game.RenderData(recieved);
                            SendToServer();
                        }
                    }
                } catch (Exception ex) {
                    DialogResult r = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.AbortRetryIgnore,
                        MessageBoxIcon.Error);
                    if (r == DialogResult.Abort) {
                        Application.Exit();
                    }
                }
            }
        }
        
        void ServerRecieve() {
            NetworkStream nws;
            try {
                if (s.Pending()) {
                    TcpClient tc = s.AcceptTcpClient();
                    tc.NoDelay = true;
                    tc.ReceiveTimeout = 999999999;
                    nws = tc.GetStream();
                    byte[] dataBytes = new byte[1];
                    string data;
                    int i;
                    string rec = "";
                    while ((i = nws.Read(dataBytes, 0, dataBytes.Length)) != 0) {
                        data = Encoding.ASCII.GetString(dataBytes, 0, i);
                        rec += data.ToString();
                    }
                    string[] recs = rec.Split(' ');
                    if (rec == "synam") {
                        try {
                            TcpClient tcpcl = new TcpClient(
                                ((IPEndPoint)tc.Client.LocalEndPoint).
                                Address.ToString(), 
                                port + 2);
                            tcpcl.SendTimeout = 999999999;
                            NetworkStream ns = tcpcl.GetStream();
                            byte[] dat = Encoding.ASCII.GetBytes(serverName);
                            ns.Write(dat, 0, dat.Length);
                            ns.Close();
                        } catch (SocketException e) {
                            Console.WriteLine("An exception ocurred: {0}", e.ToString());
                        } finally {

                        }
                    } else if (recs.Length > 0 && recs[0] == "newClient") {
                        clients.Add(new Tuple<IPEndPoint, int>(
                            (IPEndPoint)tc.Client.LocalEndPoint,
                            nextPlayerID++
                        ));
                        game.players.Add(new ClientPl(1, Color.Red,
                            clients.Last().Item2));
                        game.players.Last().Init(game);
                    } else {
                        IPEndPoint ip = (IPEndPoint)tc.Client.LocalEndPoint;
                        var q = from cl in clients
                                where (cl.Item1.Address.ToString() ==
                                    ip.Address.ToString())
                                select cl;
                        if (q.Any()) {
                            int clID = q.First().Item2;
                            game.SetControlData(clID, 
                                JsonConvert.DeserializeObject<ControlData>(rec));
                        }
                    }
                    nws.Close();
                    tc.Close();
                }
            } catch (SocketException) {
                s.Stop();
                MessageBox.Show("An exception ocurred!");
            } finally {

            }
        }

        void SendInfoToClients() {
            string msg = "";
            List<int> disconnect = new List<int>();
            foreach (var tcpc in clients) {
                try {
                    GameData gdata = new GameData();
                    gdata.track = (from p in game.players
                                   where (p.ID == tcpc.Item2)
                                   select p).First();
                    Vec ra = gdata.track.pos -
                        new Vec(320, 320) * gdata.track.zoom;
                    Vec rb = gdata.track.pos +
                         new Vec(320, 320) * gdata.track.zoom;
                    foreach (Player pl in game.players) {
                        if (pl.pos.InRect(ra, rb) &&
                            !(pl.Invis > 0 && pl.InvisTime <= 0))
                            gdata.pls.Add(pl);
                    }
                    foreach (Bullet b in game.bullets) {
                        if (b.pos.InRect(ra, rb))
                            gdata.bs.Add(b);
                    }
                    foreach (Shape s in game.shapes) {
                        if (s.pos.InRect(ra, rb))
                            gdata.ss.Add(s);
                    }
                    msg = JsonConvert.SerializeObject(gdata);
                    byte[] data = Encoding.ASCII.GetBytes(msg);
                    TcpClient tcpcl = new TcpClient(
                            tcpc.Item1.Address.ToString(), port + 1);
                    NetworkStream ns = tcpcl.GetStream();
                    ns.Write(data, 0, data.Length);
                    ns.Close();
                    tcpcl.Close();
                } catch (SocketException) {
                    disconnect.Add(tcpc.Item2);
                } finally {

                }
            }
            for (int i = 0; i < disconnect.Count; i++) {
                for (int j = i; j < disconnect.Count; j++)
                    if (disconnect[i] < disconnect[j])
                        disconnect[j]--;
                DisconnectClient(disconnect[i]);
            }
        }

        void DisconnectClient(int id) {
            for (int i = 0; i < clients.Count; i++) {
                if (clients[i].Item2 == id) {
                    for (int j = 0; j < game.players.Count; j++) {
                        if (game.players[j].ID == id) {
                            game.players.RemoveAt(j);
                            break;
                        }
                    }
                    clients.RemoveAt(i);
                    return;
                }
            }
        }

        void LoginToServer() {
            string msg = "newClient";
            try {
                c = new TcpClient((IP == "" || IP == null || IP == "localhost" ?
                    "127.0.0.1" : IP), port - 1);
                c.NoDelay = true;
                c.SendTimeout = 999999999;
                byte[] data = Encoding.ASCII.GetBytes(msg);
                NetworkStream ns = c.GetStream();
                ns.Write(data, 0, data.Length);
                ns.Close();
            } catch (SocketException e) {
                Console.WriteLine("An exception ocurred: {0}", e.ToString());
            } finally {

            }
        }

        void SendToServer() {
            ControlData cdata = new ControlData();
            cdata.keys[0] = press.Contains(Keys.W) || press.Contains(Keys.Up);
            cdata.keys[1] = press.Contains(Keys.A) || press.Contains(Keys.Left);
            cdata.keys[2] = press.Contains(Keys.D) || press.Contains(Keys.Right);
            cdata.keys[3] = press.Contains(Keys.S) || press.Contains(Keys.Down);
            cdata.keys[4] = press.Contains(Keys.E);
            cdata.keys[5] = press.Contains(Keys.C);
            cdata.keys[6] = press.Contains(Keys.D1);
            cdata.keys[7] = press.Contains(Keys.D2);
            cdata.keys[8] = press.Contains(Keys.D3);
            cdata.keys[9] = press.Contains(Keys.D4);
            cdata.keys[10] = press.Contains(Keys.D5);
            cdata.keys[11] = press.Contains(Keys.D6);
            cdata.keys[12] = press.Contains(Keys.D7);
            cdata.keys[13] = press.Contains(Keys.D8);
            cdata.keys[14] = press.Contains(Keys.V);
            cdata.keys[15] = press.Contains(Keys.B);
            cdata.keys[16] = press.Contains(Keys.N);
            cdata.keys[17] = press.Contains(Keys.F);
            cdata.keys[18] = press.Contains(Keys.G);
            cdata.keys[19] = press.Contains(Keys.J);
            cdata.mousep = Game.mousepos;
            cdata.mouseb[0] = Game.mousedown;
            cdata.mouseb[1] = Game.rmousedown;
            string msg = JsonConvert.SerializeObject(cdata);
            try {
                c = new TcpClient((IP == "" || IP == null || IP == "localhost" ?
                    "127.0.0.1" : IP), port - 1);
                c.NoDelay = true;
                c.SendTimeout = 999999999;
                byte[] data = Encoding.ASCII.GetBytes(msg);
                NetworkStream ns = c.GetStream();
                ns.Write(data, 0, data.Length);
                ns.Close();
            } catch (SocketException e) {
                Console.WriteLine("An exception ocurred: {0}", e.ToString());
            } finally {

            }
        }

        GameData RecieveFromServer() {
            GameData gd = new GameData();
            try {
                if (c == null) {
                    return null;
                }
                if (!s.Pending())
                    return null;
                c = s.AcceptTcpClient();
                c.NoDelay = true;
                c.ReceiveTimeout = 999999999;
                NetworkStream ns = c.GetStream();
                byte[] dataBytes = new byte[4];
                string data;
                int i;
                string rec = "";
                while ((i = ns.Read(dataBytes, 0, dataBytes.Length)) != 0) {
                    data = Encoding.ASCII.GetString(dataBytes, 0, i);
                    rec += data.ToString();
                }
                ns.Close();
                gd = JsonConvert.DeserializeObject<GameData>(rec);
            } catch (SocketException) {
                c.Close();
                s.Stop();
                MessageBox.Show("An exception ocurred!");
                return null;
            }
            return gd;
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        bool IsApplicationIdle() {
            return PeekMessage(out NativeMessage result,
                IntPtr.Zero, 0u, 0u, 0u) == 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                Game.mousedown = true;
            if (e.Button == MouseButtons.Right)
                Game.rmousedown = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                Game.mousedown = false;
            if (e.Button == MouseButtons.Right)
                Game.rmousedown = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if (!press.Contains(e.KeyCode))
                press.Add(e.KeyCode);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e) {
            if (press.Contains(e.KeyCode))
                press.Remove(e.KeyCode);
        }

        private void Form1_Leave(object sender, EventArgs e) {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            /*foreach (var tcpc in clients) {
                try {
                    tcpc.Item1.Close();
                } catch { }
            }*/
            try {
                c.Close();
            } catch { }
            try {
                s.Stop();
            } catch { }
        }
    }

    [Serializable]
    public class GameData {
        public List<Player> pls { get; set; } = new List<Player>();
        public List<Bullet> bs { get; set; } = new List<Bullet>();
        public List<Shape> ss { get; set; } = new List<Shape>();
        public Player track { get; set; }
        public string msg { get; set; } = "";
    }

    [Serializable]
    public class ControlData { /*
        public bool UA { get; set; }
        public bool LA { get; set; }
        public bool RA { get; set; }
        public bool DA { get; set; }
        public bool E { get; set; }
        public bool C { get; set; }
        public bool U1 { get; set; }
        public bool U2 { get; set; }
        public bool U3 { get; set; }
        public bool U4 { get; set; }
        public bool U5 { get; set; }
        public bool U6 { get; set; }
        public bool U7 { get; set; }
        public bool U8 { get; set; }
        public bool V { get; set; }
        public bool B { get; set; }
        public bool N { get; set; }
        public bool F { get; set; }
        public bool G { get; set; }
        public bool J { get; set; }*/
        //public bool[] keys { get; set; } = new bool[20];
        public Keydat keys { get; set; } = new Keydat();
        public Vec mousep { get; set; } = new Vec();
        public bool[] mouseb { get; set; } = new bool[2];
    }

    [Serializable]
    public class Keydat {
        public byte[] dat = new byte[3];

        public bool this[int i] {
            get {
                if (i < 8) {
                    return (dat[0] & (1 << i)) != 0;
                } else if (i < 16) {
                    return (dat[1] & (1 << i % 8)) != 0;
                }
                return (dat[2] & (1 << i % 8)) != 0;
            }
            set {
                if (i < 8) {
                    if (value) {
                        dat[0] |= (byte)(1 << i);
                    } else {
                        dat[0] &= (byte)(~(1 << i));
                    }
                } else if (i < 16) {
                    if (value) {
                        dat[1] |= (byte)(1 << i % 8);
                    } else {
                        dat[1] &= (byte)(~(1 << i % 8));
                    }
                } else {
                    if (value) {
                        dat[2] |= (byte)(1 << i % 8);
                    } else {
                        dat[2] &= (byte)(~(1 << i % 8));
                    }
                }
            }
        }
    }

    static class Serialization {
        public static void SerializeObj(object o) {

        }
    }
}
