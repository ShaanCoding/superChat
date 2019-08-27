using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace superChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket;
        EndPoint epLocal, epRemote;
        byte[] buffer;
        bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();
            userIPShow.Text += getLocalIp();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected == false)
            {
                //Grab information
                string localIp = getLocalIp();
                string remoteIp = friendsIPText.Text;
                int userPort = Convert.ToInt32(clientPort.Text);
                int friendPort = Convert.ToInt32(friendsPort.Text);

                //Setup socket
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                epLocal = new IPEndPoint(IPAddress.Parse(localIp), userPort);
                socket.Bind(epLocal);
                //Connects to remote ip
                epRemote = new IPEndPoint(IPAddress.Parse(remoteIp), friendPort);
                socket.Connect(epRemote);
                //listen to specific port
                buffer = new byte[1500];
                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                //Updates connection flags
                if (isConnected == true)
                {
                    isConnected = false;
                }
                else if (isConnected == false)
                {
                    isConnected = true;
                }
                MessageBox.Show("Connected");
            }
            else if(isConnected == true)
            {
                MessageBox.Show("Error Already Connected");
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            //Sending message function
            if(clientMessage.Text != "")
            {
                //Convert string message to byte
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                byte[] sendingMessage = new byte[1500];
                sendingMessage = aEncoding.GetBytes(userName.Text + ": " + clientMessage.Text);
                //Sending the encoded message
                socket.Send(sendingMessage);
                //Adding to the listbox
                chatBox.Items.Add("Me: " + clientMessage.Text);
                clientMessage.Text = "";
            }
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            //Async recursive function checking if any messages have been sent to the client
            try
            {
                byte[] recievedData = new byte[1500];
                recievedData = (byte[])aResult.AsyncState;
                //Convert byte[] to string
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                string recievedMessage = aEncoding.GetString(recievedData);
                int i = recievedMessage.IndexOf('\0');
                if (i >= 0) recievedMessage = recievedMessage.Substring(0, i);

                //If recieved message is present multi-thread will delegate a update to chatbox
                chatBox.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(
                        delegate ()
                        {
                            chatBox.Items.Add(recievedMessage);
                        }
                        )
                    );

                //clear buffer & rescan
                buffer = new byte[1500];
                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

            }
            catch (Exception ex)
            {
                chatBox.Items.Add("Error: " + ex);
            }

        }

        private void FriendsIPText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private string getLocalIp()
        {
            //Grabs local ip of clients server
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}
