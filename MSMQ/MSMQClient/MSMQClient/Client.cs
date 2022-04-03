using System;
using System.Messaging;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace MSMQ
{
    public partial class frmMain : Form
    {
        private MessageQueue serverQueue = null;
        private MessageQueue clientQueue = null;
        private bool isConnected = false;
        private bool _continue = false;
        Thread t;
        public frmMain()
        {
            InitializeComponent();
            btnSend.Enabled = false;
            btnConnect.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {              
                if (MessageQueue.Exists(tbPath.Text))
                {
                    Listen();

                    serverQueue = new MessageQueue(tbPath.Text);
                    serverQueue.Send("",tbLogin.Text);

                    btnSend.Enabled = true;
                    btnConnect.Text="Отключиться";
                    isConnected = true;
                    tbLogin.Enabled = false;
                    tbPath.Enabled = false;                   
                }
                else
                    MessageBox.Show("Указан неверный путь к очереди, либо очередь не существует");
            }
            else
            {
                if (t != null) t.Abort();
                _continue = false;
                if (clientQueue != null) MessageQueue.Delete(clientQueue.Path);
                
                serverQueue.Send("", tbLogin.Text);
        
                isConnected = false;
                tbLogin.Enabled = true;
                tbPath.Enabled = true;
                btnSend.Enabled=false;
                btnConnect.Text = "Подключится";
            }
        }
        private void Listen()
        {
            string path =$"{Dns.GetHostName()}\\private$\\{tbLogin.Text}";    

            if (MessageQueue.Exists(path))
                clientQueue = new MessageQueue(path);
            else
                clientQueue = MessageQueue.Create(path);
            clientQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });

            this.Text += "     " + clientQueue.Path;

            t = new Thread(ReceiveMessage);
            t.Start();
        }
        private void ReceiveMessage()
        {
            try
            {
                if (clientQueue == null)
                    return;
                System.Messaging.Message msg = null;
                _continue = true;
                while (_continue)
                {
                    if (clientQueue.Peek() != null)
                        msg = clientQueue.Receive(TimeSpan.FromSeconds(10.0));
                    listBox1.Invoke((MethodInvoker)delegate
                    {
                        if (msg != null)
                            listBox1.Items.Add($"\n {msg.Label} : {msg.Body}");
                    });
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            serverQueue.Send(tbMessage.Text, tbLogin.Text);          
        }
        private void tbLogin_TextChanged(object sender, EventArgs e)
        {
            if (tbLogin.TextLength != 0) btnConnect.Enabled = true;
            else btnConnect.Enabled = false;
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;      
            if (t != null)  t.Abort();                      
            if (clientQueue != null) MessageQueue.Delete(clientQueue.Path);    
            serverQueue.Send("", tbLogin.Text);
        }

    }
}