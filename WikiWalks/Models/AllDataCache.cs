using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

namespace RelatedPages.Models {

    public class AllDataCache {

        public enum Keys {
            WikiJpCategory,
            WikiJpPages
        }

        private static string GetKeyString(Keys key) {

            switch (key) {
                case Keys.WikiJpCategory:
                    return "WikiJpCategory";
                case Keys.WikiJpPages:
                    return "WikiJpPages";
            }
            return "";
        }

        public static void SaveCache(Keys key, object data) {

            var con = new DBCon();

            var json = System.Text.Json.JsonSerializer.Serialize(data);

            con.ExecuteUpdate(@"
update AllDataCache
set cacheData = @json
where cacheKey = @key
;",
                new Dictionary<string, object[]> {
                            { "@json", new object[2] { SqlDbType.NVarChar, json } },
                            { "@key", new object[2] { SqlDbType.NVarChar, GetKeyString(key) } }
                });
        }


        public static List<Category> GetCacheCategory() {
            try {
                var con = new DBCon();

                var result = con.ExecuteSelect(@"
select cacheData 
from AllDataCache 
where cacheKey = N'WikiJpCategory'
;"
                , null)
                .FirstOrDefault();

                if (result != null) {
                    return JsonConvert
                        .DeserializeObject<List<Category>>(
                            (string)result["cacheData"]
                        );
                }
            } catch (Exception ex) { }
            return null;
        }


        public static List<Page> GetCachePage() {
            try {
                var con = new DBCon();

                var result = con.ExecuteSelect(@"
select cacheData 
from AllDataCache 
where cacheKey = N'WikiJpPages'
;"
                , null)
                .FirstOrDefault();

                if (result != null) {
                    return JsonConvert
                                .DeserializeObject<List<Page>>(
                                    (string)result["cacheData"]
                                );
                }
            } catch (Exception ex) {
                var e = ex;
            }
            return null;
        }
    }
}