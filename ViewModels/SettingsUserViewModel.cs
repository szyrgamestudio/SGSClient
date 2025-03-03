using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Authorization;

namespace SGSClient.ViewModels;

public partial class SettingsUserViewModel : ObservableRecipient
{
    private readonly IAppUser _appUser;
    public SettingsUserViewModel(IAppUser appUser)
    {
        _appUser = appUser;
    }
    public async Task ChangePasswordCommand()
    {
        await _appUser.ResetPasswordAsync(IntPtr.Zero);
    }
}
