using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupRandomizer.Models;
using LiteDB;

namespace GroupRandomizer
{
    public class RosterRepository
    {
        string _dbPath;
        public string StatusMessage { get; set; }

        private LiteDatabase db;
        public ILiteCollection<Roster> col;

        private void Init()
        {
            if (db!= null)
                return;

            db = new LiteDatabase(_dbPath);
            col = db.GetCollection<Roster>("rosters");
        }

        public RosterRepository(string dbPath)
        {
            _dbPath = dbPath;
        }

        public void AddNewRoster(string name)
        {
            try
            {
                Init();

                if (string.IsNullOrEmpty(name))
                    throw new Exception("Valid name required");

                col.Insert(new Roster { Name = name });

                StatusMessage = string.Format("Added New Roster: {0}", name);
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Failed to add {0}. Error: {1}", name, ex.Message);
            }
        }

        public List<Roster> GetAllRosters()
        {
            try
            {
                Init();
                return col.FindAll().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Failed to retrieve data. {0}", ex.Message);
            }

            return new List<Roster>();
        }

        public void AddPersonToRoster(int rosterId, string name)
        {
            try
            {
                Init();

                if (string.IsNullOrEmpty(name))
                    throw new Exception("Valid name required");

                var roster = col.FindById(rosterId);
                roster.People.Add(name);
                col.Update(roster);
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Failed to update roster. {0}", ex.Message);
            }
        }
    }
}
