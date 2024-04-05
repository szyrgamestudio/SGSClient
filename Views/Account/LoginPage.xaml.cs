using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;

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
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
        if (textBoxEmail.Text.Length == 0)
        {
            errormessage.Text = "Adres e-mail jest wymagany.";
            //textBoxEmail.Focus();
        }
        else if (passwordBox1.Password.Length == 0)
        {
            errormessage.Text = "Hasło jest wymagane.";
            //textBoxEmail.Focus();
        }
        else if (!Regex.IsMatch(textBoxEmail.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
        {
            errormessage.Text = "Wprowadź prawidłowy adres e-mail";
            textBoxEmail.Select(0, textBoxEmail.Text.Length);
            //textBoxEmail.Focus();
        }
        else
        {
            string email = textBoxEmail.Text;
            string password = passwordBox1.Password;
            SqlConnection con = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RefistrationAndLogin;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            con.Open();
            SqlCommand cmd = new SqlCommand("Select * from  [dbo].[Registration]  where Email='" + email + "'  and password='" + password + "'", con);
            cmd.CommandType = CommandType.Text;
            object result = cmd.ExecuteScalar();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                DateTime loginTime = DateTime.Now;
                string updateText = string.Format("UPDATE [dbo].[Registration] SET LoginOnTime = '{0}' WHERE ID = '{1}'", loginTime, result);
                SqlCommand updateCmdText = new SqlCommand(updateText, con);
                updateCmdText.ExecuteNonQuery();
                string username = dataSet.Tables[0].Rows[0]["FirstName"].ToString() + " " + dataSet.Tables[0].Rows[0]["LastName"].ToString();
                //welcome.TextBlockName.Text = username;
                //welcome.Show();
                //Close();
            }
            else
            {
                errormessage.Text = "Sorry! Please enter existing emailid/password.";
            }
            con.Close();
        }
    }
    private void buttonRegister_Click(object sender, RoutedEventArgs e)
    {
        //registration.Show();
        //Close();
    }

}
