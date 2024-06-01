using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Core.Database;
using SGSClient.ViewModels;
using SGSClient.Services;
using System;
using System.Data.SqlClient;

namespace SGSClient.Views
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterPage()
        {
            ViewModel = App.GetService<RegisterViewModel>();
            InitializeComponent();
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();

            string email = textBoxEmail.Text;
            string username = textBoxAccountName.Text;
            string password = passwordHasher.HashPassword(passwordBox1.Password);

            if (string.IsNullOrEmpty(passwordBox1.Password))
            {
                errormessage.Text = "Podaj hasło.";
            }
            else if (string.IsNullOrEmpty(passwordBoxConfirm.Password))
            {
                errormessage.Text = "Potwierdź hasło.";
            }
            else if (passwordBox1.Password != passwordBoxConfirm.Password)
            {
                errormessage.Text = "Potwierdzenie hasła musi być takie samo jak hasło.";
            }
            else
            {
                errormessage.Text = "";
                RegisterUser(email, username, password);
            }
        }

        private void RegisterUser(string email, string username, string password)
        {
            using (SqlConnection con = new SqlConnection(Db.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    if (IsUserExisting(email, con))
                    {
                        errormessage.Text = "Użytkownik istnieje.";
                    }
                    else
                    {
                        InsertUserIntoDatabase(email, username, password, con);
                        errormessage.Text = "Zarejestrowano pomyślnie.";
                        Reset();
                    }
                }
                catch (Exception ex)
                {
                    errormessage.Text = $"Error: {ex.Message}";
                }
            }
        }

        private bool IsUserExisting(string email, SqlConnection con)
        {
            string cmdText = "SELECT ID FROM [dbo].[Registration] WHERE Email = @Email";
            using (SqlCommand cmd = new SqlCommand(cmdText, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                object result = cmd.ExecuteScalar();
                return result != null;
            }
        }

        private void InsertUserIntoDatabase(string email, string username, string password, SqlConnection con)
        {
            DateTime registrationTime = DateTime.Now;

            string insertCmd = @"
insert into sgsDevelopers (Name)
values (@Username);

declare @devId int = SCOPE_IDENTITY();

insert into Registration (Email, Password, RegistrationOnTime, DeveloperId)
values (@Email, @Password, @RegistrationOnTime, @devId);
";

            using (SqlCommand cmd = new SqlCommand(insertCmd, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@RegistrationOnTime", registrationTime);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.ExecuteNonQuery();
            }
        }

        private void Reset()
        {
            textBoxAccountName.Text = "";
            textBoxEmail.Text = "";
            passwordBox1.Password = "";
            passwordBoxConfirm.Password = "";
        }
    }
}
