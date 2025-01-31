using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Interface;

namespace SGSClient.ViewModels;

public partial class RegisterViewModel : ObservableRecipient
{
    private readonly IPasswordHasher _passwordHasher;
    public string ErrorMessage { get; private set; }

    public RegisterViewModel(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }
    public async Task RegisterUserAsync(string email, string username, string password)
    {
        var dataSet = db.con.select(SqlQueries.checkUserSql, email);
        if (dataSet.Tables[0].Rows.Count == 0)
        {
            return;
        }
        var registrationTime = DateTime.Now;
        try
        {
            db.con.select(SqlQueries.insertUserSql, username, email, _passwordHasher.HashPassword(password), registrationTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while registering the user: {ex.Message}");
            ErrorMessage = "An error occurred while registering your account. Please try again.";
        }

    }
}
