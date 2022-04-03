using PipesServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Pipes
{
    public partial class frmMain : Form
    {
        private Int32 PipeHandle;                                                      
        private string PipeName = "\\\\" + Dns.GetHostName() + "\\pipe\\ServerPipe";   
        private Thread t;                                                              
        private bool _continue = true;                                                 
        List<ServerUser> users = new List<ServerUser>();
        public frmMain()
        {
            InitializeComponent();
            PipeHandle = DIS.Import.CreateNamedPipe($"\\\\.\\pipe\\ServerPipe", DIS.Types.PIPE_ACCESS_DUPLEX, DIS.Types.PIPE_TYPE_BYTE | DIS.Types.PIPE_WAIT, DIS.Types.PIPE_UNLIMITED_INSTANCES, 0, 1024, DIS.Types.NMPWAIT_WAIT_FOREVER, (uint)0);
            this.Text += "    " + PipeName;       
            t = new Thread(ReceiveMessage);
            t.Start();
        }
        private void ReceiveMessage()
        {
            string msg = "";            
            uint realBytesReaded = 0;     
            while (_continue)
            {
                if (DIS.Import.ConnectNamedPipe(PipeHandle, 0))
                {
                    byte[] buff = new byte[1024];                                          
                    DIS.Import.FlushFileBuffers(PipeHandle);                               
                    DIS.Import.ReadFile(PipeHandle, buff, 1024, ref realBytesReaded, 0);   
                    msg = Encoding.Unicode.GetString(buff);
                    DIS.Import.DisconnectNamedPipe(PipeHandle);

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
                             msg= msg.Remove(msg.IndexOf('@'), 1);
                         }                                
                         SendMsg(msg);
                         rtbMessages.Invoke((MethodInvoker)delegate
                         {
                             rtbMessages.Text += "\n >> " + msg;
                         });
                        }
                                              
                    }                                       
                }
            }
        }
        private void SendMsg(string msg)
        {
           foreach (ServerUser user in users)
           {
               byte[] buff = new byte[1024];
               Int32 Pipe = DIS.Import.CreateFile($"\\\\.\\pipe\\{user.Name}", DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
               uint BytesWritten = 0;
               buff = Encoding.Unicode.GetBytes($"{msg}");
               DIS.Import.WriteFile(Pipe, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);
               DIS.Import.CloseHandle(Pipe);
           }
        }
        public void Connect(string name)
        {        
            users.Add(new ServerUser(){ Name = name });
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
            if (PipeHandle != -1)
                DIS.Import.CloseHandle(PipeHandle);     
        }
    }
}