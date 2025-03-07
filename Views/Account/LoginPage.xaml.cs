using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Media.Animation;
using System.Text;
using System.Security.Cryptography;
using SGSClient.Core.Database;
using SGSClient.Core.Authorization;
using Windows.System;
using SGSClient.Helpers;
using Microsoft.UI.Xaml.Navigation;

namespace SGSClient.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel
    {
        get;
    }

    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        Loaded += LoginPage_Loaded; // Dodajemy obsługę zdarzenia Loaded
    }

    private void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppSession.CurrentUserSession.IsLoggedIn == true)
        {
            Frame.Navigate(typeof(MyAccountPage), new DrillInNavigationTransitionInfo());
        }
    }
    public string HashPasswordWithSalt(string password, byte[] salt)
    {
        // Dołącz sol do hasła
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
        Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

        // Wygeneruj skrót hasła z solą
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] hashBytes = sha256Hash.ComputeHash(saltedPassword);

            // Convert the hash bytes to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
    private void buttonLogin_Click(object sender, RoutedEventArgs e)
    {
        if (textBoxEmail.Text.Length == 0)
        {
            errormessage.Text = "Adres e-mail jest wymagany.";
        }
        else if (passwordBox1.Password.Length == 0)
        {
            errormessage.Text = "Hasło jest wymagane.";
        }
        else if (!Regex.IsMatch(textBoxEmail.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
        {
            errormessage.Text = "Wprowadź prawidłowy adres e-mail";
            textBoxEmail.Select(0, textBoxEmail.Text.Length);
        }
        else
        {
            string email = textBoxEmail.Text;
            string password = passwordBox1.Password;
            string query = "SELECT Id, Password FROM [dbo].[Registration] WHERE Email = @Email";

            if (AppSession.CurrentUserSession == null)
            {
                AppSession.CurrentUserSession = new UserSession();
            }

            using (SqlConnection con = new SqlConnection(Db.GetConnectionString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            string hashedPasswordWithSalt = dataSet.Tables[0].Rows[0]["Password"].ToString();
                            string userId = dataSet.Tables[0].Rows[0]["Id"].ToString();
                            string storedHashedPassword = hashedPasswordWithSalt.Substring(0, 64); // Pobierz tylko skrót hasła (bez soli)
                            string storedSalt = hashedPasswordWithSalt.Substring(64); // Pobierz solę

                            // Wygeneruj skrót hasła na podstawie podanego hasła i przechowywanej soli
                            string enteredPasswordHash = HashPasswordWithSalt(password, Convert.FromBase64String(storedSalt));

                            // Sprawdź czy obliczony skrót hasła jest zgodny z przechowywanym skrótem
                            if (enteredPasswordHash == storedHashedPassword)
                            {
                                DateTime loginTime = DateTime.Now;
                                string updateText = @"
UPDATE Registration SET LoginOnTime = @LoginTime WHERE Email = @Email;
UPDATE Registration SET AccessToken = null WHERE Email = @Email;
";
                                using (SqlCommand updateCmdText = new SqlCommand(updateText, con))
                                {
                                    updateCmdText.Parameters.AddWithValue("@LoginTime", loginTime);
                                    updateCmdText.Parameters.AddWithValue("@Email", email);

                                    updateCmdText.ExecuteNonQuery();
                                }

                                AppSession.CurrentUserSession.IsLoggedIn = true;
                                AppSession.CurrentUserSession.UserId = userId;
                                string avatarUrl = GravatarHelper.GetAvatarUrl(email);

                                // Zapisz sesję
                                SessionManager.SaveSession(AppSession.CurrentUserSession);

                                Frame.Navigate(typeof(MyAccountPage), new DrillInNavigationTransitionInfo());
                            }
                            else
                            {
                                errormessage.Text = "Nieprawidłowe hasło.";
                            }
                        }
                        else
                        {
                            errormessage.Text = "Nieprawidłowe hasło.";
                        }
                    }
                }
            }

        }
    }
    private void buttonRegister_Click(object sender, RoutedEventArgs e)
    {
        if (AppSession.CurrentUserSession == null)
        {
            AppSession.CurrentUserSession = new UserSession();
        }

        Frame.Navigate(typeof(RegisterPage), new DrillInNavigationTransitionInfo());
    }
    private void hyperlinkForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(ForgotPasswordPage), new DrillInNavigationTransitionInfo());
    }

}
