using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    { 
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
       
        public Page1(string Name)
        {
            InitializeComponent();
            User.Content += Name;
           
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);

                try
                {
                    _socket.Connect(ip);

                    byte[] data = Encoding.Unicode.GetBytes(Name);
                    _socket.Send(data);

                    int           count         = 0;
                    StringBuilder stringbuilder = new StringBuilder();
                    byte[]        buffer        = new byte[256]; 

                    do
                    {
                        count = _socket.Receive(buffer);
                        stringbuilder.Append(Encoding.Unicode.GetString(buffer, 0, count));
                    }
                    while (_socket.Available > 0);

                    Label_Users.Text = stringbuilder.ToString();

                    Thread receivethread = new Thread(new ThreadStart(ReceiveMessage));
                    receivethread.Start();


                }
                catch
                {
                    CloseSocket();
                }
            }
            catch 
            {
                CloseSocket();
            }
        }

        void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            _socket.Send(data);
        }

        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    int           count         = 0;
                    StringBuilder stringbuilder = new StringBuilder();
                    byte[]        data          = new byte[256];

                    do
                    {
                        count = _socket.Receive(data);
                        stringbuilder.Append(Encoding.Unicode.GetString(data, 0, count));
                    }
                    while (_socket.Available > 0);

                    Label_Chat.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        Label_Chat.Text += stringbuilder.ToString() + "\n";

                        string message = stringbuilder.ToString();
                        if (message.Contains("залетел к нам в чат"))
                        {
                            int    index       = message.IndexOf(" залетел к нам в чат");
                            string messageTemp = message.Substring(0, index);
                            Label_Users.Text += messageTemp + "\n";
                        }
                    }));
                }
                catch
                {
                    CloseSocket();
                }
            }
        }
     
       
        void CloseSocket()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        private void Button_SendMsg(object sender, RoutedEventArgs e)
        {
            SendMessage(Entry1.Text);

            if (Entry1.Text.Contains("/disconect"))
            {
                System.Windows.Application.Current.Shutdown();
            }
           
            string message = Entry1.Text;
            if (Entry1.Text.Contains("/r"))
            {
                string name  = "";
                int    index = message.IndexOf(" ");
                message = message.Remove(0, index + 1);
                index = message.IndexOf(" ");
                name = message.Substring(0, index);
                message = message.Remove(0, index + 1);
                Label_Chat.Text += "(Личное сообщение пользователю " + name + "): " + message + "\n";
            }
            else
            {
                Label_Chat.Text += "Вы: " + Entry1.Text + "\n";
            }
            Entry1.Text = "";
        }
    }
}
