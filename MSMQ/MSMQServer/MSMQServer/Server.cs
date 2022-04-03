using System;
using System.Collections.Generic;
using System.Messaging;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace MSMQ
{
    public class Client
    {
        public string Name;
    }
    public partial class frmMain : Form
    {
        private MessageQueue q = null;
        private Thread t = null;
        private bool _continue = true;
        private List<Client> clients = new List<Client>();

        public frmMain()
        {
            InitializeComponent();
            string path = Dns.GetHostName() + "\\private$\\ServerQueue";
            if (MessageQueue.Exists(path))
                q = new MessageQueue(path);
            else
                q = MessageQueue.Create(path);
            q.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });

            this.Text += "     " + q.Path;

            Thread t = new Thread(ReceiveMessage);
            t.Start();
        }
        private void ReceiveMessage()
        {
            if (q == null)
                return;
            System.Messaging.Message msg = null;
            while (_continue)
            {
                if (q.Peek() != null)
                {
                    msg = q.Receive(TimeSpan.FromSeconds(10.0));
                    if (!clients.Exists(client => client.Name == msg.Label)) Connect(msg.Label);
                    else
                    {
                        if (!MessageQueue.Exists($".\\private$\\{msg.Label}"))
                        {
                            Dissconnect(msg.Label);
                        }
                        else
                        {
                            SendMsg(msg);
                        }
                    }
                }
                Thread.Sleep(200);
            }
        }
        private void Connect(string Name)
        {
            clients.Add(new Client { Name = Name });
            MessageQueue messageQueue = new MessageQueue($".\\private$\\{Name}");
            rtbMessages.Invoke((MethodInvoker)delegate
            {
                rtbMessages.Text += $"\n{Name}: подключился к чату  --> for Server";
            });
            foreach (Client client in clients)
            {
                if (MessageQueue.Exists($".\\private$\\{client.Name}"))
                {
                    MessageQueue connectedQueue = new MessageQueue($".\\private$\\{client.Name}");
                    connectedQueue.Send($"\n подключился к чату", Name);
                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                        rtbMessages.Text += $"\n{Name}: подключился к чату  --> for {client.Name}";
                    });
                }

            }
        }
        private void Dissconnect(string Name)
        {
            clients.Remove(clients.Find(client => client.Name == Name));
            rtbMessages.Invoke((MethodInvoker)delegate
            {
                rtbMessages.Text += $"\n{Name}: отключился от чата  --> for Server";
            });
            foreach (Client client in clients)
            {
                if (MessageQueue.Exists($".\\private$\\{client.Name}"))
                {
                    MessageQueue dissconnectedQueue = new MessageQueue($".\\private$\\{client.Name}");
                    dissconnectedQueue.Send($"\n отключился от чата", Name);
                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                        rtbMessages.Text += $"\n{Name}: отключился от чата  --> for {client.Name}";
                    });
                }
            }
           

        }
        private void SendMsg(System.Messaging.Message msg)
        {
            rtbMessages.Invoke((MethodInvoker)delegate
            {
                if (msg != null)
                    rtbMessages.Text += $"\n{msg.Label} : {msg.Body} --> for Server";
            });
            foreach (Client client in clients)
            {
                if (MessageQueue.Exists($".\\private$\\{client.Name}"))
                {
                    MessageQueue messageQueue = new MessageQueue($".\\private$\\{client.Name}");
                    messageQueue.Send(msg.Body, msg.Label);
                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                        if (msg != null)
                            rtbMessages.Text += $"\n{msg.Label} : {msg.Body} --> for {client.Name} ";
                    });
                }
            }
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;
            if (t != null)
            {
                t.Abort();
            }
            foreach (Client client in clients)
            {
                MessageQueue messageQueue = new MessageQueue($".\\private$\\{client.Name}");
                if (messageQueue!=null)
                    MessageQueue.Delete(messageQueue.Path);
            }
            q.Close();
        }
    }
}