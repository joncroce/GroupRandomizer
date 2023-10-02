using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GroupRandomizer.Services
{
    public class RosterService
    {
        private readonly string _rosterDir;

        public RosterService()
        {
            string mainDir = FileSystem.Current.AppDataDirectory;
            _rosterDir = Path.Combine(mainDir, "Roster");
            if (!Directory.Exists(_rosterDir))
            {
                Directory.CreateDirectory(_rosterDir);
            }
        }

        public async Task<List<string>> GetRosterAsync(string file)
        {
            string jsonText = await Task.Run(() => File.ReadAllText(Path.Combine(_rosterDir, file)));
            return JsonConvert.DeserializeObject<List<string>>(jsonText);
        }

        public async Task<List<string>> GetRosterFilesAsync()
        {
            string[] fullPaths = await Task.Run(() => Directory.GetFiles(_rosterDir));
            List<string> fileNames = fullPaths.Select(Path.GetFileName).ToList();

            return fileNames;
        }

        public async Task SaveRosterAsync(string name, List<string> value)
        {
            string jsonText = JsonConvert.SerializeObject(value);
            await Task.Run(() => File.WriteAllText(Path.Combine(_rosterDir, name), jsonText));
        }
    }

}