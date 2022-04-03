using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Sockets
{

    public class ClientObject
    {
        frmMain form2;
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server;
        Random random= new Random();
        public ClientObject(TcpClient tcpClient, ServerObject serverObject,frmMain myForm)
        {
            Id = Guid.NewGuid().ToString()+random.Next(1,25);
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            form2 = myForm;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                string message = GetMessage();
                userName = message;
                message =$"{userName}  присоединился к чату!";
               
                server.BroadcastMessage(message, this.Id);
                form2.Invoke((MethodInvoker)delegate
                          {
                              form2.RichTxt += $"\n {message}";
                          });
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);                        
                        server.BroadcastMessage(message, this.Id);
                        form2.Invoke((MethodInvoker)delegate
                        {
                            form2.RichTxt += $"\n {message}";
                        });
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        form2.Invoke((MethodInvoker)delegate
                        {
                            form2.RichTxt += $"\n {message}";
                        });
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);
                Close();
            }
        }
        private string GetMessage()
        {
            byte[] data = new byte[64]; 
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);
            return builder.ToString();
        }
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
    public class ServerObject
    {
        static TcpListener tcpListener; 
        List<ClientObject> clients = new List<ClientObject>();
        frmMain form2;
        public ServerObject(frmMain myForm)
        {
            this.form2 = myForm;
        }
        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 5050);
                tcpListener.Start();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this,form2);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                //if (clients[i].Id != id)
                //{
                    clients[i].Stream.Write(data, 0, data.Length);
                //}
             }
        }
        protected internal void Disconnect()
        {
            tcpListener.Stop(); 

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); 
            }
            Environment.Exit(0); 
        }
    }
    public partial class frmMain : Form
    {
        public string RichTxt
        {
            get { return rtbMessages.Text; }
            set { rtbMessages.Text = value; }
        }
        static ServerObject server; 
        static Thread listenThread;
        private IPAddress IP;
        private int port = 5050;
        public frmMain()
        {
            try
            {
                InitializeComponent();
            
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IP = hostEntry.AddressList[0];
                foreach (IPAddress address in hostEntry.AddressList)
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        IP = address;
                        break;
                    }
                this.Text += "     " + IP.ToString() + " " + port.ToString();
                Start();
            }
            catch
            {

            }
        }
        public void Start()
        {
            
            try
            {
                server = new ServerObject(this);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                server.Disconnect();
            }
        }
    }
} 