using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using RelatedPages.Models;
using System.Linq;
using WikiWalks;

namespace RelatedPages.Controllers
{
    [Route("api/[controller]")]
    public class WikiWalksController : Controller
    {
        private readonly AllWordsGetter allWorsGetter;
        private readonly AllCategoriesGetter allCategoriesGetter;

        public WikiWalksController(AllWordsGetter allWorsGetter, AllCategoriesGetter allCategoriesGetter)
        {
            this.allWorsGetter = allWorsGetter;
            this.allCategoriesGetter = allCategoriesGetter;
        }

        [HttpGet("[action]")]
        public IEnumerable<Page> getPartialWords(int num)
        {
            return allWorsGetter.getPages().Take(num);
        }

        [HttpGet("[action]")]
        public object GetWordIdAndSnippet(string word)
        {
            //Z-Apps専用API
            var w = allWorsGetter.getPages().FirstOrDefault(p => p.word == word);

            return new
            {
                w?.wordId,
                w?.snippet
            };
        }

        [HttpGet("[action]")]
        public IEnumerable<string> GetAllWords()
        {
            //Z-Apps専用API
            return allWorsGetter.getPages()
                .Select(p => p.word)
                .Where(s => System.Text.RegularExpressions.Regex.IsMatch(s,
                @"[\p{IsCJKUnifiedIdeographs}" +
                @"\p{IsCJKCompatibilityIdeographs}" +
                @"\p{IsCJKUnifiedIdeographsExtensionA}]|" +
                @"[\uD840-\uD869][\uDC00-\uDFFF]|\uD869[\uDC00-\uDEDF]")
                && !s.Contains("?") 
                && !s.Contains("&") 
                && !s.Contains("–") 
                && !s.Contains(",")
                && !s.Contains("[")
                && !s.Contains("]")
                );
        }

        [HttpGet("[action]")]
        public IEnumerable<string> GetPartialKanjiWords(int num)
        {
            //Z-Apps専用API
            return allWorsGetter.getPages()
                .Take(num * 2)
                .Select(p => p.word)
                .Where(s => System.Text.RegularExpressions.Regex.IsMatch(s,
                @"[\p{IsCJKUnifiedIdeographs}" +
                @"\p{IsCJKCompatibilityIdeographs}" +
                @"\p{IsCJKUnifiedIdeographsExtensionA}]|" +
                @"[\uD840-\uD869][\uDC00-\uDFFF]|\uD869[\uDC00-\uDEDF]")
                && !s.Contains("?")
                && !s.Contains("&")
                && !s.Contains("–")
                && !s.Contains(",")
                && !s.Contains("[")
                && !s.Contains("]")
                ).Take(num);
        }

        [HttpGet("[action]")]
        public IEnumerable<object> getPartialCategories(int num)
        {
            return allCategoriesGetter.getCategories().Take(num);
        }

        [HttpGet("[action]")]
        public IEnumerable<Page> getWordsForCategory(string category)
        {
            var con = new DBCon();
            var pages = new List<Page>();

            string sql = "select wordId from CategoryJp where category like @category;";

            var result = con.ExecuteSelect(sql, new Dictionary<string, object[]> { { "@category", new object[2] { SqlDbType.NVarChar, category } } });

            result.ForEach(e =>
            {
                var page = allWorsGetter.getPages().FirstOrDefault(w => w.wordId == (int)e["wordId"]);
                if (page != null)
                {
                    pages.Add(page);
                }
            });

            return pages;
        }

        [HttpGet("[action]")]
        public IEnumerable<Page> getWordsForCategoryWithoutSnippet(string category)
        {
            var con = new DBCon();
            var pages = new List<Page>();

            string sql = "select wordId from CategoryJp where category like @category;";

            var result = con.ExecuteSelect(sql, new Dictionary<string, object[]> { { "@category", new object[2] { SqlDbType.NVarChar, category } } });

            result.ForEach(e =>
            {
                var page = allWorsGetter.getPages().FirstOrDefault(w => w.wordId == (int)e["wordId"]);
                if (page != null)
                {
                    //カテゴリページにはSnippetが必要ないため、通信量削減のため除去
                    pages.Add(new Page()
                    {
                        wordId = page.wordId,
                        word = page.word,
                        referenceCount = page.referenceCount
                    });
                }
            });

            return pages;
        }

        [HttpGet("[action]")]
        public object getWord(int wordId)
        {
            if (wordId <= 0) return "";

            var con = new DBCon();
            var result = con.ExecuteSelect("select top(1) word from WordJp where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
            return new { word = (string)result.FirstOrDefault()["word"] };
        }

        [HttpGet("[action]")]
        public object getRelatedArticles(int wordId)
        {
            if (wordId <= 0) return new { };

            var con = new DBCon();
            List<Page> ps = new List<Page>();

            var result = con.ExecuteSelect(@"
select w.wordId, w.word, wr.snippet from WordJp as w
inner join
(select top(500) sourceWordId, snippet from WordReferenceJp where targetWordId = @wordId)
as wr
on w.wordId = wr.sourceWordId;
", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });

            result.ForEach((e) =>
            {
                var page = allWorsGetter.getPages().FirstOrDefault(w => w.wordId == (int)e["wordId"]);
                if (page == null)
                {
                    page = new Page
                    {
                        wordId = (int)e["wordId"],
                        word = (string)e["word"],
                        referenceCount = 0
                    };
                }
                page.snippet = (string)e["snippet"];
                ps.Add(page);
            });

            var pages = ps.OrderByDescending(p => p.referenceCount).ToList();

            return new { pages };
        }

        [HttpGet("[action]")]
        public object getRelatedCategories(int wordId)
        {
            if (wordId <= 0) return new { };

            var con = new DBCon();
            var categories = new List<Category>();

            var result = con.ExecuteSelect("select category from CategoryJp where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
            result.ForEach((f) => 
            {
                var c = allCategoriesGetter.getCategories().FirstOrDefault(ca => ca.category == (string)f["category"]);
                if (c != null)
                {
                    categories.Add(c);
                }
            });

            return new { categories };
        }
    }
}
