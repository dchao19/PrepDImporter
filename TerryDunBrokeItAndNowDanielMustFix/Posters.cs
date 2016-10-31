using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerryDunBrokeItAndNowDanielMustFix
{
    public class Posters
    {
     
    }

    public class NewSubFolder
    {
        public string folderName { get; set; }
        public int parent { get; set; }
        public int folderCategory { get; set; }
        public long teamId { get; set; }
    }

    public class Article
    {
        public DocumentData documentData { get; set; }
        public List<int> teams { get; set; }
    }

    public class DocumentData
    {
        public string title { get; set; }
        public string publication { get; set; }
        public string url { get; set; }
        public string htmlContent { get; set; }
        public bool fav { get; set; }
        public List<string> folders { get; set; }
        public List<string> subfolders { get; set; }
    }


}
