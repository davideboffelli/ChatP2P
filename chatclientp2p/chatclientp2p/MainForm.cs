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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatclientp2p
{
    public partial class MainForm : Form
    {

        private readonly TcpClient serverConnection;
        private readonly string username;
        private NetworkStream serverStream;
        private TcpClient peerClient;
        private TcpListener peerListener;
        private NetworkStream peerStream;
        private Thread serverListenerThread;
        private Thread peerListenerThread;

        public MainForm(TcpClient client, string username)
        {
            InitializeComponent();
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            this.serverConnection = client;
            this.username = username;
            this.serverStream = client.GetStream();
        }


        private void StartServerListener()
        {
            serverListenerThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int read = serverStream.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer, 0, read);




                    if (msg.StartsWith("LIST|"))
                    {
                        UpdateUserList(msg.Substring(5).Split(','));
                    }
                    else if (msg.StartsWith("CHATREQ|"))
                    {
                        string fromUser = msg.Substring(8);
                        var result = MessageBox.Show($"{fromUser} vuole chattare con te", "Richiesta chat", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            StartPeerListener();
                            SendToServer($"CHATACC|{fromUser}|{((IPEndPoint)peerListener.LocalEndpoint).Port}");
                        }
                    }
                    else if (msg.StartsWith("CHATACC|"))
                    {
                        string[] parts = msg.Split('|');
                        string user = parts[1];
                        string ip = parts[2];
                        int port = int.Parse(parts[3]);



                        ConnectToPeer(ip, port);
                    }
                }
            });
            serverListenerThread.IsBackground = true;
            serverListenerThread.Start();
        }

        private void UpdateUserList(string[] users)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUserList(users)));
                return;
            }

            dataGridView1.Rows.Clear();
            foreach (var user in users)
            {
                if (user != username && !string.IsNullOrWhiteSpace(user))
                    dataGridView1.Rows.Add(user);
            }
        }

        private void StartPeerListener()
        {
            peerListener = new TcpListener(IPAddress.Any, 0);
            peerListener.Start();



            peerListenerThread = new Thread(() =>
            {
                peerClient = peerListener.AcceptTcpClient();
                peerStream = peerClient.GetStream();
                Invoke(new Action(() =>
                {
                    labelChatWith.Text = $"In chat con: {peerClient.Client.RemoteEndPoint}";
                }));



                ListenToPeer();
            });
            peerListenerThread.IsBackground = true;
            peerListenerThread.Start();
        }

        private void ConnectToPeer(string ip, int port)
        {


            peerClient = new TcpClient(ip, port);
            peerStream = peerClient.GetStream();
            Invoke(new Action(() =>
            {
                labelChatWith.Text = $"In chat con: {ip}";
                richTextBox1.Clear();
            }));

            ListenToPeer();
        }

        private void ListenToPeer()
        {
            Thread receiveThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = peerStream.Read(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        break;
                    }

                    if (bytesRead == 0) break;



                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    AppendChat($"Lui: {message}");

                    if (message == "ENDCHAT")
                    {
                        AppendChat("L'altro utente ha chiuso la chat.");
                        CloseCurrentChat();
                        break;
                    }

                }
                Invoke(new Action(() =>
                {
                    AppendChat("Chat terminata.");
                    labelChatWith.Text = "Nessuna chat attiva";
                }));

            });
            receiveThread.IsBackground = true;
            receiveThread.Start();


        }

        private void AppendChat(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendChat(text)));
                return;
            }

            richTextBox1.AppendText(text + Environment.NewLine);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string msg = textBox1.Text.Trim();


            if (!string.IsNullOrEmpty(msg) && peerStream != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                peerStream.Write(data, 0, data.Length);
                AppendChat($"Io: {msg}");
                textBox1.Clear();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Benvenuto {username}";
            StartServerListener();
        }




        private void SendToServer(string msg)
        {
            MessageBox.Show($"Invio messaggio al server: {msg}", "Debug - SendToServer");


            byte[] data = Encoding.UTF8.GetBytes(msg);
            serverStream.Write(data, 0, data.Length);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string selectedUser = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                if (labelChatWith.Text == $"In chat con: {selectedUser}")
                    return;


                if (peerClient != null)
                {
                    try
                    {
                        peerClient.Close();
                        peerStream?.Dispose();
                        peerStream = null;
                    }
                    catch { }
                }

                richTextBox1.Clear();
                labelChatWith.Text = $"In attesa di risposta da: {selectedUser}";
                SendToServer($"REQCHAT|{selectedUser}");
            }
        }

        private void buttonCloseChat_Click(object sender, EventArgs e)
        {
            CloseCurrentChat();
        }

        private void CloseCurrentChat()
        {
            if (peerClient != null)
            {
                byte[] endMsg = Encoding.UTF8.GetBytes("ENDCHAT");
                peerStream.Write(endMsg, 0, endMsg.Length);
                try
                {
                    peerStream?.Close();
                    peerClient?.Close();
                }
                catch { }

                peerStream = null;
                peerClient = null;
                labelChatWith.Text = "Nessuna chat attiva";
                richTextBox1.Clear();

                
                AppendChat("Hai chiuso la chat.");
               

            }
        }

    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public UserInfo(string username, string ipAddress, int port)
        {
            Username = username;
            IpAddress = ipAddress;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Username} ({IpAddress}:{Port})";
        }
    }


}
