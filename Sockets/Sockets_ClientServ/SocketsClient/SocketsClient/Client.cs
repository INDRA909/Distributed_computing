using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Sockets
{
    public partial class frmMain : Form
    {
        private bool isConnected = false;
        static string userName;
        static TcpClient client;
        static NetworkStream stream;
        private  int port = 5050;
        Thread receiveThread;
        public frmMain()
        {
            InitializeComponent();          
        }
        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    string message = builder.ToString();
                    LbChat.Invoke((MethodInvoker)delegate
                    {
                        if (message != "")
                            LbChat.Items.Add(message);
                    });
                }
                catch
                {
                    MessageBox.Show("Подключение прервано!");
                    Disconnect();
                }
            }
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                Connect();
            }
            else
            {
                if (client != null)
                    client.Close();
                if (stream != null)
                    stream.Close();              
                if (receiveThread != null)
                    receiveThread.Abort();
           

                btnSend.Enabled = false;
                btnConnect.Text = "Подключиться";
                tbIP.Enabled = true;
                tbLogin.Enabled = true;
                isConnected = false;
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMsg(tbMessage.Text);
            tbMessage.Text = string.Empty;
        }
        private void Connect()
        {
            try
            {
                client = new TcpClient();
                userName = tbLogin.Text;
                port= Convert.ToInt32(tbPort.Text);
                client.Connect(tbIP.Text, port);
                stream = client.GetStream();
                SendMsg(userName);
                btnSend.Enabled = true;
                btnConnect.Text = "Отключиться";
                tbIP.Enabled = false;
                tbLogin.Enabled = false;
                tbPort.Enabled = false;
                isConnected = true;

                receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         
        }
        private void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
        private void SendMsg(string msg)
        {
            byte[] buff = Encoding.Unicode.GetBytes(msg);
            stream.Write(buff, 0, buff.Length);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        private void tbIP_TextChanged(object sender, EventArgs e)
        {

        }
    }
}