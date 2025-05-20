using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        // Regisztrációs logika
        // Regisztrációs logika
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Felhasználónév és jelszó ellenőrzése
            string userName = MyTextBox.Text;
            string password = new System.Net.NetworkCredential(string.Empty, MyPasswordBox.SecurePassword).Password;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Registration Error: Please fill in both username and password fields!");
                return;
            }

            // Felhasználónév ellenőrzése
            string filePath = @"..\..\..\..\Resources\credentials.txt";

            if (IsUsernameExists(filePath, userName))
            {
                MessageBox.Show("Registration Error: This user already exists!");
                return;
            }

            // Felhasználónév és jelszópáros kiíratása
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Username: " + userName);
                    writer.WriteLine("Password: " + password); // A jelszót tisztán szövegként tároljuk
                    MessageBox.Show("Registration Successful!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration error: " + ex.Message);
            }
        }


        // Bejelentkezési logika
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Felhasználónév és jelszó ellenőrzése
            string username = MyTextBox.Text;
            string password = new System.Net.NetworkCredential(string.Empty, MyPasswordBox.SecurePassword).Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Login Error: Please fill in both username and password fields!");
                return;
            }

            string filePath = @"..\..\..\..\Resources\credentials.txt";

            // Ellenőrizze, hogy a felhasználónév és a jelszó megegyezik-e a fájlban tároltakkal
            bool userExists = false;
            bool passwordMatch = false;

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    string line = lines[i];
                    if (line.StartsWith("Username: ") && line.Contains(username))
                    {
                        userExists = true;
                        // A következő sor ellenőrzése a jelszóra vonatkozóan
                        string nextLine = lines[i + 1];
                        string[] parts = nextLine.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && parts[1] == password)
                        {
                            passwordMatch = true;
                            break;
                        }
                    }
                }


                if (!userExists)
                {
                    MessageBox.Show("Login Error: User does not exist!");
                }
                else if (!passwordMatch)
                {
                    MessageBox.Show("Login Error: Incorrect password!");
                }
                else
                {
                    // Sikeres bejelentkezés
                    MessageBox.Show("Login Successful!");
                    // Folytassa a bejelentkezési logikát, például nyissa meg az új ablakot
                    ChatWindow cw = new ChatWindow(username);
                    cw.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login Error: " + ex.Message);
            }
        }




        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }




        //---------- Regisztracio ellenorzes ----------//
        private bool IsUsernameExists(string filePath, string userName)
        {
            //Fájl meglétének ellenőrzése
            if (!File.Exists(filePath))
                return false;

            //Fájl bejárása
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                //Sorok ellenőrzése (Csak a felhasznlónév sorokat ellenőrzi)
                if (line.StartsWith("Username:") && line.Contains(userName))
                    return true;
            }
            return false;
        }



    }
}
