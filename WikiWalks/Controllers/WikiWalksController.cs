using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using RelatedPages.Models;
using System.Linq;
using WikiWalks;
using System.Threading.Tasks;
using System.Text.Json;


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

        public class RelatedArticlesResponse
        {
            public IEnumerable<Page> pages;
        }
        [HttpGet("[action]")]
        public string getRelatedArticles(int wordId)
        {
            if (wordId <= 0) return "{}";

            var con = new DBCon();

            Func<string> getRelatedArticlesWithoutCache = () =>
            {
                var ps = new List<Page>();

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
                        page = new Page();
                        page.wordId = (int)e["wordId"];
                        page.word = (string)e["word"];
                        page.referenceCount = 0;
                    }
                    page.snippet = (string)e["snippet"];
                    ps.Add(page);
                });

                if (ps.Any(p => p.referenceCount > 0))
                {
                    var pages = ps.OrderByDescending(p => p.referenceCount).ToList();
                    return JsonSerializer.Serialize(new { pages });
                }
                else
                {
                    //デプロイ直後でまだallWorsGetterの準備ができていない場合は、
                    //キャッシュテーブルに登録しない
                    return "{}";
                }
            };


            //キャッシュ取得
            var cache = con.ExecuteSelect(@"
select wordId, response
from RelatedArticlesCacheJp
where wordId = @wordId
", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });

            if (cache.Count() > 0)
            {
                //キャッシュデータあり

                Task.Run(async () =>
                {
                    //10秒待って再取得・更新
                    await Task.Delay(10 * 1000);
                    string json = getRelatedArticlesWithoutCache();
                    if (json.Contains("pages"))
                    {
                        con.ExecuteUpdate(@"
update RelatedArticlesCacheJp
set response = @json,
    lastMod = DATEADD(hour, 9, SYSDATETIME())
where wordId = @wordId
", new Dictionary<string, object[]> {
                            { "@json", new object[2] { SqlDbType.NVarChar, json } },
                            { "@wordId", new object[2] { SqlDbType.Int, wordId } }
                        });
                    }
                });

                //上記完了を待たずに、キャッシュされていたデータを返す
                return (string)cache.FirstOrDefault()["response"];
            }
            else
            {
                //キャッシュデータなし
                string json = getRelatedArticlesWithoutCache();
                //キャッシュデータあり
                Task.Run(async () =>
                {
                    //2秒待って登録
                    await Task.Delay(2 * 1000);
                    if (json.Contains("pages"))
                    {
                        con.ExecuteUpdate("insert into RelatedArticlesCacheJp values(@wordId, @json, DATEADD(hour, 9, SYSDATETIME()));", new Dictionary<string, object[]> {
                            { "@json", new object[2] { SqlDbType.NVarChar, json } },
                            { "@wordId", new object[2] { SqlDbType.Int, wordId } }
                        });
                    }
                });
                return json;
            }
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
