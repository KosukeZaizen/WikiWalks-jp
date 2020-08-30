using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RelatedPages.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using System;
using System.Data;
using System.Net.Http;

namespace WikiWalks
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            //一旦ロックを解除
            //DB_Util.releaseLock();

            var allWorsGetter = new AllWordsGetter();
            services.AddSingleton(allWorsGetter);

            var allCategoriesGetter = new AllCategoriesGetter(allWorsGetter);
            services.AddSingleton(allCategoriesGetter);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, AllWordsGetter allWorsGetter, AllCategoriesGetter allCategoriesGetter)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            var options = new RewriteOptions().AddRedirect("(.*)/$", "$1");
            app.UseRewriter(options);

            app.Use(async (context, next) =>
            {
                string url = context.Request.Path.Value;
                if (url.EndsWith("sitemap.xml"))
                {
                    var domain = "https://wiki-jp.lingual-ninja.com";
                    var lstSitemap = new List<Dictionary<string, string>>();

                    //top page
                    var dic1 = new Dictionary<string, string>();
                    dic1["loc"] = domain;
                    lstSitemap.Add(dic1);

                    //all keywords page
                    var dicAll = new Dictionary<string, string>();
                    dicAll["loc"] = domain + "/all";
                    lstSitemap.Add(dicAll);

                    //category page
                    //カテゴリページはnoindexとし、除外
                    //IEnumerable<string> allCategories = allCategoriesGetter.getCategories().Select(c => c.category);
                    //foreach (var category in allCategories)
                    //{
                    //    var dic2 = new Dictionary<string, string>();
                    //    dic2["loc"] = domain + "/category/" + HttpUtility.UrlEncode(category.Replace(" ", "_")).Replace("%27", "'");
                    //    lstSitemap.Add(dic2);
                    //}

                    //word page
                    allWorsGetter.addNewPages();
                    IEnumerable<int> allWordId = allWorsGetter.getPages().Select(p => p.wordId);
                    foreach (var wordId in allWordId)
                    {
                        var dicWordId = new Dictionary<string, string>();
                        dicWordId["loc"] = domain + "/word/" + HttpUtility.UrlEncode(wordId.ToString());
                        lstSitemap.Add(dicWordId);
                    }

                    string resultXML = RegisterSitemap(lstSitemap);

                    await context.Response.WriteAsync(resultXML);
                }
                else
                {
                    await next.Invoke();
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        public string RegisterSitemap(IEnumerable<Dictionary<string, string>> sitemapItems)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("\n");
            sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.Append("\n");

            foreach (var item in sitemapItems)
            {
                sb.Append("  <url>");
                sb.Append("\n");

                sb.Append("    <loc>");
                sb.Append(item["loc"]);
                sb.Append("</loc>");
                sb.Append("\n");

                sb.Append("  </url>");
                sb.Append("\n");
            }
            sb.Append("</urlset>");

            return sb.ToString();
        }
    }

    public class AllWordsGetter
    {
        private List<Page> pages = new List<Page>();
        private List<Page> newPages = new List<Page>();
        private int randomLimit = 5;

        public IEnumerable<Page> getPages()
        {
            return pages;
        }

        public void addNewPages()
        {
            Random random = new Random();
            for (int i = 0; i < random.Next(1, randomLimit); i++)
            {
                if (newPages.Count() > 0)
                {
                    pages.Add(newPages[0]);
                    newPages.RemoveAt(0);
                }
            }
            pages = pages.OrderByDescending(p => p.referenceCount).ToList();
        }

        public void hurryToSetAllPages()
        {
            try
            {
                DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpPage, true); //開始記録

                var con = new DBCon();
                var allPages = new List<Page>();

                string sql = @"
select
wr1.wordId,
wr1.word,
wr1.cnt,
isnull(
	(select top(1) snippet from WordReferenceJp wr3 where wr3.sourceWordId = wr3.targetWordId and wr3.sourceWordId = wr1.wordId),
	(select top(1) snippet from WordReferenceJp wr2 where wr2.sourceWordId = wr1.wordId)
) as snippet
from (
		select w.wordId, w.word, wr.cnt from WordJp as w
		inner join (
			select targetWordId, count(targetWordId) cnt
			from WordReferenceJp
			group by targetWordId having count(targetWordId) > 4
		) as wr
		on w.wordId = wr.targetWordId
	) as wr1
;";

                var result = con.ExecuteSelect(sql);

                result.ForEach((e) =>
                {
                    var page = new Page();
                    page.wordId = (int)e["wordId"];
                    page.word = (string)e["word"];
                    page.referenceCount = (int)e["cnt"];
                    page.snippet = (string)e["snippet"];

                    allPages.Add(page);
                });

                pages = allPages.OrderByDescending(p => p.referenceCount).ToList();

                DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpPage, false); //終了記録
            }
            catch (Exception ex)
            {
                System.Threading.Thread.Sleep(1000 * 60);//DBへの負荷を考慮してSleep

                //DBにエラー内容書き出し
                var con = new DBCon();
                con.ExecuteUpdate(
                    "INSERT INTO Log VALUES (DATEADD(HOUR, 9, GETDATE()), @message);",
                    new Dictionary<string, object[]> { { "@message", new object[2] {
                        SqlDbType.NVarChar, "WikiWalksJp-hurryToSetAllPages Message: " + ex.Message + " StackTrace: " + ex.StackTrace
                    } } }
                );
                hurryToSetAllPages();
            }
        }


        public async Task setAllPagesAsync()
        {
            DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpPage, true); //開始記録

            var con = new DBCon();
            var allPages = new List<Page>();

            var min = 35;// 2020-08-01確認
            var max = (int)con.ExecuteSelect("select max(wordId) as max from WordJp;").FirstOrDefault()["max"];

            string sqlForCnt = "select count(targetWordId) as cnt from WordReferenceJp where targetWordId = @wordId;";
            string sqlForEachWord = @"
select
wr1.word,
isnull(
	(select top(1) snippet from WordReferenceJp wr3 where wr3.sourceWordId = wr3.targetWordId and wr3.sourceWordId = wr1.wordId),
	(select top(1) snippet from WordReferenceJp wr2 where wr2.sourceWordId = wr1.wordId)
) as snippet
from (
	select wordId, word
	from WordJp
	where wordId = @wordId
) as wr1
;";

            await Task.Delay(1000 * 10);
            for (var wordId = min; wordId <= max; wordId++)
            {
                var d = wordId - min;
                if (d < 1000)
                {
                    //前半に大きな負荷がかかっているように見受けられるため、前半の待機を長めに
                    await Task.Delay(2003 - (d * 2));
                }
                else
                {
                    await Task.Delay(3);
                }

                var count = (int)con.ExecuteSelect(
                        sqlForCnt,
                        new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } }
                        ).FirstOrDefault()["cnt"];

                if (count > 4)
                {
                    await Task.Delay(50);
                    Page page = new Page
                    {
                        wordId = wordId,
                        referenceCount = count
                    };

                    var resultForEachWord = con.ExecuteSelect(
                            sqlForEachWord,
                            new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, wordId } } }
                            );
                    var wordInfo = resultForEachWord.FirstOrDefault();
                    if (wordInfo != null)
                    {
                        page.word = (string)wordInfo["word"];
                        page.snippet = (string)wordInfo["snippet"];
                    }

                    allPages.Add(page);
                    await Task.Delay(50);
                }
            }

            int remainingNewPagesCount = newPages.Count();
            if (remainingNewPagesCount <= 0)
            {
                if (randomLimit > 1) {
                    randomLimit--;
                }
            }
            else
            {
                if (randomLimit < 5) {
                    randomLimit++;
                }
            }

            //前回と同じwordIdのページのみ更新（この時点では新規追加なし）
            pages = allPages
                .Where(p =>
                    pages.Any(oldPage => oldPage.wordId == p.wordId) || 
                    newPages.Any(oldPage => oldPage.wordId == p.wordId)
                )
                .OrderByDescending(p => p.referenceCount)
                .ToList();

            //新たに追加されているページを格納
            //（サイトマップへの問い合わせがある度に、上記のpagesに移していく）
            newPages = allPages
                .Where(p => !pages.Any(oldPage => oldPage.wordId == p.wordId))
                .ToList();

            DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpPage, false); //終了記録
        }
    }

    public class AllCategoriesGetter
    {
        private IEnumerable<Category> categories = new List<Category>();
        private AllWordsGetter allWordsGetter;

        public AllCategoriesGetter(AllWordsGetter allWordsGetter)
        {
            try
            {
                this.allWordsGetter = allWordsGetter;

                Task.Run(() =>
                {
                    allWordsGetter.hurryToSetAllPages();
                    System.Threading.Thread.Sleep(1000 * 5);//DBへの負荷を考慮して5秒Sleep
                    hurryToSetAllCategories();
                });

                Task.Run(async () =>
                {
                    await Task.Delay(1000 * 60 * 30);

                    while (true)
                    {
                        await Task.Delay(1000 * 60);

                        if (DateTime.Now.Minute == 0)
                        {
                            try
                            {
                                await allWordsGetter.setAllPagesAsync();
                            }
                            catch (Exception ex)
                            {
                                //
                            }

                            await Task.Delay(1000 * 60 * 5);

                            try
                            {
                                await setAllCategoriesAsync();
                            }
                            catch (Exception ex)
                            {
                                //
                            }

                            try
                            {
                                //バッチが動いてなければ起動
                                StartBatch();
                            }
                            catch (Exception ex)
                            {
                                //
                            }
                        }
                    }
                });
            }
            catch (Exception ex) { }
        }

        private async void StartBatch()
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(@"https://wiki-bat-jp.azurewebsites.net/");
                string msg = await response.Content.ReadAsStringAsync();
            }
        }

        public IEnumerable<Category> getCategories()
        {
            return categories;
        }

        private void hurryToSetAllCategories()
        {
            try
            {
                DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpCategory, true); //開始記録

                var con = new DBCon();
                var l = new List<Category>();

                var result = con.ExecuteSelect(@"
select category, count(*) as cnt 
from CategoryJp C
inner join (select targetWordId from WordReferenceJp group by targetWordId having count(targetWordId) > 4) as W
on W.targetWordId = C.wordId 
group by category
;");

                result.ForEach((e) =>
                {
                    var c = new Category();
                    c.category = (string)e["category"];
                    c.cnt = (int)e["cnt"];

                    l.Add(c);
                });

                categories = l.OrderByDescending(c => c.cnt).ToList();

                DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpCategory, false); //終了記録
            }
            catch (Exception ex)
            {
                System.Threading.Thread.Sleep(1000 * 60);//DBへの負荷を考慮してSleep

                //DBにエラー内容書き出し
                var con = new DBCon();
                con.ExecuteUpdate(
                    "INSERT INTO Log VALUES (DATEADD(HOUR, 9, GETDATE()), @message);",
                    new Dictionary<string, object[]> { { "@message", new object[2] {
                        SqlDbType.NVarChar, "WikiWalksJp-hurryToSetAllCategories Message: " + ex.Message + " StackTrace: " + ex.StackTrace
                    } } }
                );
                hurryToSetAllCategories();
            }
        }


        private async Task setAllCategoriesAsync()
        {
            DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpCategory, true); //開始記録

            var con = new DBCon();
            var l = new List<Category>();

            var pages = allWordsGetter.getPages().ToList();

            var hashCategories = new HashSet<string>();
            foreach (var page in pages)
            {
                await Task.Delay(10);
                con.ExecuteSelect(
                        "select category from CategoryJp where wordId = @wordId;",
                        new Dictionary<string, object[]> { { "@wordId", new object[2] { SqlDbType.Int, page.wordId } } }
                ).ForEach(cat =>
                        {
                            hashCategories.Add((string)cat["category"]);
                        });
            }

            await Task.Delay(1000 * 45);
            foreach (var cat in hashCategories)
            {
                await Task.Delay(10);

                var c = new Category();
                c.category = cat;

                c.cnt = con.ExecuteSelect(
                    "select wordId from CategoryJp where category like @category;",
                    new Dictionary<string, object[]> { { "@category", new object[2] { SqlDbType.NVarChar, c.category } } }
                    )
                .Count((a) => pages.Any(p => p.wordId == (int)a["wordId"]));

                if (c.cnt > 0)
                {
                    l.Add(c);
                }
            }

            categories = l.OrderByDescending(c => c.cnt).ToList();

            DB_Util.RegisterLastTopUpdate(DB_Util.procTypes.jpCategory, false); //終了記録
        }
    }
}