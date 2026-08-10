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


namespace chatserverP2p2._0
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
            listener = new TcpListener(IPAddress.Any, 3000);
            listener.Start();

            Log("Server avviato sulla porta 9000.");
            listenerThread = new Thread(ListenForClients) { IsBackground = true };
            listenerThread.Start();
        }

        private void BroadcastUserList()
        {
            string userList = string.Join(",", users.Keys);
            string message = "LIST|" + userList;
            byte[] data = Encoding.UTF8.GetBytes(message);

            lock (lockObj)
            {
                foreach (var user in users.Values)
                {
                    try
                    {
                        NetworkStream stream = user.Client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Log($"Errore inviando la lista a {user.Username}: {ex.Message}");
                    }
                }
            }
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
            string rawMsg = Encoding.UTF8.GetString(buffer, 0, byteCount);

            if (!rawMsg.StartsWith("LOGIN|"))
            {
                client.Close();
                return;
            }

            string username = rawMsg.Substring(6);
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

                    byte[] okMsg = Encoding.UTF8.GetBytes("OK");
                    stream.Write(okMsg, 0, okMsg.Length);

                    BroadcastUserList(); 
                }
                else
                {
                    byte[] errorMsg = Encoding.UTF8.GetBytes("ERROR");
                    stream.Write(errorMsg, 0, errorMsg.Length);
                    client.Close();
                    return;
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

                    if (message.StartsWith("REQCHAT|"))
                    {
                        string targetUser = message.Substring(8);
                        lock (lockObj)
                        {
                            if (users.ContainsKey(targetUser))
                            {
                                var targetClient = users[targetUser].Client;
                                var targetStream = targetClient.GetStream();

                              
                                byte[] chatReq = Encoding.UTF8.GetBytes("CHATREQ|" + username);
                                targetStream.Write(chatReq, 0, chatReq.Length);
                                Log($"Richiesta chat inviata da {username} a {targetUser}");
                            }
                        }
                    }
                    else if (message.StartsWith("CHATACC|"))
                    {
                      
                        string[] parts = message.Split('|');
                        if (parts.Length == 3)
                        {
                            string requester = parts[1];
                            string portStr = parts[2];

                            lock (lockObj)
                            {
                                if (users.ContainsKey(requester))
                                {
                                    var requesterClient = users[requester].Client;
                                    var requesterStream = requesterClient.GetStream();

                                    
                                    string userIp = users[username].IPAddress;

                            
                                    string response = $"CHATACC|{username}|{userIp}|{portStr}";
                                    byte[] data = Encoding.UTF8.GetBytes(response);
                                    requesterStream.Write(data, 0, data.Length);

                                    Log($"Connessione peer inviata da {username} a {requester} su IP {ip}:{portStr}");
                                }
                            }
                        }
                    }

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
                        BroadcastUserList();
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
