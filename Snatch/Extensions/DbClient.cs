using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Snatch
{
  public class DbClient : DbContext
  {
    public DbSet<Entry> Entries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string fullPath = $"{localPath}\\Snatch";
      string destination = $"{fullPath}\\Snatch.db";

      Debug.WriteLine($"destination: {destination}");

      if (!Directory.Exists(fullPath))
      {
        Directory.CreateDirectory(fullPath);
      }

      if (!File.Exists(destination))
      {
        string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
        int index = location.LastIndexOf("\\");
        string source = $"{location.Substring(0, index)}\\Snatch.db";
        Debug.WriteLine($"source: {source}");
        File.Copy(source, destination);
      }

      var cs = new SqliteConnectionStringBuilder { DataSource = destination, Password = AskPassword(destination) }.ToString();
      var connection = new SqliteConnection(cs);
      optionsBuilder.UseSqlite(connection);
      base.OnConfiguring(optionsBuilder);
    }

    private string AskPassword(string destination)
    {
        string password;
        while (true)
        {
            try
            {
                var pw = new Snatch.Windows.Password();
                pw.ShowDialog();
                password = pw.TxtPassword.Password;
                var cs = new SqliteConnectionStringBuilder { DataSource = destination, Password = password }.ToString();
                var connection = new SqliteConnection(cs);
                connection.Open();
                //var command = connection.CreateCommand();
                //command.CommandText = "PRAGMA rekey = foobar";
                //command.ExecuteNonQuery();
                break;
            }
            catch
            {
                // ignored
            }
        }
        return password;
    }
  }
}
