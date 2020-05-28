using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using RelatedPages.Models;

namespace RelatedPages.Controllers
{
    [Route("api/[controller]")]
    public class WikiWalksController : Controller
    {
        [HttpGet("[action]")]
        public IEnumerable<object> getAllDates()
        {
            var con = new DBCon();
            var l = new List<object>();

            var result = con.ExecuteSelect($"SELECT publishDate, count(titleId) as cnt FROM Titles GROUP BY publishDate ORDER BY publishDate desc;");

            result.ForEach((e) =>
            {
                var publishDate = DateTime.ParseExact(((int)e["publishDate"]).ToString(), "yyyyMMdd", null);
                var cnt = (int)e["cnt"];

                var dates = new { publishDate, cnt };
                l.Add(dates);
            });

            return l;
        }

        [HttpGet("[action]")]
        public IEnumerable<Title> getTitlesForTheDay(int publishDate)
        {
            var con = new DBCon();
            var l = new List<Title>();

            string sql = @"
select t.publishDate, t.title, t.titleId, p.cnt from (
SELECT * FROM Titles WHERE publishDate = @publishDate
) as t inner join (
SELECT count(*) as cnt, titleId from Pages GROUP BY titleId
) as p on t.titleId = p.titleId;
";

            var result = con.ExecuteSelect(sql, new Dictionary<string, object[]> { { "@publishDate", new object[2] { SqlDbType.Int, publishDate } } });

            result.ForEach((e) =>
            {
                var title = new Title();
                title.publishDate = DateTime.ParseExact(((int)e["publishDate"]).ToString(), "yyyyMMdd", null);
                title.titleId = (int)e["titleId"];
                title.title = (string)e["title"];
                title.cnt = (int)e["cnt"];

                l.Add(title);
            });

            return l;
        }

        [HttpGet("[action]")]
        public IEnumerable<Word> getPagesForTitle(int titleId)
        {
            var con = new DBCon();
            var l = new List<Word>();

            var result = con.ExecuteSelect($"SELECT p.titleId, p.pageName, p.link, p.explanation, t.publishDate, t.title FROM Pages as p inner join Titles as t on p.titleId = t.titleId and p.titleId = @titleId;", new Dictionary<string, object[]> { { "@titleId", new object[2] { SqlDbType.Int, titleId } } });

            result.ForEach((e) =>
            {
                var page = new Word();
                page.titleId = (int)e["titleId"];
                page.title = (string)e["title"];
                page.publishDate = DateTime.ParseExact(((int)e["publishDate"]).ToString(), "yyyyMMdd", null);
                page.link = (string)e["link"];
                page.pageName = (string)e["pageName"];
                page.explanation = (string)e["explanation"];

                l.Add(page);
            });

            return l;
        }
    }
}
