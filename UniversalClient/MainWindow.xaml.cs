using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UniversalClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServerFacade server;
        private bool connected = false;
        private Timer connecting = new Timer();
        private int connectionCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerConnect(string ip, string port)
        {
            int _port;
            if (!int.TryParse(port, out _port))
            {
                // failed
                return;
            }
            WriteToChat("Trying to connect to the server... attempt #" + (connectionCount + 1).ToString());
            server = new ServerFacade(ip, _port);
            server.AddCompletedEvent += getFromServer;
            server.StartRecieveFromServerThread();
            connected = true;
            connecting.Stop();
            connectionCount = 0;
            return;
        }

        private void Connecting_Tick(object sender, EventArgs e)
        {
            if (connectionCount == 5)
            {
                connecting.Stop();
                connectionCount = 0;
                WriteToChat("Connection failed!");
                return;
            }
            ServerConnect(txtIp.Text, txtPort.Text); // THEN try to connect
            connectionCount++;
        }

        private void getFromServer(string message)
        {
            if (message != null)
            {
                WriteToChat(message);
            }
        }

        private void WriteToChat(string message)
        {
            try
            {
                // write to chat -- thread
                this.Dispatcher.Invoke(new Action(() => { txtbChat.Text += message + "\n"; }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseConnection()
        {
            try
            {
                if (connected)
                {
                    server.CloseConnection();
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {
                connected = false;
            }
            WriteToChat("Connection closed!");
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            connecting.Interval = 1000;
            connecting.Elapsed += Connecting_Tick;
            connecting.Start();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                server.SendToServer(txtSend.Text);
                txtSend.Clear();
                txtSend.Focus();
            }
            else
            {
                WriteToChat("Not connected to a server...");
            }
        }

        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btnSend_Click(sender, e);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseConnection();
        }
    }
}
