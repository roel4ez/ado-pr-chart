using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;
using QuickChart;

namespace ADOPRChart
{
    public class Options
    {
        [Option('f', "file", Required = false, HelpText = "Load config from appsettings file. Make sure there is an appsettings.json file available.", Default = false, SetName = "file")]
        public bool LoadFromFile { get; set; }

        [Option('o', "ado-org", Required = false, HelpText = "ADO Organization (string)", SetName = "manual")]
        public string AdoOrganization { get; set; }

        [Option('p', "ado-proj", Required = false, HelpText = "ADO Project (string)", SetName = "manual")]
        public string AdoProject { get; set; } 

        [Option('r', "ado-repo", Required = false, HelpText = "ADO Repository ID (guid)", SetName = "manual")]
        public string AdoRepository { get; set; }

        [Option('a', "ado-pat", Required = false, HelpText = "ADO Private Access Token (will not be stored)", SetName = "manual")]
        public string AdoAccessToken { get; set; }

        [Option('s', "pr-status", Required = false, HelpText = "Status of PR (default 'Completed')", SetName = "manual")]
        public string PullRequestStatus { get; set; }

        [Option('c', "page-size", Required = false, HelpText = "Page Size (default '100')", SetName = "manual")]
        public int PageSize { get; set; }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("ado-pr-chart");  
            var parser= new Parser();
            var result = parser.ParseArguments<Options>(args);

            await result.MapResult(async x =>
                     {
                         if (x.LoadFromFile)
                         {
                             var file = JObject.Parse(File.ReadAllText("appsettings.json"));
                             x.AdoOrganization = (string)file.SelectToken("ado-org");
                             x.AdoProject = (string)file.SelectToken("ado-proj");
                             x.AdoRepository = (string)file.SelectToken("ado-repo");
                             x.AdoAccessToken = (string)file.SelectToken("ado-pat");
                             x.PullRequestStatus = (string)file.SelectToken("pr-status");
                             x.PageSize = (int)file.SelectToken("page-size");
                         }

                         var response = await DoCall(x);

                         DoParse(response);
                     }, errors => Task.FromResult(0)); //todo: make sure this show the help instead
        }

        private static Task<string> DoCall(Options o)
        {
            var client = new HttpClient();
            var baseUri = new Uri($"https://dev.azure.com/{o.AdoOrganization}/{o.AdoProject}");
            client.BaseAddress = baseUri;

            var byteArray = Encoding.UTF8.GetBytes($":{o.AdoAccessToken}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var getUri = $"{baseUri}/_apis/git/pullrequests?api-version=6.1-preview.1&searchCriteria.repositoryId={o.AdoRepository}&searchCriteria.status={o.PullRequestStatus}&$top={o.PageSize}";
            
            Console.WriteLine(getUri);
            
            var response = client.GetStringAsync(getUri);

            return response;
        }

        private static void DoParse(string json)
        {
            JObject o = JObject.Parse(json);

            var allPrDates = o.SelectToken("value")
                            .Select(p => DateTime.Parse((string)p.SelectToken("closedDate")))
                            .Where(d => d > new DateTime(2020,11,11))
                            .OrderBy(d => d);
            var total = allPrDates.Count();

            var groups = allPrDates.GroupBy(p => p.ToString("MMM")).Select(n => new {Month = n.Key, Count = n.Count()});

            var labelString = string.Join(",", groups.Select(g => "'" + g.Month + "'"));
            var dataString = string.Join(",", groups.Select(g => g.Count));
            Chart qc = new Chart();

            qc.Width = 300;
            qc.Height = 300;
            
            qc.Config = $"{{type:'doughnut',data:{{labels:[{labelString}],datasets:[{{data:[{dataString}]}}]}},options:{{plugins:{{ {dataLabels},{doughnutlabels.Replace("%%total%%",total.ToString())} }}}}}}";

            
            Console.WriteLine(qc.GetShortUrl());
        }

        private static string dataLabels = @" datalabels: {
                                                display: true,
                                                align: 'center',
                                                borderRadius: 3,
                                                color: '#FFFFFF',
                                                font: {
                                                size: 18,
                                                style: 'bold'
                                                }
                                            }";

        private static string doughnutlabels = @"doughnutlabel: {
                                                labels: [{
                                                text: '%%total%%',
                                                font: {
                                                    size: 20,
                                                    weight: 'bold'
                                                }
                                                }, {
                                                text: 'total'
                                                }]
                                            }";

       
        

    }
}
