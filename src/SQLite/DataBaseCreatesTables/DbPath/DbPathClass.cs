namespace Messangers.SQLite.DataBaseCreatesTables.DbPath
{
    public class DbPathClass
    {
        public string dbpath()
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string dbPath = Path.Combine(projectDirectory, "DataBase.db");
            return dbPath;
        }
    }
}
