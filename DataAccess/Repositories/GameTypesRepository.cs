using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Models;
using System.Data;

namespace SGSClient.DataAccess.Repositories;
public static class GameTypesRepository
{
    public static IEnumerable<GameTypeItem> FetchGameTypes()
    {
        var ds = db.con.select(@"
select
  gt.Id
, gt.Name
from GameTypes gt
");

        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            int id = Convert.ToInt32(dr.TryGetValue("Id"));
            string name = dr.TryGetValue("Name").ToString();

            yield return new GameTypeItem(id, new KeyValuePair<int, string>(id, name));
        }
    }
}
