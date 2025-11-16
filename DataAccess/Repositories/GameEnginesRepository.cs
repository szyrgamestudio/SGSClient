using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Models;
using System.Data;

namespace SGSClient.DataAccess.Repositories;
public static class GameEnginesRepository
{
    public static IEnumerable<GameEngineItem> FetchGameEngines()
    {
        var ds = db.con.select(@"
select
  ge.Id
, ge.Name
from GameEngines ge
");

        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            int id = Convert.ToInt32(dr.TryGetValue("Id"));
            string name = dr.TryGetValue("Name").ToString();

            yield return new GameEngineItem(id, new KeyValuePair<int, string>(id, name));
        }
    }
}
