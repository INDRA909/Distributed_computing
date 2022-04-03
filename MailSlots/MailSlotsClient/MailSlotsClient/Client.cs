using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MailSlots
{
    public partial class frmMain : Form
    {
        private Int32 HandleMailSlot;
        private Int32 HandleMailSlotServ;
        bool isConnected = false;
        private bool _continue = true;
        private Thread t;
        public frmMain()
        {
            InitializeComponent();
            this.Text += "     " + Dns.GetHostName();   
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                DisconnectUser();
            }
            else
            {
                ConnectUser();               
            }
        }
        private void SendMsg(string msg)
        {
            uint BytesWritten = 0;
            byte[] buff = Encoding.Unicode.GetBytes(msg);
            DIS.Import.WriteFile(HandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);
            
        }
        void ConnectUser()
        {
            if (!isConnected)
            {
                try
                {                   

                    HandleMailSlot = DIS.Import.CreateFile(tbMailSlot.Text, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
                    if (HandleMailSlot != -1)
                    {
                        btnConnect.Enabled = true;
                        btnSend.Enabled = true;

                        string msg = $"!{Dns.GetHostName()}" + (textBox2.Text.Length == 0 ? "@" : $" >> @{textBox2.Text}@");
                        SendMsg(msg);

                        tbMailSlot.Enabled = false;
                        textBox2.Enabled = false;
                        btnConnect.Text = "Отключиться";
                        isConnected = true;
                    }
                    else
                        MessageBox.Show("Не удалось подключиться к мейлслоту");
                    string slot = (textBox2.Text.Length == 0 ? $"{Dns.GetHostName()}" : textBox2.Text);
                    HandleMailSlotServ = DIS.Import.CreateMailslot($"\\\\.\\mailslot\\{slot}", 0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);
                    _continue = true;
                    t = new Thread(ReceiveMessage);
                    t.Start();
                }
                catch
                {
                    MessageBox.Show("Не удалось подключиться к мейлслоту");
                }
            }
        }

        void DisconnectUser()
        {
            try
            {
                if (isConnected)
                {
                    _continue = false;
                    if (t != null)
                    {
                        t.Abort();
                    }
                    btnConnect.Enabled = true;
                    btnSend.Enabled = false;

                    string msg = $"*{Dns.GetHostName()}" + (textBox2.Text.Length == 0 ? "@" : $" >> @{textBox2.Text}@");
                    SendMsg(msg);

                    tbMailSlot.Enabled = true;
                    textBox2.Enabled = true;
                    btnConnect.Text = "Подключиться";
                    isConnected = false;    
                }
            }
            catch
            {
                MessageBox.Show("Не удалось отключится от мейлслота");
            }
        }
        private void ReceiveMessage()
        {
            string msg = "";            
            int MailslotSize = 0;       
            int lpNextSize = 0;         
            int MessageCount = 0;       
            uint realBytesReaded = 0;   
            while (_continue)
            {
                DIS.Import.GetMailslotInfo(HandleMailSlotServ, MailslotSize, ref lpNextSize, ref MessageCount, 0);
                if (MessageCount > 0)
                    for (int i = 0; i < MessageCount; i++)
                    {
                        byte[] buff = new byte[1024];                        
                        DIS.Import.FlushFileBuffers(HandleMailSlotServ);     
                        DIS.Import.ReadFile(HandleMailSlotServ, buff, 1024, ref realBytesReaded, 0);     
                        msg = Encoding.Unicode.GetString(buff);                

                        lbChat.Invoke((MethodInvoker)delegate
                        {
                            if (msg != "")
                                lbChat.Items.Add(msg);
                        });
                        Thread.Sleep(500);
                    }
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = $"{Dns.GetHostName()}" + (textBox2.Text.Length == 0 ? "@" : $" >> @{textBox2.Text}@") + " >> " + tbMessage.Text;
            SendMsg(msg);
            tbMessage.Text = String.Empty;              
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HandleMailSlotServ != -1)
                DIS.Import.CloseHandle(HandleMailSlotServ);
            DisconnectUser();
            DIS.Import.CloseHandle(HandleMailSlot);

        }

        private void tbMailSlot_TextChanged(object sender, EventArgs e)
        {

        }
    }
}