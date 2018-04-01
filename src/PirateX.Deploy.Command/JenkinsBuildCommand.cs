using Akka.Configuration.Hocon;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PirateX.Deploy.Command
{
    [CommandName("jenkins-build")]
    public class JenkinsBuildCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("jenkins-build command param error");
            }
            var source = hobj.GetKey("Source")?.GetString();
            var job = hobj.GetKey("JobName").GetString();
            var credential = hobj.GetKey("Credential")?.GetString();

            //http://192.168.1.94:8080/job/baby/view/dev/job/gameserver/build?delay=0sec
            //http://192.168.1.94:8080/job/baby/view/dev/job/gameserver/lastBuild/api/json?tree=result,executor[progress]

            var buildTask = PostJenkinsBuild(source, job, credential);
            string buildResponse = buildTask.Result;

            if (buildResponse == "")
            {
                Send("$Prepare Progress Bar$");
                bool building = true;
                while (building)
                {
                    var task = GetProgressMessage(source, job, credential);
                    var msg = task.Result;
                    var model = JsonConvert.DeserializeObject<ProgressModel>(msg);
                    if(model == null)
                        continue;
                    if (model.result == "SUCCESS")
                    {
                        Send("100");
                        building = false;
                    }
                    else
                    {
                        int progress = model.executor.progress;
                        Send(progress.ToString());
                    }
                    Thread.Sleep(200);
                }
                Send("$Finish Progress Bar$");
            }
            else
            {
                throw new Exception(buildResponse);
            }
            Thread.Sleep(400);
            return $"build {job} success!";
        }

        private async Task<string> GetProgressMessage(string source, string job, string credential)
        {
            var handler = new HttpClientHandler
            {
                UseProxy = false //防止开了lantern等代理之后产生死锁
            };
            var client = new HttpClient(handler);

            var byteArray = Encoding.ASCII.GetBytes(credential);
            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Authorization = header;
            string progressUrl =
                $"http://{source}/job/{job}/lastBuild/api/json?tree=result,executor[progress]";
            var httpMessge = await client.GetAsync(progressUrl);
            string progressMsg = await httpMessge.Content.ReadAsStringAsync();
            return progressMsg;
        }

        private async Task<string> PostJenkinsBuild(string source, string job, string credential)
        {
            var handler = new HttpClientHandler()
            {
                UseProxy = false //防止开了lantern等代理之后产生死锁
            };
            var client = new HttpClient(handler);
            var byteArray = Encoding.ASCII.GetBytes(credential);
            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Authorization = header;
            string buildUrl = $"http://{source}/job/{job}/build?delay=0sec";
            var response = await client.PostAsync(buildUrl, null);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
    public class ProgressModel
    {
        public string _class { get; set; }

        public Executor executor { get; set; }

        public string result { get; set; }

    }

    public class Executor
    {
        public int progress { get; set; }

    }
}
