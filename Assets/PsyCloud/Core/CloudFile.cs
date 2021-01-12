using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PsyCloud
{
    public class CloudFile
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public string desc { get; private set; }
        public string size { get; private set; }

        private LanzouClient client;
        private GetFilesResponse.TextItem info;

        private string url;
        private string durl;

        public CloudFile(LanzouClient client, GetFilesResponse.TextItem info)
        {
            this.client = client;
            this.info = info;
            this.id = info.id;
            this.name = info.name;
            this.size = info.size;
            this.desc = info.name_all;
        }

        public async Task<string> GetUrl()
        {
            if (url == null)
            {
                var rep = await client.GetShareUrl(info.id);
                url = rep.info.url;
            }
            return url;
        }

        public async Task<string> GetDurl()
        {
            var url = await GetUrl();
            if (durl == null)
            {
                var rep = await client.GetDurl(url);
                durl = rep.url;
            }
            return durl;
        }

        public async Task Download(string path, Action<float> downloadProgress = null)
        {
            var durl = await GetDurl();
            using (var client = new HttpClientDownloadWithProgress(durl, path, 8196, 1024 * 1024))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    UnityEngine.Debug.Log($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                };

                await client.StartDownload();
            }
        }

        public class HttpClientDownloadWithProgress : IDisposable
        {
            private readonly string _downloadUrl;
            private readonly string _destinationFilePath;
            private readonly int _buffSize;
            private readonly int _limitSizePerSec;

            private HttpClient _httpClient;

            public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

            public event ProgressChangedHandler ProgressChanged;

            public HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath, int buffSize = 8196, int limitSizePerSec = -1)
            {
                _downloadUrl = downloadUrl;
                _destinationFilePath = destinationFilePath;
                _buffSize = buffSize;
                _limitSizePerSec = limitSizePerSec;
            }

            public async Task StartDownload()
            {
                _httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };

                using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    await DownloadFileFromHttpResponseMessage(response);
            }

            private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                    await ProcessContentStream(totalBytes, contentStream);
            }

            private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
            {
                var totalBytesRead = 0L;
                var readCount = 0L;
                var buffer = new byte[_buffSize];
                var isMoreToRead = true;
                var startTicks = DateTime.Now.Ticks;
                using (var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, _buffSize, true))
                {
                    do
                    {
                        if (_limitSizePerSec != -1)
                        {
                            var limitSize = (DateTime.Now.Ticks - startTicks) / 10000L * _limitSizePerSec / 1000L;
                            if (totalBytesRead > limitSize)
                            {
                                await Task.Delay(1);
                                continue;
                            }
                        }

                        var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            isMoreToRead = false;
                            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                            continue;
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        totalBytesRead += bytesRead;
                        readCount += 1;

                        if (readCount % 100 == 0)
                        {
                            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        }
                    }
                    while (isMoreToRead);
                }
            }

            private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
            {
                if (ProgressChanged == null)
                    return;

                double? progressPercentage = null;
                if (totalDownloadSize.HasValue)
                    progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

                ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
            }

            public void Dispose()
            {
                _httpClient?.Dispose();
            }
        }
    }
}
