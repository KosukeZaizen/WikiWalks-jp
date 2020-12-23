using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using WikiWalks;

namespace Z_Apps.Models.SystemBase
{
    public class SiteMapService
    {
        private readonly AllWordsGetter allWorsGetter;
        private readonly AllCategoriesGetter allCategoriesGetter;
        private List<IEnumerable<Dictionary<string, string>>> sitemapChunks;

        public SiteMapService(AllWordsGetter allWorsGetter, AllCategoriesGetter allCategoriesGetter)
        {
            this.allWorsGetter = allWorsGetter;
            this.allCategoriesGetter = allCategoriesGetter;

            //��������sitemapChunks�̏����l�𐶐�
            var result = GetSiteMapText(false, 0);
        }


        public string GetSiteMapText(
            bool onlyStrageXmlFile = false,
            //�e�t�@�C���̔z�u�������Ă���XML��0�ԁA����ȍ~��1�ԁ`...
            int sitemapNumber = 0
        )
        {
            //Startup.cs��Sitemap���N�G�X�g���̏����ƁA
            //�T�C�g�}�b�v�ҏW��ʂ̓��e���X�g���[�W�ɓo�^���鏈���̗�������Ă΂��
            using (var client = new HttpClient())
            {

                if (sitemapNumber == 0)
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
                    //�J�e�S���y�[�W��noindex�Ƃ��A���O
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


                    //------------------------------------------------------------
                    //�T�C�g�}�b�v�̕���
                    var chunkSize = 30000;
                    sitemapChunks = lstSitemap.Select((v, i) => new { v, i })
                                            .GroupBy(x => x.i / chunkSize)
                                            .Select(g => g.Select(x => x.v))
                                            .ToList();

                    var result = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                    for (var i = 1; i < sitemapChunks.Count() + 1; i++)
                    {
                        result.Append("<sitemap><loc>");
                        result.Append(domain + "/sitemap" + i + ".xml");
                        result.Append("</loc></sitemap>");
                    }
                    result.Append("</sitemapindex>");
                    return result.ToString();
                }
                else
                {
                    string baseXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></urlset>";
                    string partialXML = GetStringSitemapFromDics(sitemapChunks[sitemapNumber - 1]);
                    return baseXML.Replace("</urlset>", partialXML + "</urlset>");
                }
            }
        }


        private string GetStringSitemapFromDics(IEnumerable<Dictionary<string, string>> sitemapItems)
        {
            StringBuilder sb = new StringBuilder();

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

            return sb.ToString();
        }
    }
}