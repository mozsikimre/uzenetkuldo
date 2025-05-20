using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
using System.Windows.Threading;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {

        //---------- Socket Inicializalas ----------//
        private Socket clientSocket;
        private List<ConnectedClient> connectedClients;

        //---------- Felhasználónév lekérése ----------//
        public string Username { get; set; }
        public class ConnectedClient
        {
            public string ipaddress { get; set; }
            public int port { get; set; }
            public string username { get; set; }
        }

        public ChatWindow(string username)
        {
            Username = username;
            InitializeComponent();
            UsernameTextBox.Text = Username;
            connectedClients = new List<ConnectedClient>();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        //---------- Szerverhez való csatlakozás ----------//
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await Task.Run(() => clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888)));

                Log("Kapcsolódva a szerverhez... ");

                await Task.Run(() => SendUsername(Username, clientSocket));

                await Task.Run(() => ReceiveMessages(clientSocket));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt a kapcsolódás során: " + ex.Message);
            }
        }
        private async Task SendUsername(string username, Socket clientSocket)
        {
            // Felhasználónév elküldése speciális üzenetként a szervernek
            string message = $"USERNAME:{username}|RECEIVER:|STATUS:discover|MESSAGE:";
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            await Task.Run(() => clientSocket.Send(messageData));
        }

        //---------- Üzenet fogadás ----------//
        private async Task ReceiveMessages(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (true)
            {
                try
                {
                    bytesRead = await Task.Run(() => clientSocket.Receive(buffer));
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Az üzenet feldolgozása
                    string[] parts = receivedMessage.Split('|');

                    string senderUsername = parts.FirstOrDefault(p => p.StartsWith("USERNAME:"))?.Substring("USERNAME:".Length);
                    string receiverUsername = parts.FirstOrDefault(p => p.StartsWith("RECEIVER:"))?.Substring("RECEIVER:".Length);
                    string status = parts.FirstOrDefault(p => p.StartsWith("STATUS:"))?.Substring("STATUS:".Length);
                    string message = parts.FirstOrDefault(p => p.StartsWith("MESSAGE:"))?.Substring("MESSAGE:".Length);

                    // Ha van üzenet rész a szövegben, akkor írjuk ki a Log-ba
                    if (!string.IsNullOrWhiteSpace(message) && status.Trim().ToLower() == "letter")
                    {
                        // Kiírjuk az üzenetet a logba
                        await Dispatcher.InvokeAsync(() => Log("[" + senderUsername + "]: " + message));
                    }
                    else if (status.Trim().ToLower() == "discover")
                    {
                        await Dispatcher.InvokeAsync(() => Log("[" + senderUsername + "]: sent a DISCOVER message!"));
                    }
                    else if (status.Trim().ToLower() == "active")
                    {
                        await Dispatcher.InvokeAsync(() => Log("[" + senderUsername + "]: sent an ACTIVE message!"));
                    }

                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() => Log("Hiba történt az üzenet fogadása közben: " + ex.Message));
                    break;
                }
            }
        }

        //---------- Log kiíratás ----------//
        private void Log(string message)
        {
            LogTextBox.AppendText(message + "\n");
        }

        private void UsersLog()
        {
            // Töröljük a jelenlegi tartalmat
            ActiveTextBox.Clear();

            // A kliensek adatainak kiírása
            foreach (var client in connectedClients)
            {
                ActiveTextBox.AppendText(client.username + "\n");
            }
        }

        //---------- Üzenet küldés ----------//
        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageTextBox.Text;

                // Ellenőrizzük, hogy az üzenet elején van-e "/felhasznalonev"
                if (message.StartsWith("/"))
                {
                    int spaceIndex = message.IndexOf(' ');
                    if (spaceIndex != -1)
                    {
                        // Kinyerjük a címzett felhasználónevet az üzenetből
                        string recipient = message.Substring(1, spaceIndex - 1);
                        message = message.Substring(spaceIndex + 1);

                        // Beállítjuk a címzett felhasználónevet
                        string receiverUsername = recipient;
                        string status = "letter";

                        // Az üzenet formázása
                        string formattedMessage = $"USERNAME:{Username}|RECEIVER:{receiverUsername}|STATUS:{status}|MESSAGE:{message}";

                        // Az üzenet küldése
                        byte[] buffer = Encoding.UTF8.GetBytes(formattedMessage);
                        await Task.Run(() => clientSocket.Send(buffer));

                        // Naplózzuk az üzenetet
                        await Dispatcher.InvokeAsync(() => Log("[" + Username + "]: " + message));
                    }
                    else
                    {
                        // Hibakezelés: nincs szóköz a "/felhasznalonev" és az üzenet között
                        throw new ArgumentException("Hibás üzenetformátum: nincs szóköz a felhasználónév és az üzenet között.");
                    }
                }
                else
                {
                    // Ha nincs "/felhasznalonev", az üzenetet az alapértelmezett módon küldjük
                    string status = "letter";
                    string formattedMessage = $"USERNAME:{Username}|RECEIVER:|STATUS:{status}|MESSAGE:{message}";

                    byte[] buffer = Encoding.UTF8.GetBytes(formattedMessage);

                    await Task.Run(() => clientSocket.Send(buffer));

                    // Naplózzuk az üzenetet
                    if (status.Trim().ToLower() == "letter")
                    {
                        await Dispatcher.InvokeAsync(() => Log("[" + Username + "]: " + message));
                    }
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => MessageBox.Show("Hiba történt az üzenet küldése közben: " + ex.Message));
            }
        }



    }

}
