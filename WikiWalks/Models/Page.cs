using System;

namespace RelatedPages.Models
{
    public class Page
    {
        public int titleId { get; set; }
        public string title { get; set; }
        public DateTime publishDate { get; set; }
        public string pageName { get; set; }
        public string link { get; set; }
        public string explanation { get; set; }
    }
}