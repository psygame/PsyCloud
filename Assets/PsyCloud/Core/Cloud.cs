using System.Threading.Tasks;

namespace PsyCloud
{
    public class Cloud
    {
        public CloudDirectory root;
        private LanzouClient client;

        public async Task<bool> Login(string cookies)
        {
            client = new LanzouClient(cookies);
            root = new CloudDirectory(client, new GetDirResponse.TextItem() { fol_id = "-1", name = "Root", folder_des = "Root Directory" });
            await Task.Delay(1);
            return true;
        }
    }
}
