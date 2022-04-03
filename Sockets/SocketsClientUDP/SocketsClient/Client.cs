using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sockets
{
    public partial class frmMain : Form
    {
        private bool isConnected = false;
        static string userName;
        static UdpClient client;
        private  int LocalPort = 1010;
        private int RemotePort = 1010;

        const int receiveTimeout = 20;
        const string HOST = "235.5.5.1";
        IPAddress groupAdrdress;
        public frmMain()
        {
            InitializeComponent();
            groupAdrdress = IPAddress.Parse(HOST);
            btnConnect.Enabled = false;
            btnSend.Enabled = false;
            tbIP.Text = HOST;
            tbPort.Text = LocalPort.ToString();
        }
     
        private void ReceiveMessage()
        {
            try
            {
                while (isConnected)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);
                    LbChat.Invoke((MethodInvoker)delegate
                    {
                        LbChat.Items.Add(message);
                    });
                }
            }
            catch (ObjectDisposedException)
            {
                if (!isConnected)
                    return;
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                Dissconnect();
            }
        }
        private void Dissconnect()
        {
            SendMsg($"{userName} отключился от чата");
            client.DropMulticastGroup(groupAdrdress);
            isConnected = false;
            client.Close();

            btnSend.Enabled = false;
            btnConnect.Text = "Подключиться";
            tbIP.Enabled = true;
            tbLogin.Enabled = true;
            isConnected = false;
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", userName, tbMessage.Text);
                SendMsg(message);
                tbMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Connect()
        {
            try
            {
                userName = tbLogin.Text;
                //LocalPort = Convert.ToInt32(tbPort.Text);
                //groupAdrdress = IPAddress.Parse(tbIP.Text);
                client = new UdpClient(LocalPort);
                client.JoinMulticastGroup(groupAdrdress, receiveTimeout);

                isConnected= true;
                btnSend.Enabled = true;
                btnConnect.Text = "Отключиться";
                tbIP.Enabled = false;
                tbLogin.Enabled = false;
                tbPort.Enabled = false;
                isConnected = true;
                Task recieveMessagesTask = new Task(ReceiveMessage);
                recieveMessagesTask.Start();

                SendMsg($"{userName} присоединился к чату");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         
        }
        private void SendMsg(string msg)
        {
            byte[] buff = Encoding.Unicode.GetBytes(msg);
            client.Send(buff, buff.Length, HOST, RemotePort);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isConnected) Dissconnect();
        }

        private void tbLogin_TextChanged(object sender, EventArgs e)
        {
            if (tbLogin.TextLength != 0) btnConnect.Enabled = true;
            else btnConnect.Enabled = false;
        }
    }
}