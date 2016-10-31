using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TerryDunBrokeItAndNowDanielMustFix
{
    public partial class Form1 : Form
    {
        List<Folder> existingFolders = new List<Folder>();
        List<Highlight> highlights = new List<Highlight>();
        HttpClient httpC;
        Dictionaries dictionaries = new Dictionaries();
        public Form1()
        {
            InitializeComponent();
            httpC = GetHttpClient("PREPD_USERNAME","PREPD_PASSWORD");
        }

        private void LoadExistingFolders()
        {
            using (StreamReader r = new StreamReader(@"C:\Users\Daniel\Desktop\PREPD\AllFolders"))
            {
                 string json = r.ReadToEnd();
                JObject parsed = JObject.Parse(json);
                foreach (var parsedFolder in parsed)
                {
                    Folder deFoldered = (JsonConvert.DeserializeObject<Folder>(parsedFolder.Value.ToString()));
                    if (deFoldered.id == 418535 || deFoldered.id == 445189 || deFoldered.id == 418026 || deFoldered.id == 419027
                        || deFoldered.id == 419711 || deFoldered.id == 419672 || deFoldered.id == 419756 || deFoldered.id == 420851
                        || deFoldered.id == 424610 || deFoldered.id == 424567 || deFoldered.id == 424900 || deFoldered.id == 424852
                        || deFoldered.id == 424954 || deFoldered.id == 425177 || deFoldered.id == 425360 || deFoldered.id == 425507
                        || deFoldered.id == 425444 || deFoldered.id == 425506 || deFoldered.id == 425467 || deFoldered.id == 425408
                        || deFoldered.id == 425541 || deFoldered.id == 425468 || deFoldered.id == 425409 || deFoldered.id == 419028
                        || deFoldered.id == 439825 || deFoldered.id == 440476 || deFoldered.id == 442229 || deFoldered.id == 365214)
                    { existingFolders.Add(deFoldered); }
                }
            }
        }


        private HttpClient GetHttpClient(String username, String password)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            return httpClient;
        }

        private void DoLogin()
        {

            HttpResponseMessage response = httpC.GetAsync("https://sandbox-v2.herokuapp.com/v2/login/catcher").Result;
            JObject parsed = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            if (parsed["done"].ToString() != "OK") throw new Exception("LOGIN FAILURE!!!!!!!");
            else Console.WriteLine("Login Successful!");        

        }
        private void createAndMatchFolders()
        {
            HttpContent contentPost = new StringContent("{\"folderName\":\"" + existingFolders[0].name + " - ReUpload" + "\",\"folderCategory\":0,\"teamId\":43703}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = httpC.PostAsync("https://sandbox-v2.herokuapp.com/v2/folders/new", contentPost).Result;
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var resultObject = result["result"];
            foreach(var property in resultObject)
            {
                foreach(var info in property)
                {
                    if(!dictionaries.newIdsOldIds.ContainsKey(existingFolders[0].id.ToString()))dictionaries.newIdsOldIds.Add(existingFolders[0].id.ToString(),(string)(info.SelectToken("id")));
                    dictionaries.oldIdsPaths.Add(existingFolders[0].id, (string)info.SelectToken("name"));     
                }
               
            }
            foreach (Folder folder in existingFolders)
            {
                if(existingFolders.IndexOf(folder) == 0) continue;
                        
                NewSubFolder newSub = new NewSubFolder();
                newSub.folderName = folder.name;
                newSub.folderCategory = folder.category;
                newSub.parent = Int32.Parse(dictionaries.newIdsOldIds[folder.parent.ToString()]);
                newSub.teamId = folder.teamId;
                HttpContent contentPostSub = new StringContent(JsonConvert.SerializeObject(newSub), Encoding.UTF8, "application/json");
                HttpResponseMessage responseSub = httpC.PostAsync("https://sandbox-v2.herokuapp.com/v2/folders/subfolder", contentPostSub).Result;
                JObject resultSub = JObject.Parse(responseSub.Content.ReadAsStringAsync().Result);
                var resultObjectSub = resultSub["result"];
                if (!dictionaries.newIdsOldIds.ContainsKey(existingFolders[0].id.ToString())) dictionaries.newIdsOldIds.Add(folder.id.ToString(),resultObjectSub.SelectToken("id").ToString());
                dictionaries.oldIdsPaths.Add(folder.id, ("United States - Domestic - ReUpload/" + folder.name));
            }
        }
        private List<string> foldersinArticles(long id)
        {
            List<string> temp = new List<string>();
            foreach(Folder folder in existingFolders)
            {
                foreach(long article in folder.articles)
                {
                    if (id == article) {
                        
                        temp.Add(dictionaries.oldIdsPaths[folder.id]);
                        //get id back to name
                    };
                }
            }
            return temp;
        }
        private void loadArticles()
        {
            using(StreamReader r = new StreamReader(@"C:\Users\Daniel\Desktop\PREPD\AllArticles"))
            {
                string json = r.ReadToEnd();
                JObject parsed = JObject.Parse(json);
                int numDone = 0;
                foreach(var article in parsed)
                {
                    
                    long deFoldered = (long)article.Value.SelectToken("id");
                    List<string> longs = foldersinArticles((long)article.Value.SelectToken("id"));
                    if (longs.Count > 0)
                    {
                        numDone++;
                        DocumentData dData = JsonConvert.DeserializeObject<DocumentData>(article.Value.ToString());
                        dData.publication = dictionaries.publicationIds[(long)(article.Value.SelectToken("publicationId"))];
                        dData.subfolders = foldersinArticles((long)(article.Value.SelectToken("id")));
                        if (foldersinArticles((long)(article.Value.SelectToken("id")))[0].Equals("United States - Domestic - ReUpload")) dData.folders = foldersinArticles((long)(article.Value.SelectToken("id")));
                        String docContent = article.Value.SelectToken("documentContent").SelectToken("body").ToString();
                        String replaced = docContent.Replace("\n", "<br />");
                        //Console.WriteLine(replaced + replaced.IndexOf("\n"));
                        dData.htmlContent = "<html><head><title>" + dData.title + "</title></head><body><p>" + replaced + "</p></body></html>";
                        Article toPost = new Article();
                        toPost.documentData = dData;
                        dData.folders = new List<string>(1);
                        toPost.teams = new List<int>();
                        toPost.teams.Add(43703);
                        //Console.WriteLine("done");
                        //Console.WriteLine(JsonConvert.SerializeObject(toPost));
                        HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(toPost), Encoding.UTF8, "application/json");
                        HttpResponseMessage response = httpC.PostAsync("https://sandbox-v2.herokuapp.com/v2/widget/catch", contentPost).Result;
                        var httpresponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        string jobName = (string)httpresponse["result"].SelectToken("token");
                        //lol highlights
                        var highlightsfromarticle = article.Value["highlights"];
                        if (highlightsfromarticle != null)
                        {
                            foreach (var highlight2 in highlightsfromarticle)
                            {
                                
                                    Highlight high = JsonConvert.DeserializeObject<Highlight>(highlight2.First.ToString());
                                    List<string> paragraphs = docContent.Split(new String[] { "\n" }, StringSplitOptions.None).ToList();
                                    if (paragraphs[high.container].Length < high.rangeEnd - high.rangeStart - 4) continue;
                                    else if (high.fileId == 1524062 || high.fileId == 1743228 || high.fileId == 1831597 || high.fileId == 1906801 || high.fileId == 1963175 || high.fileId == 2010612) continue;
                                    else high.data = (paragraphs[high.container].Substring(high.rangeStart + 1, (high.rangeEnd - high.rangeStart - 1)));
                                    if (dictionaries.newIdsOldIds.ContainsKey(article.Value.SelectToken("id").ToString())) high.fileId = Int32.Parse(dictionaries.newIdsOldIds[article.Value.SelectToken("id").ToString()]);
                                    else continue;
                                    high.token = SHA1Util.SHA1HashStringForUTF8String(high.fileId + high.container + high.rangeStart + high.rangeEnd + high.data);
                                    highlights.Add(high);
                                
                                    
                                

                            }
                        }
                        
                        if (httpresponse["message"].ToString() == "alreadyCaught")
                        {
                            Console.WriteLine("MOVING ON");
                            Console.WriteLine(httpresponse["message"].ToString());
                            continue;
                        }
                        
                        
                        
                        string notyet = "notYet";
                        int counter = 0;
                        int articleId = 0;
                        while (notyet == "notYet" && counter <= 50)
                        {
                            counter++;
                            HttpContent contentPostJob = new StringContent("{\"jobName\":\"" + jobName + "\"}", Encoding.UTF8, "application/json");
                            HttpResponseMessage responseJob2 = httpC.PostAsync("https://sandbox-v2.herokuapp.com/v2/jobs/status", contentPostJob).Result;
                            Console.WriteLine(responseJob2.Content.ReadAsStringAsync().Result);
                            String responseString = responseJob2.Content.ReadAsStringAsync().Result;
                            var responseObject = JObject.Parse(responseString);
                            notyet = responseObject["message"].ToString();
                            if (notyet != "notYet")
                            {
                                articleId = (int)responseObject.SelectToken("result").SelectToken("message");
                                dictionaries.newIdsOldIds.Add(article.Value.SelectToken("id").ToString(), responseObject.SelectToken("result").SelectToken("message").ToString());
                            }
                            Thread.Sleep(2000);
                        }

                        HttpContent contentPostUpdate = new StringContent("{\"articleId\":\"" + articleId + "\",\"teamId\":43703,\"newDate\":" + (article.Value.SelectToken("published")) + "}", Encoding.UTF8, "application/json");
                        HttpResponseMessage responseUpdate = httpC.PostAsync("https://sandbox-v2.herokuapp.com/v2/documents/change_date", contentPostUpdate).Result;
                        Console.WriteLine(responseUpdate.Content.ReadAsStringAsync().Result);
                        Console.WriteLine("done");
                    }
                }
                Console.WriteLine("PARSED: " + numDone);

            }
        }

        private string getUrlSource(String url)
        {
            using (WebClient client = new WebClient())
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(client.DownloadString(url));
                doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "iframe")
                .ToList()
                .ForEach(n => n.Remove());
                return doc.DocumentNode.SelectSingleNode("//body").InnerHtml;
 
            }
        }

        private void loadPublications()
        {
            using(StreamReader r = new StreamReader(@"C:\Users\Daniel\Desktop\PREPD\AllPublications")){
                string json = r.ReadToEnd();
                JObject parsed = JObject.Parse(json);
                foreach(var publication in parsed)
                {
                    if (!dictionaries.publicationIds.ContainsKey(Int64.Parse(publication.Key))) dictionaries.publicationIds.Add(Int64.Parse(publication.Key), publication.Value.SelectToken("name").ToString());
                }
            }
        }
        private void postHighlights()
        {

            AllHighlights allH = new AllHighlights();
            allH.highlights = highlights;
            Console.WriteLine(JsonConvert.SerializeObject(allH));
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(allH), Encoding.UTF8, "application/json");
            HttpResponseMessage responseUpdate = httpC.PostAsync("https://sandbox-v2.herokuapp.com/v2/highlight/create", contentPost).Result;
            Console.WriteLine(responseUpdate.Content.ReadAsStringAsync().Result);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            LoadExistingFolders();
            loadPublications();
            DoLogin();
            createAndMatchFolders();
            loadArticles();
            postHighlights();
            Console.WriteLine("tery");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
