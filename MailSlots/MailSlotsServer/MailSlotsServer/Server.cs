using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MailSlots
{
    public class ServerUser
    {
        public string Name { get; set; }
    }
    public partial class frmMain : Form
    {
        private int ClientHandleMailSlot;    
        private string MailSlotName = "\\\\" + Dns.GetHostName() + "\\mailslot\\ServerMailslot";  
        private Thread t;                       
        private bool _continue = true;
        List<ServerUser> users = new List<ServerUser>();
        public frmMain()
        {
            InitializeComponent();
            ClientHandleMailSlot = DIS.Import.CreateMailslot("\\\\.\\mailslot\\ServerMailslot", 0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);         
            this.Text += "     " + MailSlotName;
            Thread t = new Thread(ReceiveMessage);
            t.Start();
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
                DIS.Import.GetMailslotInfo(ClientHandleMailSlot, MailslotSize, ref lpNextSize, ref MessageCount, 0);

                if (MessageCount > 0)
                    for (int i = 0; i < MessageCount; i++)
                    {
                        byte[] buff = new byte[1024];
                        DIS.Import.FlushFileBuffers(ClientHandleMailSlot);
                        DIS.Import.ReadFile(ClientHandleMailSlot, buff, 1024, ref realBytesReaded, 0);
                        msg = Encoding.Unicode.GetString(buff);

                        if (msg[0] == '!')
                        {
                            Connect(msg.Substring((msg.IndexOf('@') - msg.LastIndexOf('@') == 0) ? msg.IndexOf('!') + 1 : msg.IndexOf('@') + 1,
                                                   (msg.IndexOf('@') - msg.LastIndexOf('@') == 0) ? msg.IndexOf('@') - msg.IndexOf('!') - 1 :
                                                                                                     msg.LastIndexOf('@') - msg.IndexOf('@') - 1));

                            msg = $"\n >> {msg.Substring(msg.IndexOf('!') + 1, msg.LastIndexOf('@') - msg.IndexOf('!') - 1)}  подключился к чату!";
                            if (msg.Contains('@')) msg = msg.Remove(msg.IndexOf('@'), 1);
                            rtbMessages.Invoke((MethodInvoker)delegate
                            {
                                rtbMessages.Text += msg;
                            });
                        }
                        else
                        if (msg[0] == '*')
                        {
                            Disconnect(msg.Substring((msg.IndexOf('@') - msg.LastIndexOf('@') == 0) ? msg.IndexOf('*') + 1 : msg.IndexOf('@') + 1,
                                                   (msg.IndexOf('@') - msg.LastIndexOf('@') == 0) ? msg.IndexOf('@') - msg.IndexOf('*') - 1 :
                                                                                                     msg.LastIndexOf('@') - msg.IndexOf('@') - 1));
                            msg = $"\n >> {msg.Substring(msg.IndexOf('*') + 1, msg.LastIndexOf('@') - msg.IndexOf('*') - 1)}  отключился от чата!";
                            if (msg.Contains('@')) msg = msg.Remove(msg.IndexOf('@'), 1);
                            rtbMessages.Invoke((MethodInvoker)delegate
                            {
                                rtbMessages.Text += msg;
                            });
                        }
                        else
                        {

                            if (msg != "")
                            {
                                msg = msg.Remove(msg.IndexOf('@'), 1);
                                if (msg.Contains('@'))
                                {
                                    msg = msg.Remove(msg.IndexOf('@'), 1);
                                }
                                SendMsg(msg);
                                rtbMessages.Invoke((MethodInvoker)delegate
                                {
                                    rtbMessages.Text += "\n >> " + msg;
                                });
                            }
                        }
                    }
                Thread.Sleep(500);
            }
        }
        private void SendMsg(string msg)
        {
            foreach (ServerUser user in users)
            {
                Int32 MailSlot = DIS.Import.CreateFile($"\\\\.\\mailslot\\{user.Name}", DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
                byte[] buff = new byte[1024];
                uint BytesWritten = 0;
                buff = Encoding.Unicode.GetBytes($"{msg}");        
                DIS.Import.WriteFile(MailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);
                DIS.Import.CloseHandle(MailSlot);
            }
        }
        public void Connect(string name)
        {
            users.Add(new ServerUser() { Name = name });
            SendMsg($"{name} подключился к чату!");
        }
        public void Disconnect(string name)
        {
            var user = users.FirstOrDefault(i => i.Name == name);
            if (user != null)
            {
                SendMsg($"{name} отключился от чата!");
                users.Remove(user);
            }

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;      

            if (t != null)
                t.Abort();
            if (ClientHandleMailSlot != -1)
                DIS.Import.CloseHandle(ClientHandleMailSlot);            
        }
    }
}