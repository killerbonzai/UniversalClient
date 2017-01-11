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
        private Timer connecting = new Timer(1000);
        private int connectionCount = 0;
        private string ip;
        private string port;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerConnect()
        {
            int _port;
            if (!int.TryParse(port, out _port))
            {
                // failed
                return;
            }

            try
            {
                connecting.Stop();
                WriteToChat("<<Trying to connect to the server... attempt #" + (connectionCount + 1).ToString() + ">>");
                server = new ServerFacade(ip, _port);
                server.StartClient();
                server.AddCompletedEvent += getFromServer;
                server.StartRecieveFromServerThread();
                connected = true;
                connectionCount = 0;
                return;
            }
            catch (Exception)
            {
                connecting.Start();
            }
        }

        private void Connecting_Tick(object sender, EventArgs e)
        {
            if (connectionCount > 4) // if we reach 5: stop
            {
                connecting.Stop();
                connectionCount = 0;
                WriteToChat("<<Connection failed!>>");
                try
                {
                    // write to chat -- threadpool
                    this.Dispatcher.Invoke(new Action(() => // invoke ui dispatcher
                    {
                        btnConnect.IsEnabled = true;
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("A handled exception just occurred: " + ex.Message, "Write to chat", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                return;
            }
            ServerConnect(); // THEN try to connect
            connectionCount++; // increase number in case we failed
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
                // write to chat -- threadpool
                this.Dispatcher.Invoke(new Action(() => // invoke ui dispatcher
                {
                    txtbChat.Text += message + "\n"; // add text
                    scrollviewer.ScrollToEnd(); // scroll to end
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Write to chat", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseConnection()
        {
            try
            {
                if (connected)
                {
                    server.CloseConnection();
                    WriteToChat("<<Connection closed!>>");
                }
                else
                {
                    WriteToChat("<<Not connected!>>");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Closing connection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                connected = false;
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            btnConnect.IsEnabled = false;
            ip = txtIp.Text;
            port = txtPort.Text;
            
            connecting.Start();
            connecting.Elapsed += Connecting_Tick;
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
                WriteToChat("<<Not connected!>>");
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
