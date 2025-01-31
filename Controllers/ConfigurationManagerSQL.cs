using System.Data;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Models;
using SGSClient.ViewModels;

namespace SGSClient.Controllers
{
    public class ConfigurationManagerSQL(IAppUser appUser)
    {
        private readonly IAppUser _appUser = appUser;
        #region Game info
        public List<GamesViewModel> LoadGamesFromDatabase(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            DataSet ds = db.con.select(@"
select
  CAST(g.Id as nvarchar(max))       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, t.Name	 [GameType]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, CAST(g.DraftP as nvarchar(max)) [DraftP]
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where g.DraftP = 0 and @p0 = 0 or @p0 = 1
order by g.Title
", bypassDraftP);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                GamesViewModel game = new GamesViewModel
                {
                    GameId = dr.TryGetValue("GameId"),
                    GameSymbol = dr.TryGetValue("GameSymbol"),
                    GameTitle = dr.TryGetValue("Title"),
                    GamePayloadName = dr.TryGetValue("PayloadName"),
                    GameExeName = dr.TryGetValue("ExeName"),
                    GameZipLink = dr.TryGetValue("ZipLink"),
                    GameVersionLink = dr.TryGetValue("VersionLink"),
                    GameDescription = dr.TryGetValue("Description"),
                    HardwareRequirements = dr.TryGetValue("HardwareRequirements"),
                    OtherInformations = dr.TryGetValue("OtherInformation"),
                    GameDeveloper = dr.TryGetValue("GameDeveloper"),
                    LogoPath = dr.TryGetValue("LogoPath"),
                    GameType = dr.TryGetValue("GameType"),
                    DraftP = dr.TryGetValue("DraftP"),
                };
                gamesList.Add(game);
            }
            return gamesList;
        }
        public List<GamesViewModel> LoadFeaturedGamesFromDatabase(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            DataSet ds = db.con.select(@"
select
  CAST(g.Id as nvarchar(max))       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, t.Name	 [GameType]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, CAST(g.DraftP as nvarchar(max)) [DraftP]
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where (g.DraftP = 0 and @p0 = 0 or @p0 = 1) and g.FeaturedP = 1
order by g.Title
", bypassDraftP);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                GamesViewModel game = new GamesViewModel
                {
                    GameId = dr.TryGetValue("GameId"),
                    GameSymbol = dr.TryGetValue("GameSymbol"),
                    GameTitle = dr.TryGetValue("Title"),
                    GamePayloadName = dr.TryGetValue("PayloadName"),
                    GameExeName = dr.TryGetValue("ExeName"),
                    GameZipLink = dr.TryGetValue("ZipLink"),
                    GameVersionLink = dr.TryGetValue("VersionLink"),
                    GameDescription = dr.TryGetValue("Description"),
                    HardwareRequirements = dr.TryGetValue("HardwareRequirements"),
                    OtherInformations = dr.TryGetValue("OtherInformation"),
                    GameDeveloper = dr.TryGetValue("GameDeveloper"),
                    LogoPath = dr.TryGetValue("LogoPath"),
                    GameType = dr.TryGetValue("GameType"),
                    DraftP = dr.TryGetValue("DraftP"),
                };
                gamesList.Add(game);
            }
            return gamesList;
        }
        public List<GamesViewModel> LoadMyGamesFromDatabase()
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            // Use the asynchronous version of the database query
            DataSet ds = db.con.select(@"
select  
  g.Id       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, t.Name     [GameType]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, g.DraftP
from Registration r
inner join sgsDevelopers d on d.Id = r.DeveloperId
inner join sgsGames g on g.DeveloperId = d.Id
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where r.id = @p0
", _appUser.UserId);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                GamesViewModel game = new GamesViewModel
                {
                    GameId = dr.TryGetValue("GameId"),
                    GameSymbol = dr.TryGetValue("GameSymbol"),
                    GameTitle = dr.TryGetValue("Title"),
                    GamePayloadName = dr.TryGetValue("PayloadName"),
                    GameExeName = dr.TryGetValue("ExeName"),
                    GameZipLink = dr.TryGetValue("ZipLink"),
                    GameVersionLink = dr.TryGetValue("VersionLink"),
                    GameDescription = dr.TryGetValue("Description"),
                    HardwareRequirements = dr.TryGetValue("HardwareRequirements"),
                    OtherInformations = dr.TryGetValue("OtherInformation"),
                    GameDeveloper = dr.TryGetValue("GameDeveloper"),
                    LogoPath = dr.TryGetValue("LogoPath"),
                    GameType = dr.TryGetValue("GameType"),
                    DraftP = dr.TryGetValue("DraftP"),
                };

                gamesList.Add(game);
            }

            return gamesList;
        }
        public List<string> LoadGalleryImagesFromDatabase(string gameSymbol)
        {
            List<string> galleryImages = [];
            var ds = db.con.select(@"
select
  i.ImagePath
from sgsGameImages i
inner join sgsGames g on g.Id = i.GameId
where g.Symbol = @p0
", gameSymbol);
            foreach (DataRow dr in ds.Tables[0].Rows)
                galleryImages.Add(dr.TryGetValue("ImagePath").ToString());

            return galleryImages;
        }
        public string GetGameVersion(string gameIdentifier)
        {
            List<string> galleryImages = [];
            var ds = db.con.select(@"
select
  g.CurrentVersion
from sgsGames g
where g.Symbol = @p0
", gameIdentifier);
            if (ds.Tables[0].Rows.Count > 0)
                return ds.Tables[0].Rows[0].TryGetValue("GameId");
            else
                return "0.0.0.0";
        }
        #endregion

        #region Rating
        public List<GameRating> LoadRatingsFromDB(string gameIdentifier)
        {
            List<GameRating> gameRatings = new List<GameRating>();

            DataSet ds = db.con.select(@"
select
  gr.Id
, d.Id [DeveloperId]
, d.Name
, gr.Rating
, gr.Title
, gr.Review
from GameRatings gr
inner join sgsGames g on g.Id = gr.GameId
inner join Registration r on r.Id = gr.UserId
inner join sgsDevelopers d on d.Id = r.DeveloperId
where g.Symbol = @p0
", gameIdentifier);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                GameRating rating = new GameRating
                {
                    RatingId = dr.TryGetValue("RatingId"),
                    UserId = dr.TryGetValue("UserId"),
                    Author = dr.TryGetValue("Author"),
                    Rating = dr.TryGetValue("Rating"),
                    Title = dr.TryGetValue("Title"),
                    Review = dr.TryGetValue("Review")
                };
                gameRatings.Add(rating);
            }

            return gameRatings;
        }
        public void AddRatingToDB(string gameIdentifier, GameRating gameRating)
        {
            db.con.exec(@"
declare @gameId int = (select g.Id from sgsGames g where g.Symbol = @p0)
insert into GameRatings (GameId, UserId, Rating, Title, Review, CreationDateTime, ModificationDateTime)
select
  @gameId
, @p1
, @p2
, @p3
, @p4
, GETDATE()
, GETDATE()
", _appUser.UserId, gameRating.Review, gameIdentifier, gameRating.Rating, gameRating.Title);
        }
        public void UpdateRatingInDB(GameRating gameRating)
        {
            db.con.exec(@"
update r set
  r.Rating = @p1
, r.Title = @p2
, r.Review = @p3
, r.ModificationDateTime = GETDATE()
from GameRatings r
where r.Id = @p0
", gameRating.Rating, gameRating.Review, gameRating.Title, gameRating.RatingId);
        }
        public void DeleteRatingInDB(GameRating gameRating)
        {
            db.con.exec(@"
delete from GameRatings
where Id = @p0", gameRating.RatingId);
        }

        #endregion
    }
}
