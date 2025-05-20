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
using System.Net.WebSockets;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        //Szerver Socket Inicializacio
        private Socket serverSocket;
        private List<Socket> clientSockets;
        private List<ConnectedClient> connectedClients;

        //Username importalas
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
            clientSockets = new List<Socket>();
            connectedClients = new List<ConnectedClient>();
        }

        //--------------- Random cucc? ---------------//
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void CloseConnection(Socket clientSocket)
        {
            try
            {
                // Kapcsolat lezárása
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                // Kliens eltávolítása az aktív kliensek listájából
                var clientToRemove = connectedClients.FirstOrDefault(c =>
                    c.ipaddress == ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString() &&
                    c.port == ((IPEndPoint)clientSocket.RemoteEndPoint).Port);

                if (clientToRemove != null)
                {
                    connectedClients.Remove(clientToRemove);
                    // Frissítjük az aktív felhasználók listáját
                    UsersLog();
                }
            }
            catch (Exception ex)
            {
                // Hiba esetén logoljuk
                Log("Hiba történt a kapcsolat lezárása közben: " + ex.Message);
            }
        }


        private void ActiveUsers(Socket clientSocket, string senderName)
        {
            // Ellenőrizzük, hogy a felhasználó már szerepel-e a listában
            var existingClient = connectedClients.FirstOrDefault(c => c.username == senderName);

            //Aktuális kliens IP címének és Portjának lekérése
            string clientIPAddress = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)clientSocket.RemoteEndPoint).Port;

            // Ha nem szerepel még a listában, akkor hozzáadjuk
            if (existingClient == null)
            {
                // Hozzáadjuk az aktuálisan csatlakozott kliens adatait a listához
                connectedClients.Add(new ConnectedClient { ipaddress = clientIPAddress, port = clientPort, username = senderName });

                // Új kliens csatlakozása után frissítjük a felhasználói listát
                UsersLog();
            }
        }
        //--------------- Szerver indítása ---------------//
        private async void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Szerver Socket létrehozása
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8888);

                // Socket kötése az IP-címhez és portszámhoz
                serverSocket.Bind(iPEndPoint);
                // Várakozás a kliensek csatlakozására
                serverSocket.Listen(5);

                // Naplózás: A szerver sikeresen elindult
                Log("Szerver elindult...");

                while (true)
                {
                    // Várakozás a kliens csatlakozására
                    Socket clientSocket = await Task.Run(() => serverSocket.Accept());
                    // Hozzáadás a kliens socketek listájához
                    clientSockets.Add(clientSocket);
                    // Naplózás: Egy kliens sikeresen csatlakozott
                    Log("Kliens csatlakozott");

                    // Felhasználónév küldése a kliensnek
                    await Task.Run(() => SendUsername(Username, clientSocket));

                    // Üzenetek fogadása a kliensről
                    ReceiveMessages(clientSocket);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt a szerver elindítása során: " + ex.Message);
            }
        }


        private async Task SendUsername(string username, Socket clientSocket)
        {
            // Felhasználónév elküldése speciális üzenetként a szervernek
            string message = $"USERNAME:{username}|RECEIVER:|STATUS:discover|MESSAGE:";
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            await Task.Run(() => clientSocket.Send(messageData));
        }

        //--------------- Szerver indítása ---------------//
        private async Task ReceiveMessages(Socket clientSocket)
        {
            // Buffer létrehozása az adatok fogadásához
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    // Üzenet fogadása a kliensről
                    int bytesRead = await Task.Run(() => clientSocket.Receive(buffer));
                    // Fogadott adatok dekódolása szöveggé

                    // Ellenőrizzük, hogy a kliens lecsatlakozott-e
                    if (bytesRead == 0)
                    {
                        // Ha a kliens lecsatlakozott, akkor zárjuk le a kapcsolatot és távolítsuk el a kliens adatait
                        CloseConnection(clientSocket);
                        break; // Kilépünk a végtelen while ciklusból
                    }

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Az üzenet feldolgozása
                    string[] parts = receivedMessage.Split('|');

                    // Feladó felhasználónevének, Címzett felhasználónevének, üzenetnek és státusznak kinyerése
                    string senderUsername = parts.FirstOrDefault(p => p.StartsWith("USERNAME:"))?.Substring("USERNAME:".Length);
                    string receiverUsername = parts.FirstOrDefault(p => p.StartsWith("RECEIVER:"))?.Substring("RECEIVER:".Length);
                    string status = parts.FirstOrDefault(p => p.StartsWith("STATUS:"))?.Substring("STATUS:".Length);
                    string message = parts.FirstOrDefault(p => p.StartsWith("MESSAGE:"))?.Substring("MESSAGE:".Length);

                    // Ha van üzenet rész a fogadott üzenetben, akkor kiírjuk a Log-ba
                    if (status.Trim().ToLower() == "letter" && string.IsNullOrEmpty(receiverUsername))
                    {
                        // Üzenet naplózása, beleértve a feladó nevét is
                        await Dispatcher.InvokeAsync(() => Log("[" + senderUsername + "]: " + message));
                    }

                    else if (status.Trim().ToLower() == "discover")
                    {
                        // Aktív felhasználók frissítése
                        await Dispatcher.InvokeAsync(() => Log("[" + senderUsername + "] sent a DISCOVER message!"));
                        ActiveUsers(clientSocket, senderUsername);
                    }

                    // Küldjük el az üzenetet annak a kliensnek, akinek a receiverUsername-e megegyezik a fogadott üzenetben lévővel
                    foreach (var otherClientSocket in clientSockets)
                    {
                        // Kihagyjuk az üzenet küldőjét a loop-ból
                        if (otherClientSocket != clientSocket)
                        {
                            string otherClientUsername = connectedClients.FirstOrDefault(c => c.ipaddress == ((IPEndPoint)otherClientSocket.RemoteEndPoint).Address.ToString() && c.port == ((IPEndPoint)otherClientSocket.RemoteEndPoint).Port)?.username;

                            // Ha a receiverUsername üres, akkor az üzenetet mindenki számára elküldjük,
                            // kivéve az üzenet küldőjének
                            if (string.IsNullOrEmpty(receiverUsername))
                            {
                                // Ha az otherClientUsername nem üres, akkor elküldjük az üzenetet
                                if (!string.IsNullOrEmpty(otherClientUsername))
                                {
                                    // Átállítjuk a buffer tartalmát a státusz mezővel együtt
                                    string fullMessage = "USERNAME:" + senderUsername + "|RECEIVER:" + receiverUsername + "|STATUS:" + status + "|MESSAGE:" + message;
                                    byte[] fullMessageBytes = Encoding.UTF8.GetBytes(fullMessage);

                                    await Task.Run(() => otherClientSocket.Send(fullMessageBytes, 0, fullMessageBytes.Length, SocketFlags.None));
                                }
                            }
                            // Ha a receiverUsername nem üres, akkor csak annak a kliensnek küldjük el az üzenetet,
                            // akinek a felhasználóneve megegyezik a receiverUsername-el
                            else if (otherClientUsername == receiverUsername)
                            {
                                // Átállítjuk a buffer tartalmát a státusz mezővel együtt
                                string fullMessage = "USERNAME:" + senderUsername + "|RECEIVER:" + receiverUsername + "|STATUS:" + status + "|MESSAGE:" + message;
                                byte[] fullMessageBytes = Encoding.UTF8.GetBytes(fullMessage);

                                await Task.Run(() => otherClientSocket.Send(fullMessageBytes, 0, fullMessageBytes.Length, SocketFlags.None));
                            }
                        }
                    }

                }
                catch (ObjectDisposedException ex)
                {
                    // A kapcsolat lezárása közben a Socket objektumot már felszabadították vagy bezárták.
                    Log("Hiba történt a kapcsolat lezárása közben: " + ex.Message);
                }
                catch (SocketException ex)
                {
                    // Ha a távoli állomás kényszerítetten bezárta a kapcsolatot, kezeljük ezt a kivételt
                    if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        // A kliens lecsatlakozott, ezért zárjuk le a kapcsolatot és távolítsuk el a kliens adatait
                        CloseConnection(clientSocket);
                    }
                    else
                    {
                        // Más típusú hiba történt, ezt logoljuk
                        Log("Hiba történt az üzenet fogadása közben: " + ex.Message);
                    }
                    break; // Kilépünk a végtelen while ciklusból
                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() => Log("Hiba történt az üzenet fogadása közben: " + ex.Message));
                    break;
                }

            }
        }


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

                        foreach (var connectedClient in connectedClients)
                        {
                            // Ellenőrizzük, hogy a kliens felhasználóneve egyezik-e a címzett felhasználónévvel
                            if (connectedClient.username == receiverUsername)
                            {
                                // Kliens socket keresése a ConnectedClients alapján
                                var clientSocket = clientSockets.FirstOrDefault(socket =>
                                {
                                    var remoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                                    return remoteEndPoint.Address.ToString() == connectedClient.ipaddress &&
                                           remoteEndPoint.Port == connectedClient.port;
                                });

                                // Ha megtaláltuk a megfelelő kliens socketet, küldjük el neki az üzenetet
                                if (clientSocket != null)
                                {
                                    await Task.Run(() => clientSocket.Send(buffer));
                                }
                            }
                        }


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

                    foreach (var clientSocket in clientSockets)
                    {
                        await Task.Run(() => clientSocket.Send(buffer));
                    }

                    // Naplózzuk az üzenetet
                    await Dispatcher.InvokeAsync(() => Log("[" + Username + "]: " + message));
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => MessageBox.Show("Hiba történt az üzenet küldése közben: " + ex.Message));
            }
        }
    }

}