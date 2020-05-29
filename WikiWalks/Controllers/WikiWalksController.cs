using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using RelatedPages.Models;
using System.Linq;

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
        public object getRelatedArticles(int wordId)
        {
            if (wordId <= 0) return new { };

            var con = new DBCon();
            var pages = new List<Page>();
            var categories = new List<String>();

            var result1 = con.ExecuteSelect($"select word from word where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
            string word = (string)result1.FirstOrDefault()["word"];

            var result2 = con.ExecuteSelect($"select wr.sourceWordId, w.word, w.snippet from WordReference as wr inner join word as w on wr.sourceWordId = w.wordId and targetWordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
            result2.ForEach((e) =>
            {
                var page = new Page();
                page.wordId = (int)e["sourceWordId"];
                page.word = (string)e["word"];
                page.snippet = (string)e["snippet"];

                page.categories = new List<string>();
                var result3 = con.ExecuteSelect($"select category from Category where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, page.wordId } } });
                result3.ForEach((f) =>
                {
                    ((List<string>)page.categories).Add((string)f["category"]);
                });

                pages.Add(page);
            });

            var result4 = con.ExecuteSelect($"select category from Category where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
            result4.ForEach((e) =>
            {
                categories.Add((string)e["category"]);
            });

            return new { wordId, word, pages, categories };
        }
    }
}
