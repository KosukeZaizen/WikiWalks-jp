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
        public IEnumerable<Page> getWordsForCategory(string category)
        {
            var con = new DBCon();
            var pages = new List<Page>();

            string sql = @"
select c.wordId, w.word, w.snippet 
from category as c 
inner join word as w 
on c.wordId = w.wordId 
and c.category like @category;
";

            var result = con.ExecuteSelect(sql, new Dictionary<string, object[]> { { "@category", new object[2] { SqlDbType.NVarChar, category } } });

            result.ForEach((e) =>
            {
                var page = new Page();
                page.wordId = (int)e["wordId"];
                page.word = (string)e["word"];
                page.snippet = (string)e["snippet"];

                page.categories = new List<string>();
                var result2 = con.ExecuteSelect($"select category from Category where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, page.wordId } } });
                result2.ForEach((f) =>
                {
                    ((List<string>)page.categories).Add((string)f["category"]);
                });

                pages.Add(page);
            });

            return pages;
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

                var res = con.ExecuteSelect($"select wr.sourceWordId, w.word, w.snippet from WordReference as wr inner join word as w on wr.sourceWordId = w.wordId and targetWordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, page.wordId } } });
                page.referenceCount = res.Count();

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
