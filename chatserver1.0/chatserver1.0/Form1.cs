using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace chatserver1._0
{
    public partial class Form1 : Form
    {
        private TcpListener listener;
        private Thread listenerT;
        private Thread listenerThread;
        private readonly Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();
        private readonly object lockObj = new object();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("Username", "Username");
            dataGridView1.Columns.Add("IPAddress", "IP Address");
            dataGridView1.Columns.Add("Port", "Port");
            StartServer();
        }

        private void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 3001);
            listener.Start();

            Log("Server avviato sulla porta 9000.");
            listenerThread = new Thread(ListenForClients) { IsBackground = true };
            listenerThread.Start();
        }


        private async void ListenForClients()
        {
            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (SocketException ex)
                {
                    Log($"Errore di socket: {ex.Message}");
                }
            }
        }
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int byteCount = stream.Read(buffer, 0, buffer.Length);
            string username = Encoding.UTF8.GetString(buffer, 0, byteCount);

            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            string ip = remoteEndPoint?.Address.ToString();
            int port = remoteEndPoint?.Port ?? 0;

            lock (lockObj)
            {
                if (!users.ContainsKey(username))
                {
                    users.Add(username, new UserInfo(username, ip, port, client));
                    UpdateUserGrid();
                    Log($"{username} connesso da {ip}:{port}.");
                }
            }

            try
            {
                while (true)
                {
                    byteCount = stream.Read(buffer, 0, buffer.Length);
                    if (byteCount == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Log($"Messaggio da {username}: {message}");
                }
            }
            catch
            {
                Log($"{username} disconnesso inaspettatamente.");
            }
            finally
            {
                lock (lockObj)
                {
                    if (users.ContainsKey(username))
                    {
                        users.Remove(username);
                        UpdateUserGrid();
                    }
                }
                client.Close();
            }
        }

        private void UpdateUserGrid()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUserGrid));
                return;
            }

            dataGridView1.Rows.Clear();
            foreach (var user in users.Values)
            {
                dataGridView1.Rows.Add(user.Username, user.IPAddress, user.Port);
            }
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }
            listBox1.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public TcpClient Client { get; set; }

        public UserInfo(string username, string ipAddress, int port, TcpClient client)
        {
            Username = username;
            IPAddress = ipAddress;
            Port = port;
            Client = client;
        }
    }
}