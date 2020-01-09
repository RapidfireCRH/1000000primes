using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace _100primes
{
    class dbclass
    {

        public struct dbstats_st
        {
            public bool squarestoobig;
            public string squarestoobigstart;
            public string databasesize;
            public string largestnum;
        }
        /// <summary>
        /// A collection of dbstats
        /// </summary>
        public static dbstats_st dbstats = new dbstats_st();

        private static List<string> writer = new List<string>();
        private static List<string> dbcache = new List<string>();
        private static double largest = 0;
        private static bool _init = false; // init variable
        private static SQLiteConnection m_dbConnection; // global connection variable
        private static double number = 1;
        private static int writerlimit = 1000;

        /// <summary>
        /// Opens a new db connection
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="filename">filename to open/ create</param>
        /// <param name="_override">overwrite existing database connection</param>
        public static void open(string path, string filename, bool _override = false)
        {
            if (m_dbConnection != null)
            {
                if (!(m_dbConnection.State == System.Data.ConnectionState.Closed) && !_override)
                {
                    throw new IOException("A database is already open. " + m_dbConnection.ConnectionString);
                }
                else if (!(m_dbConnection.State == System.Data.ConnectionState.Closed) && _override)
                {
                    close();
                    init(Path.Combine(path, filename));
                }
            }
            else
                init(Path.Combine(path, filename));
        }
        /// <summary>
        /// Find all possible factors for number (basically any primes below or equal to the squareroot of num)
        /// </summary>
        /// <param name="num">the number to get possible factors for</param>
        /// <returns>a list of factors to check</returns>
        public static string[] find_possible_factors(string num)
        {
            if (largest * largest > Double.Parse(num))//if we are still below the largest prime
                return dbcache.ToArray();

            if (largest * largest == Double.Parse(num))//we are equal to the square of the largest prime (just return the largest, since it will be a factor)
                return new string[] { largest.ToString() };

            //write(writer.ToArray());// need to write writer in case some included entries are part of factors
            //writer = new List<string>();

            string[] temp = read(num);
            dbcache.Clear();
            dbcache.AddRange(temp);
            largest = Double.Parse(temp[temp.Length - 1]);
            return dbcache.ToArray();
        }
        /// <summary>
        /// Adds prime to writer
        /// </summary>
        /// <param name="num">group of number to add</param>
        public static void add_prime(string[] num)
        {
            dbcache.AddRange(num);
            if (dbcache.Count >= writerlimit)
            {
                write(writer.ToArray());
                writer = new List<string>();
            }
        }
        /// <summary>
        /// Adds prime to writer
        /// </summary>
        /// <param name="num">number to add</param>
        public static void add_prime(string num)
        {
            writer.Add(num);
            if (writer.Count >= writerlimit)
            {
                write(writer.ToArray());
                writer = new List<string>();
            }
        }
        /// <summary>
        /// writes last of cache, clears out cache and closes db connection
        /// </summary>
        public static void close()
        {
            if (writer.Count != 0) // lets not lose any of our work when closing
                write(writer.ToArray());

            m_dbConnection.Close();
            dbstats = new dbstats_st();
            dbcache = new List<string>();
            writer = new List<string>();
        }
        /// <summary>
        /// 
        /// </summary>
        public static void updatestats()
        {
            if (!_init)
                init();
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                {
                    string sql = "select max(num) from primes";
                    SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader read = create.ExecuteReader();
                    read.Read();
                    dbstats.databasesize = read[0].ToString();
                }
                if (!(dbstats.databasesize == ""))
                {
                    using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                    {
                        string sql = "select prime from primes where num = " + dbstats.databasesize;
                        SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                        SQLiteDataReader read = create.ExecuteReader();
                        read.Read();
                        dbstats.largestnum = read[0].ToString();
                    }
                }
                else
                {
                    dbstats.databasesize = "0";
                    dbstats.largestnum = "1";
                }

            }
        }

        private static void init(string path = "DEFAULT")
        {
            _init = true;
            string filepath = "";
            if (path == "DEFAULT")
                filepath = Path.Combine(Directory.GetCurrentDirectory(), "db.sqlite");
            else
                filepath = path;

            if (!File.Exists(filepath))//If the database does not exist, create it
            {
                SQLiteConnection.CreateFile(filepath);
                m_dbConnection = new SQLiteConnection("Data Source=" + filepath + "; Version=3;"); //initialize connection for the rest of the program
                m_dbConnection.Open();
                using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                    {
                        cmd.Transaction = tr;
                        cmd.CommandText = "CREATE TABLE primes (num INTEGER Primary Key, prime INTEGER, date INTEGER)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE INDEX n ON primes(num)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE INDEX p ON primes(prime)";
                        cmd.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
            else
            {
                m_dbConnection = new SQLiteConnection("Data Source=" + filepath + "; Version=3;"); //initialize connection for the rest of the program
                m_dbConnection.Open();
            }

            dbstats.squarestoobig = false;
            dbstats.squarestoobigstart = "0";
            dbstats.databasesize = "";
            dbstats.largestnum = "";

            updatestats();

            number = Double.Parse(dbstats.databasesize) + 1;

        }
        private static String[] read(string condition)
        {
            if (!_init)
                init();
            List<string> ret = new List<string>();
            string temp = "0";
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                {

                    cmd.Transaction = tr;
                    string sql = "select num, prime from onemillion where squaredprime < " + condition + " and squaredprime <> 0";
                    SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader read = create.ExecuteReader();
                    while (read.Read())
                    {
                        ret.Add(read["prime"].ToString());
                        temp = read["num"].ToString();
                    }
                    sql = "select prime from onemillion where num = " + (uint.Parse(temp) + 1);
                    create = new SQLiteCommand(sql, m_dbConnection);
                    read = create.ExecuteReader();
                    while (read.Read())
                        ret.Add(read["prime"].ToString());
                    return ret.ToArray();

                }
            }
        }
        private static void write(string[] prime)
        {
            if (!_init)
                init();
            Int32 time = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                {
                    foreach (string x in prime)
                    {
                        cmd.CommandText = @"INSERT INTO primes (num, prime, date) VALUES (@num, @prime, @date)";
                        cmd.Parameters.Add(new SQLiteParameter("@num", number++));
                        cmd.Parameters.Add(new SQLiteParameter("@prime", x));
                        cmd.Parameters.Add(new SQLiteParameter("@date", time));
                        cmd.ExecuteNonQuery();
                    }
                }
                tr.Commit();
            }
            updatestats();
        }
    }
}
