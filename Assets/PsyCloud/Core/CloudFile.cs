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
        public long size { get; private set; }

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
            this.size = long.Parse(info.size);
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
                durl = rep;
            }
            return durl;
        }

        public async Task Save(string path, Action<float> downloadProgress = null)
        {
            var durl = await GetDurl();
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var n = response.Content.Headers.ContentLength;

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            using (var fileStream = new FileInfo(path).Create())
            {
                using (stream)
                {
                    byte[] buffer = new byte[1024];
                    var readLength = 0;
                    int length;
                    while ((length = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        readLength += length;
                        downloadProgress?.Invoke((float)readLength / (long)n);
                        fileStream.Write(buffer, 0, length);
                    }
                }
            }
        }
    }
}
