using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatclientp2p
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Inserisci un nome utente.");
                return;
            }

            try
            {
                TcpClient client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 3000);
                NetworkStream stream = client.GetStream();

                string loginMsg = $"LOGIN|{username}";
                byte[] data = Encoding.UTF8.GetBytes(loginMsg);
                await stream.WriteAsync(data, 0, data.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (response == "OK")
                {
                    this.Hide();
                    MainForm mainForm = new MainForm(client, username);
                    mainForm.Show();
                }
                else
                {
                    MessageBox.Show("Username già in uso o errore di login.");
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore di connessione al server: " + ex.Message);
            }
        }
    }
}
