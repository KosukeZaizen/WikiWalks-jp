using System.Collections.Generic;

namespace RelatedPages.Models
{
    public class Page
    {
        public int wordId { get; set; }
        public string word { get; set; }
        public string snippet { get; set; }
        public IEnumerable<string> categories { get; set; }
    }
}