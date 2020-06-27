using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using RelatedPages.Models;
using System.Linq;
using WikiWalks;
using System.Threading.Tasks;

namespace RelatedPages.Controllers
{
    [Route("api/[controller]")]
    public class WikiWalksController : Controller
    {
        private readonly AllWorsGetter allWorsGetter;
        private readonly AllCategoriesGetter allCategoriesGetter;

        public WikiWalksController(AllWorsGetter allWorsGetter, AllCategoriesGetter allCategoriesGetter)
        {
            this.allWorsGetter = allWorsGetter;
            this.allCategoriesGetter = allCategoriesGetter;
        }

        [HttpGet("[action]")]
        public IEnumerable<Page> getAllWords()
        {
            return allWorsGetter.getPages();
        }

        [HttpGet("[action]")]
        public IEnumerable<object> getAllCategories()
        {
            return allCategoriesGetter.getCategories();
        }

        [HttpGet("[action]")]
        public IEnumerable<Page> getWordsForCategory(string category)
        {
            var con = new DBCon();
            var pages = new List<Page>();

            string sql = @"
select wordsForCategory.wordId, wordsForCategory.word, 
max(case when wr.targetWordId = wr.sourceWordId then wr.snippet else ' ' + wr.snippet end) as snippet, 
count(wr.targetWordId) as cnt 
from
(
select c.wordId, w.word 
from CategoryJp as c 
inner join WordJp as w 
on c.wordId = w.wordId 
and c.category like @category
) as wordsForCategory
left outer join WordReferenceJp as wr
on wordsForCategory.wordId = wr.targetWordId
group by wordsForCategory.wordId, wordsForCategory.word
order by cnt desc
;";

            var result = con.ExecuteSelect(sql, new Dictionary<string, object[]> { { "@category", new object[2] { SqlDbType.NVarChar, category } } });

            result.ForEach((e) =>
            {
                var page = new Page();
                page.wordId = (int)e["wordId"];
                page.word = (string)e["word"];
                page.snippet = (string)e["snippet"];
                page.referenceCount = (int)e["cnt"];

                pages.Add(page);
            });

            return pages;
        }

        [HttpGet("[action]")]
        public async Task<object> getRelatedArticles(int wordId)
        {
            if (wordId <= 0) return new { };

            Task<List<Page>> pagesTask = Task.Run(() =>
            {
                var con = new DBCon();
                var ps = new List<Page>();

                var result = con.ExecuteSelect(@"
select firstRef.sourceWordId, firstRef.word, firstRef.snippet, count(wwrr.targetWordId) as cnt
from WordReferenceJp as wwrr
right outer join (
select wr.sourceWordId, w.word, wr.snippet 
from (
select sourceWordId, snippet from WordReferenceJp where targetWordId = @wordId
) as wr 
inner join WordJp as w 
on wr.sourceWordId = w.wordId 
) as firstRef
on firstRef.sourceWordId = wwrr.targetWordId
group by firstRef.sourceWordId, firstRef.word, firstRef.snippet
order by cnt desc;
", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });

                result.ForEach((e) =>
                {
                    var page = new Page();
                    page.wordId = (int)e["sourceWordId"];

                    page.word = (string)e["word"];
                    page.snippet = (string)e["snippet"];
                    page.referenceCount = (int)e["cnt"];

                    ps.Add(page);
                });
                return ps;
            });

            Task<List<Category>> categoriesTask = Task.Run(() =>
            {
                var con = new DBCon();
                var cs = new List<Category>();

                var result = con.ExecuteSelect(@"
select category, count(*) as cnt 
from (
	select wordId, category, count(*) as cnt1 from 
	(
	select wordId, category from CategoryJp A where exists (select * from CategoryJp B where A.category = B.category and wordId = @wordId)
	) as c 
	inner join WordReferenceJp as r 
	on c.wordId = r.targetWordId 
	group by wordId, category
	having count(*) > 4
) as rel
group by category
order by cnt desc;
", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
                result.ForEach((f) =>
                {
                    var c = new Category();
                    c.category = (string)f["category"];
                    c.cnt = (int)f["cnt"];
                    cs.Add(c);
                });
                return cs;
            });

            Task<string> wordTask = Task.Run(() =>
            {
                var con = new DBCon();
                var result = con.ExecuteSelect($"select word from WordJp where wordId = @wordId;", new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } });
                return (string)result.FirstOrDefault()["word"];
            });


            var word = await wordTask;
            var categories = await categoriesTask;
            var pages = await pagesTask;
            return new { wordId, word, pages, categories };
        }
    }
}
