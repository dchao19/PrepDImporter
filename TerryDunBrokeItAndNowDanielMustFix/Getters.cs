using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerryDunBrokeItAndNowDanielMustFix
{
    class Getters
    {
    }

    public class Folder
    {
        public string name { get; set; }
        public int category { get; set; }
        public int teamId { get; set; }
        public long id { get; set; }
        public long parent { get; set; }
        public List<long> articles { get; set; }
    }

    public class GetArticle
    {
        public string title { get; set; }
        public int folders { get; set; }
    }

    public class Highlight
    {
        public int container { get; set; }
        public int fileId { get; set; }
        public int memberId { get; set; }
        public int rangeEnd { get; set; }
        public int rangeStart { get; set; }
        public int teamId { get; set; }
        public string token { get; set; }
        public int trashed { get; set; }
        public string data { get; set; }
    }

    public class AllHighlights
    {
        public List<Highlight> highlights { get; set; }
    }

}
