using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Core.Database;
using SGSClient.ViewModels;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Windows.Networking;

namespace SGSClient.Views;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel
    {
        get;
    }

    public RegisterPage()
    {
        ViewModel = App.GetService<RegisterViewModel>();
        InitializeComponent();
    }

    private string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

        // Append the salt to the password
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
        Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

        // Compute the hash using SHA-256 with salt
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] hashBytes = sha256Hash.ComputeHash(saltedPassword);

            // Convert the hash bytes to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            // Append the salt to the hashed password
            builder.Append(Convert.ToBase64String(salt));

            return builder.ToString();
        }
    }

    private void buttonRegister_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        string password = HashPassword(passwordBox1.Password);
        if (passwordBox1.Password.Length == 0)
        {
            errormessage.Text = "Podaj hasło.";
            //passwordBox1.Focus();
        }
        else if (passwordBoxConfirm.Password.Length == 0)
        {
            errormessage.Text = "Potwierdź hasło.";
            //passwordBoxConfirm.Focus();
        }
        else if (passwordBox1.Password != passwordBoxConfirm.Password)
        {
            errormessage.Text = "Potwierdzenie hasła musi być takie samo jak hasło.";
            //passwordBoxConfirm.Focus();
        }
        else
        {
            errormessage.Text = "";
            //string address = textBoxAddress.Text;

            string constr = db.con;
            SqlConnection con = new SqlConnection(constr);

            try
            {
                con.Open();

                string cmdText = string.Format("SELECT ID FROM [dbo].[Registration] Where Email = '{0}'", email);
                SqlCommand cmd = new SqlCommand(cmdText, con);
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    errormessage.Text = "Użytkownik istnieje.";
                    //passwordBoxConfirm.Focus();
                }
                else
                {
                    DateTime registrationTime = DateTime.Now;

                    // Wstawienie zaszyfrowanego hasła do bazy danych
                    SqlCommand cmd1 = new SqlCommand("INSERT INTO [dbo].[Registration] (Email, Password, RegistrationOnTime) VALUES (@Email, @Password, @RegistrationOnTime)", con);
                    cmd1.Parameters.AddWithValue("@Email", email);
                    cmd1.Parameters.AddWithValue("@Password", password);
                    cmd1.Parameters.AddWithValue("@RegistrationOnTime", registrationTime);
                    cmd1.ExecuteNonQuery();

                    errormessage.Text = "Zarejestrowano pomyślnie.";
                    Reset();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
    }

    private void Reset()
    {
        //textBoxFirstName.Text = "";
        //textBoxLastName.Text = "";
        textBoxEmail.Text = "";
        passwordBox1.Password = "";
        passwordBoxConfirm.Password = "";
        //textBoxAddress.Text = "";
    }
}
