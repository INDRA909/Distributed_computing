using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Pipes
{
    public partial class frmMain : Form
    {
        private Int32 PipeHandle;  
        private Int32 PipeHandleServ;   
        bool isConnected = false;
        private bool _continue = true;
        private Thread t;

        public frmMain()
        {
            InitializeComponent();
            this.Text += "     " + Dns.GetHostName();              
        }
        private void ReceiveMessage()
        {
            string msg = "";           
            uint realBytesReaded = 0;                 
            while (_continue)
            {
                if (DIS.Import.ConnectNamedPipe(PipeHandleServ, 0))
                {
                    byte[] buff = new byte[1024];                                               
                    DIS.Import.FlushFileBuffers(PipeHandleServ);                                
                    DIS.Import.ReadFile(PipeHandleServ, buff, 1024, ref realBytesReaded, 0);    
                    msg = Encoding.Unicode.GetString(buff);                                                                                                                                   
                    lbChat.Invoke((MethodInvoker)delegate
                    {
                        lbChat.Items.Add(msg);
                    });
                    DIS.Import.DisconnectNamedPipe(PipeHandleServ);
                }
            }
        }
        void ConnectUser()
        {
            if (!isConnected)
            {
                string Pipe = (textBox2.Text.Length == 0 ? $"{Dns.GetHostName()}" : textBox2.Text);
                PipeHandleServ = DIS.Import.CreateNamedPipe($"\\\\.\\pipe\\{Pipe}", DIS.Types.PIPE_ACCESS_DUPLEX, DIS.Types.PIPE_TYPE_BYTE | DIS.Types.PIPE_WAIT, DIS.Types.PIPE_UNLIMITED_INSTANCES, 0, 1024, DIS.Types.NMPWAIT_WAIT_FOREVER, (uint)0);
                _continue = true;
                t = new Thread(ReceiveMessage);
                t.Start();
                
                string msg = $"!{Dns.GetHostName()}" + (textBox2.Text.Length == 0 ? "@" : $" >> @{textBox2.Text}@");
                SendMsg(msg);

                tbPipe.Enabled = false;
                textBox2.Enabled = false;
                button1.Text = "Отключиться";
                isConnected = true;
            }
        }

        void DisconnectUser()
        {
            if (isConnected)
            {   
                string msg = $"*{Dns.GetHostName()}" + (textBox2.Text.Length == 0 ? "@" : $" >> @{textBox2.Text}@");
                SendMsg(msg);

                tbPipe.Enabled = true;
                textBox2.Enabled = true;
                button1.Text = "Подключиться";
                isConnected = false;

                _continue = false;
                if (t != null)
                {
                    t.Abort();
                }
                if (PipeHandleServ != -1)
                    DIS.Import.CloseHandle(PipeHandleServ);
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {           
            string msg = $"{Dns.GetHostName()}" + (textBox2.Text.Length == 0 ? "@" : $" >> @{textBox2.Text}@") + " >> " + tbMessage.Text;
            SendMsg(msg);
            tbMessage.Text = String.Empty;
        }
        private void SendMsg(string msg)
        {
            PipeHandle = DIS.Import.CreateFile(tbPipe.Text, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
            uint BytesWritten = 0;         
            byte[] buff = Encoding.Unicode.GetBytes(msg);   
            DIS.Import.WriteFile(PipeHandle, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);
            DIS.Import.CloseHandle(PipeHandle);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                btnSend.Enabled = false;
                DisconnectUser();
            }
            else
            {
                btnSend.Enabled = true;
                ConnectUser();
            }
        }
       
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisconnectUser();               
        }
    }
}
