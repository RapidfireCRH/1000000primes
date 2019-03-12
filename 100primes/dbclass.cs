using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace _100primes
{
    class dbclass
    {
        static bool _init = false;
        static SQLiteConnection m_dbConnection;
        static bool lck = false;
        static List<string> cache = new List<string>();
        static double number = 1;
        public static string databasesize = "";
        public static string largestnum = "";
        public static bool squarestoobig = false;
        public static string squarestoobigstart = "0";
        public static void init()
        {
            bool first = false;
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "db.sqlite");
            if (!File.Exists(filepath))
            {//If it doesnt exist, create it
                SQLiteConnection.CreateFile(filepath);
                first = true;
            }
            m_dbConnection = new SQLiteConnection("Data Source=" + filepath + "; Version=3;");
            m_dbConnection.Open();
            if (first)
            {
                using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                    {
                        cmd.Transaction = tr;
                        cmd.CommandText = "CREATE TABLE primes (num INTERGER Primary Key, prime INTEGER, squaredprime INTERGER)";
                        cmd.ExecuteNonQuery();
                    }
                    tr.Commit();
                }

            }
            else
            {
                using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                    {
                        string sql = "select max(num) from primes";
                        SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                        SQLiteDataReader read = create.ExecuteReader();
                        read.Read();
                        databasesize = read[0].ToString();
                    }
                    using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                    {
                        string sql = "select prime from primes where num = " + databasesize;
                        SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                        SQLiteDataReader read = create.ExecuteReader();
                        read.Read();
                        largestnum = read[0].ToString();
                    }

                }
                number = Double.Parse(databasesize) + 1;
            }
            _init = true;

        }
        public static String[] read(string condition)
        {
            if (!_init)
                init();
            while (lck)
                Thread.Sleep(200);
            List<string> ret = new List<string>();
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                {
                    cmd.Transaction = tr;
                    string sql = "select prime,squaredprime from primes where squaredprime < " + condition + " and squaredprime <> 0" ;
                    SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader read = create.ExecuteReader();
                    while (read.Read())
                    {
                        ret.Add(read["prime"].ToString());
                    }
                }
            }
            return ret.ToArray();
        }
        public static void write(string[] prime)
        {
            if (!_init)
                init();
            lck = true;
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                {
                    foreach (string x in prime)
                    {
                        cmd.CommandText = @"INSERT INTO primes (num, prime, squaredprime) VALUES (@num, @prime, @squaredprime)";
                        cmd.Parameters.Add(new SQLiteParameter("@num", number++));
                        cmd.Parameters.Add(new SQLiteParameter("@prime", x));
                        UInt64 temp2 = 0;
                        try
                        {
                            temp2 = UInt64.Parse(x);
                        }
                        catch { squarestoobig = true; squarestoobigstart = (number - 1).ToString(); }
                        cmd.Parameters.Add(new SQLiteParameter("@squaredprime",  temp2 * temp2));
                        cmd.ExecuteNonQuery();
                    }
                }
                tr.Commit();
            }
            lck = false;
            updatestats();
        }
        public static void close()
        {
            while(lck)
                Thread.Sleep(200);
            m_dbConnection.Close();
        }
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
                    databasesize = read[0].ToString();
                }
                using (SQLiteCommand cmd = m_dbConnection.CreateCommand())
                {
                    string sql = "select prime from primes where num = " + databasesize;
                    SQLiteCommand create = new SQLiteCommand(sql, m_dbConnection);
                    SQLiteDataReader read = create.ExecuteReader();
                    read.Read();
                    largestnum = read[0].ToString();
                }

            }
        }
    }
}
