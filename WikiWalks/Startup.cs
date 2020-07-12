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

            var allWorsGetter = new AllWorsGetter();
            services.AddSingleton(allWorsGetter);

            var allCategoriesGetter = new AllCategoriesGetter();
            services.AddSingleton(allCategoriesGetter);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, AllWorsGetter allWorsGetter, AllCategoriesGetter allCategoriesGetter)
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
                    IEnumerable<string> allCategories = allCategoriesGetter.getCategories().Select(c => c.category);
                    foreach (var category in allCategories)
                    {
                        var dic2 = new Dictionary<string, string>();
                        dic2["loc"] = domain + "/category/" + HttpUtility.UrlEncode(category.Replace(" ", "_")).Replace("%27", "'");
                        lstSitemap.Add(dic2);
                    }

                    //word page
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

    public class AllWorsGetter
    {
        private IEnumerable<Page> pages = new List<Page>();
        public AllWorsGetter()
        {
            pages = new List<Page>();

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(1000 * 60);

                        int min = DateTime.Now.Minute % 20;
                        if (min == 0)
                        {
                            await setAllPagesAsync();
                        }
                    }
                    catch (Exception ex) { }
                }
            });

            setAllPagesAsync();
        }

        public IEnumerable<Page> getPages()
        {
            return pages;
        }

        private async Task setAllPagesAsync()
        {
            Action proc = () =>
            {
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
order by wr1.cnt desc;
";

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

                pages = allPages;
            };

            await DB_Util.runHeavySqlAsync(proc);
        }
    }

    public class AllCategoriesGetter
    {
        private IEnumerable<Category> categories = new List<Category>();
        public AllCategoriesGetter()
        {
            categories = new List<Category>();

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(1000 * 60);

                        int min = DateTime.Now.Minute % 20;
                        if (min == 5)
                        {
                            await setAllCategoriesAsync();
                        }
                    }
                    catch (Exception ex) { }
                }
            });

            setAllCategoriesAsync();
        }

        public IEnumerable<Category> getCategories()
        {
            return categories;
        }

        private async Task setAllCategoriesAsync()
        {
            Action proc = () =>
            {
                var con = new DBCon();
                var l = new List<Category>();

                var result = con.ExecuteSelect(@"
select category, count(*) as cnt 
from CategoryJp C
inner join (select targetWordId from WordReferenceJp group by targetWordId having count(targetWordId) > 4) as W
on W.targetWordId = C.wordId 
group by category
order by cnt desc;
");

                result.ForEach((e) =>
                {
                    var c = new Category();
                    c.category = (string)e["category"];
                    c.cnt = (int)e["cnt"];

                    l.Add(c);
                });

                categories = l;
            };

            await DB_Util.runHeavySqlAsync(proc);
        }
    }
}