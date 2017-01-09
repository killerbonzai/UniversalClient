using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace UniversalClient
{
    class ServerFacade
    {
        private string IP;
        private int Port;
        private StreamReader reader;
        private StreamWriter writer;
        private NetworkStream stream;
        private TcpClient server;
        public bool Run = true;

        public delegate void AddCompletedEventType(string msg);
        public event AddCompletedEventType AddCompletedEvent;

        public ServerFacade(string ip, int port)
        {
            IP = ip;
            Port = port;

            StartClient();
        }

        private void StartClient()
        {
            server = new TcpClient(IP, Port); // try to connect to server
            stream = server.GetStream(); // get stream from server
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
        }

        public void StartRecieveFromServerThread()
        {
            new System.Threading.Thread(() =>
            {
                while (Run)
                {
                    if (AddCompletedEvent != null)
                        AddCompletedEvent(ReceiveFromServer());
                }
            }).Start();
        }

        public void SendToServer(string text)
        {
            if (text == "exit")
            {
                text = "_exit";
            }
            writer.WriteLine(text);
            writer.Flush();
        }

        private string ReceiveFromServer()
        {
            try
            {
                return reader.ReadLine();
            }
            catch
            {
                return null;
            }
        }

        public void CloseConnection()
        {
            writer.WriteLine("exit");
            writer.Flush();
            Run = false;
            writer.Close();
            reader.Close();
            stream.Close();
            server.Close();
            server = null;
        }
    }
}
