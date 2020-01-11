using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _100primes
{
    class Program
    {
        static double placeholder = 0;
        static DateTime start;
        static string prevnum = "0";
        static uint[] seconds = new uint[60];
        static uint[] minutes = new uint[60];
        static uint[] hours = new uint[24];
        static void Main(string[] args)
        {
            dbclass.open(Directory.GetCurrentDirectory(), "db.sqlite");
            try
            {
                Double lowlimit = Double.Parse(dbclass.dbstats.largestnum);
                Double highlimit = 239811952854768; //P^1000000 -1
                DateTime timer = DateTime.Now;
                start = DateTime.Now;
                for (double i = lowlimit + 1; i != highlimit; i++)
                {
                    if (i == 2)//new db, populate with first 5 primes
                    {
                        dbclass.add_prime("2");
                        dbclass.add_prime("3");
                        dbclass.add_prime("5");
                        dbclass.add_prime("7");
                        dbclass.add_prime("11");
                        i = 12;
                    }
                    if (DateTime.Now.Second != timer.Second)
                    {
                        display(i, timer);
                        timer = DateTime.Now;
                    }
                    if (isprime(i.ToString()))
                    {
                        dbclass.add_prime(i.ToString());
                        seconds[0]++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine("Stacktrace: " + e.StackTrace);
            }
            dbclass.close();
        }
        
    }
}



