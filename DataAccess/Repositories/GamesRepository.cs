﻿using System.Data;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Models;

namespace SGSClient.DataAccess.Repositories
{
    public static class GamesRepository
    {
        public static IEnumerable<Game> FetchGames(bool bypassDraftP)
        {
            DataSet ds = db.con.select(@"
select
  g.Id       [GameId]
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
, g.DraftP
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where g.DraftP = 0 and @p0 = 0 or @p0 = 1
order by g.Title", bypassDraftP);

            return MapGames(ds);
        }
        public static IEnumerable<Game> FetchFeaturedGames(bool bypassDraftP)
        {
            DataSet ds = db.con.select(@"
select
  g.Id       [GameId]
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
, g.DraftP [DraftP]
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where (g.DraftP = 0 and @p0 = 0 or @p0 = 1) and g.FeaturedP = 1
order by g.Title", bypassDraftP);

            return MapGames(ds);
        }
        private static EnumerableRowCollection<Game> MapGames(DataSet ds)
        {
            return ds.Tables[0].AsEnumerable().Select(dr =>
            {
                var baseGame = new Game
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
                };

                return baseGame;
            });
        }
    }
}