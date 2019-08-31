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
            catch
            {
                dbclass.close();
            }
            dbclass.close();
        }
        static bool isprime(string number)
        {
            if (number[number.Length - 1] != '1' && 
                number[number.Length - 1] != '3' && 
                number[number.Length - 1] != '7' && 
                number[number.Length - 1] != '9')
                return false;

            string[] primes = dbclass.find_possible_factors(number);
            foreach (string x in primes)
            {
                double num = double.Parse(x);
                double num2 = double.Parse(number);
                if (num * num > num2)//too big
                    return true;
                if (num2 % num == 0)//is divisable
                    return false;
            }

            return true;
        }
        static void display(double num, DateTime timer)
        {
           uint addval(uint[] val, int num2 = -1)
            {
                uint ret = 0;
                if (num2 == -1)
                    num2 = val.Length;
                for (int i = 0; i != num2; i++)
                    ret += val[i];
                return ret;
            }
            if (placeholder == 0)
                placeholder = Double.Parse(dbclass.dbstats.databasesize);

            Console.Clear();
            Console.WriteLine("Process started at " + start.ToLongTimeString() + ".");
            TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
            Console.WriteLine("Running for {0:N0} days, {1} hours, {2} minutes, {3} seconds",
                                elapsedSpan.Days, elapsedSpan.Hours,
                                elapsedSpan.Minutes, elapsedSpan.Seconds);
            Console.WriteLine("Currently running " + String.Format("{0:n0}", num));

            if (DateTime.Now.Hour != timer.Hour)
            {
                for (int i = 22; i != -1; i--)
                    hours[i + 1] = hours[i];
                hours[0] = 0;
            }
            if (DateTime.Now.Minute != timer.Minute)
            {
                for (int i = 58; i != -1; i--)
                    minutes[i + 1] = minutes[i];
                minutes[0] = 0;
            }
            for (int i = 58; i != -1; i--)
                seconds[i + 1] = seconds[i];

            minutes[0] += seconds[0];
            hours[0] += seconds[0];
            double temp = Double.Parse(dbclass.dbstats.largestnum);
            Console.Write("Last Prime Found: #" + String.Format("{0:n0}", Double.Parse(dbclass.dbstats.databasesize)) + " - ");
            Console.WriteLine(String.Format("{0:n0}", temp));
            double temp2 = (double)(seconds[0] + Double.Parse(dbclass.dbstats.databasesize));
            Console.WriteLine(dbclass.dbstats.squarestoobig ? "We are now not able to calculate squares. This happened on prime number " + dbclass.dbstats.squarestoobigstart : "");


            Console.WriteLine("");
            Console.WriteLine("Found Prime Stats:");
                Console.WriteLine(num - Double.Parse(prevnum) + " numbers processed.");
                prevnum = num.ToString();
            Console.WriteLine("Last Second      : " + seconds[0]);
            Console.WriteLine("Last 5 Seconds   : " + addval(seconds, 5) + " (" + addval(seconds, 5) / 5 + ")");
            Console.WriteLine("Last 30 Seconds  : " + addval(seconds, 30) + " (" + addval(seconds, 30) / 30 + ")");
            Console.WriteLine("Last Minute      : " + addval(seconds) + " (" + addval(seconds) / 60 + ")");
            Console.WriteLine("Last Hour        : " + addval(minutes) + " (" + (addval(minutes) / 60) / 60 + ")");
            Console.WriteLine("Last 24 Hours    : " + addval(hours) + " (" + ((addval(hours) / 24) / 60) / 60 + ")");
            placeholder = (double)(seconds[0] + Double.Parse(dbclass.dbstats.databasesize));


            seconds[0] = 0;
        }
        
    }
}



