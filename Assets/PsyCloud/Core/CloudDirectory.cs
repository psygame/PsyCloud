using System.Collections.Generic;
using System.Threading.Tasks;

namespace PsyCloud
{
    public class CloudDirectory
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public string desc { get; private set; }

        public List<CloudFile> cachedFiles { get; private set; }
        public List<CloudDirectory> cachedDirectories { get; private set; }

        private LanzouClient client;
        private GetDirResponse.TextItem info;

        public CloudDirectory(LanzouClient client, GetDirResponse.TextItem info)
        {
            this.client = client;
            this.info = info;
            this.id = info.fol_id;
            this.name = info.name;
            this.desc = info.folder_des;
        }

        public async Task<List<CloudFile>> GetFiles(int page = 1)
        {
            if (cachedFiles == null)
            {
                var rep = await this.client.LsFilesAsync(info.fol_id, page);
                cachedFiles = new List<CloudFile>();
                if (rep.text != null)
                {
                    foreach (var info in rep.text)
                    {
                        cachedFiles.Add(new CloudFile(this.client, info));
                    }
                }
            }
            return cachedFiles;
        }

        public async Task<List<CloudDirectory>> GetDirectories(int page = 1)
        {
            if (cachedDirectories == null)
            {
                var rep = await this.client.LsDirAsync(info.fol_id);
                cachedDirectories = new List<CloudDirectory>();
                if (rep.text != null)
                {
                    foreach (var info in rep.text)
                    {
                        cachedDirectories.Add(new CloudDirectory(this.client, info));
                    }
                }
            }
            return cachedDirectories;
        }

        public void ClearCaches()
        {
            cachedDirectories = null;
            cachedFiles = null;
        }
    }
}
