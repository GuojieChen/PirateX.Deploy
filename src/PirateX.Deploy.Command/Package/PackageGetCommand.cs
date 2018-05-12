using Akka.Configuration.Hocon;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PirateX.Deploy.Command
{
    [CommandName("package-get",Description ="获取包")]
    public class PackageGetCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("package-get command param error");
            }
            var feedName = hobj.GetKey("FeedName").GetString();
            var groupName = hobj.GetKey("GroupName")?.GetString() ?? "";
            var packageName = hobj.GetKey("PackageName").GetString();
            var version = hobj.GetKey("Version")?.GetString() ?? "latest";
            var credential = hobj.GetKey("Credential")?.GetString();
            var progetSource = hobj.GetKey("Source")?.GetString() ;
            var target = hobj.GetKey("Target")?.GetString();//目标路径
            var downloadTask = DownLoadPackage(feedName, groupName, packageName, version, credential, progetSource);
            return downloadTask.Result;
        }

        internal async Task<string> DownLoadPackage(string feedName, string groupName, string packageName, string version, string credential, string source)
        {
            string dir = Path.Combine(CommandExecutor.WorkSpace, "Packages");//$"{Runner.DefaultFilePath}Packages\\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (version != "latest")//用名称检查文件是否存在
            {
                string zipName = $"{packageName}-{version}.zip";
                string zipPath = Path.Combine(dir, zipName);//$"{dir}{zipName}";
                if (File.Exists(zipPath))
                {
                    return $"file {zipName} existed, need no download";
                }
            }

            string query = $"{version}?contentOnly=zip"; ;
            if (version == "latest")
            {
                query = $"?{version}&contentOnly=zip";
            }

            string packagePath = packageName;
            if (!string.IsNullOrEmpty(groupName))
            {
                packagePath = groupName + "/" + packageName;
            }

            string url = $"{source}{feedName}/download/{packagePath}/{query}";
            var userData = credential.Split(':');
            string name = userData[0];
            string password = userData[1];
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(name, password),
                UseDefaultCredentials = false,
                PreAuthenticate = true,
                UseProxy = false
            };
            var httpClient = new HttpClient(handler);
            string result = await DownloadFileAsync(httpClient, url, dir);
            return result;
        }

        public async Task<string> DownloadFileAsync(HttpClient client, string url, string dir)
        {
            var token = new CancellationTokenSource();

            base.Send(url);
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"The request returned with HTTP status code {response.StatusCode}");
            }

            var total = response.Content.Headers.ContentLength ?? -1L;
            if (total == -1)
            {
                throw new Exception($"response content length is -1, can not download package!");
            }

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isComplete = false;
                string fileName = response.Content.Headers.ContentDisposition.FileName.TrimStart('\"').TrimEnd('\"');

                string filePath = Path.Combine(dir, fileName);//$"{dir}{fileName}";
                if (File.Exists(filePath))
                {
                    return $"file {fileName} existed, need no download";
                }
                Send("$Prepare Progress Bar$");

                using (var memoryStream = new MemoryStream(1000 * 1000 * 4))
                {
                    do
                    {
                        token.Token.ThrowIfCancellationRequested();
                        int timeout = 1000 * 60 * 3; //3分钟
                        var read = await ReadAsync(stream, buffer, 0, buffer.Length, timeout);
                        if (read == 0)
                        {
                            break;
                        }

                        memoryStream.Write(buffer, 0, read);
                        totalRead += read;

                        var progress = (totalRead * 1d) / (total * 1d) * 100;
                        isComplete = Math.Abs(100.0 - progress) < 1e-6;
                        if (isComplete) //确定下载完成了再创建文件并写入字节流
                        {
                            using (var fileStream =
                                new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                            {
                                var arr = memoryStream.ToArray();
                                fileStream.Write(arr, 0, arr.Length);
                            }
                        }
                        Send(progress.ToString("F"),false);
                        if (isComplete) //下载完成跳出循环
                        {
                            break;
                        }
                    } while (true);
                }
                Send("$Finish Progress Bar$");
                Thread.Sleep(300);
                if (isComplete)
                {
                    return $"download {fileName} success!";
                }
                return $"error: download {fileName} failure!";
            }
        }

        /// <summary>
        /// 异步读取超时封装处理
        /// </summary>
        public async Task<int> ReadAsync(Stream stream, byte[] buffer, int offset, int count, int timeout)
        {
            var reciveCount = 0;
            var receiveTask = Task.Run(async () => { reciveCount = await stream.ReadAsync(buffer, offset, count); });
            var isReceived = await Task.WhenAny(receiveTask, Task.Delay(timeout)) == receiveTask;
            if (!isReceived) throw new Exception("http request timeout!");
            return reciveCount;
        }
    }
}
