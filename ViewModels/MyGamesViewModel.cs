using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Models;

namespace SGSClient.ViewModels
{
    public partial class MyGamesViewModel : ObservableRecipient
    {
        #region Ctors and Properties
        private readonly IAppUser _appUser;
        public ObservableCollection<Game> MyGamesList { get; private set; } = new();
        public MyGamesViewModel(IAppUser appUser)
        {
            _appUser = appUser;
        }
        #endregion

        #region Public Methods
        public void LoadMyGamesFromDatabase()
        {
            try
            {
                MyGamesList = new ObservableCollection<Game>(FetchMyGamesFromDatabase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading my games: {ex.Message}");
            }
        }
        #endregion

        #region Private or Protected Methods
        private IEnumerable<Game> FetchMyGamesFromDatabase
        {
            get
            {
                DataSet ds = db.con.select(@"
select
  g.Id       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, u.DisplayName     [GameDeveloper]
, l.Url [LogoPath]
, t.Name	 [GameType]
--, g.PayloadName
, g.ExeName
, g.ZipLink
--, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, g.DraftP
from Games g
inner join Users u on u.Id = g.UserId
left join GameImages l on l.GameId = g.Id and l.LogoP = 1
left join GameTypes t on t.Id = g.TypeId
--where r.UserId = @p0
order by g.Title
", _appUser.UserId);

                return ds.Tables[0].AsEnumerable().Select(dr => new Game
                {
                    GameId = dr.TryGetValue<int>("GameId"),
                    GameSymbol = dr.TryGetValue<string>("GameSymbol"),
                    GameName = dr.TryGetValue<string>("Title"),
                    GameDeveloper = dr.TryGetValue<string>("GameDeveloper"),
                    GameTitle = dr.TryGetValue<string>("Title"),
                    ImageSource = new Uri(dr.TryGetValue<string>("LogoPath") ?? "about:blank"),
                    GameVersion = string.Empty,
                    GamePayloadName = dr.TryGetValue<string>("PayloadName"),
                    GameExeName = dr.TryGetValue<string>("ExeName"),
                    GameZipLink = dr.TryGetValue<string>("ZipLink"),
                    GameVersionLink = dr.TryGetValue<string>("VersionLink"),
                    GameDescription = dr.TryGetValue<string>("Description"),
                    HardwareRequirements = dr.TryGetValue<string>("HardwareRequirements"),
                    OtherInformations = dr.TryGetValue<string>("OtherInformation"),
                    LogoPath = dr.TryGetValue<string>("LogoPath"),
                    GameType = dr.TryGetValue<string>("GameType"),
                    DraftP = dr.TryGetValue<bool>("DraftP")
                });
            }
        }
        #endregion
    }
}