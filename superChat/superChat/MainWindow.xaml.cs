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
        Socket sck;
        EndPoint epLocal, epRemote;
        byte[] buffer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            //Setup socket
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            string localIp = getLocalIp();
            string remoteIp = getLocalIp();

            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
            Console.WriteLine("Please enter your port");
            int userPort = Convert.ToInt32(clientPort.Text);
            Console.WriteLine("Please enter friend port");
            int friendPort = Convert.ToInt32(friendsPort.Text);

            epLocal = new IPEndPoint(IPAddress.Parse(localIp), userPort);
            sck.Bind(epLocal);
            //Connects to remote ip
            epRemote = new IPEndPoint(IPAddress.Parse(remoteIp), friendPort);
            sck.Connect(epRemote);
            //listen to specific port
            buffer = new byte[1500];
            sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            //Convert string message to byte
            ASCIIEncoding aEncoding = new ASCIIEncoding();
            byte[] sendingMessage = new byte[1500];
            sendingMessage = aEncoding.GetBytes(clientMessage.Text);
            //Sending the encoded message
            sck.Send(sendingMessage);
            //Adding to the listbox
            chatBox.Items.Add("Me: " + clientMessage.Text + "\n");
            clientMessage.Text = "";
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                byte[] recievedData = new byte[1500];
                recievedData = (byte[])aResult.AsyncState;
                //Convert byte[] to string
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                string recievedMessage = aEncoding.GetString(recievedData);
                int i = recievedMessage.IndexOf('\0');
                if (i >= 0) recievedMessage = recievedMessage.Substring(0, i);

                chatBox.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(
                        delegate ()
                        {
                            chatBox.Items.Add("Friend: " + recievedMessage);
                        }
                        )
                    );

       

                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

            }
            catch (Exception ex)
            {
                chatBox.Items.Add("Error: " + ex);
            }

        }
        private string getLocalIp()
        {
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
