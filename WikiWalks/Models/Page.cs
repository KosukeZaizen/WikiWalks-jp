using System;

namespace RelatedPages.Models
{
    public class Word
    {
        public int wordId { get; set; }
        public string word { get; set; }
        public string snippet { get; set; }
        public DateTime lastEdit { get; set; }
        public IEquatable<Word> relatedWords { get; set; }
    }
}