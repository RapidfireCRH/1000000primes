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
        static List<string> writer = new List<string>();
        static double placeholder = 0;
        static DateTime start;
        static void Main(string[] args)
        {
            dbclass.updatestats();
            Double lowlimit = Double.Parse(dbclass.largestnum) == 0 ? 2: Double.Parse(dbclass.largestnum);
            Double highlimit = Math.Sqrt(Double.MaxValue);
            DateTime timer = DateTime.Now;
            start = DateTime.Now;
            for (double i = lowlimit + 1; i != highlimit; i++)
            {
                if (DateTime.Now.Second != timer.Second)
                {
                    display(i);
                    timer = DateTime.Now;
                }
                if (isprime(i.ToString()))
                {
                    writer.Add(i.ToString());
                    if (writer.Count % 1000 == 0)
                    {
                        Console.WriteLine("Writing Database...");
                        dbclass.write(writer.ToArray());
                        writer = new List<string>();
                    }
                }
            }
        }
        static bool isprime(string number)
        {
            //if (number == "1619")
            //    Console.Write("");
            if(number.Length == 1 || number == "11")
            {
                int[] firstprimes = { 2, 3, 5, 7, 11 };
                foreach (int x in firstprimes)
                    if (int.Parse(number) == x)
                        return true;
            }
            int last = int.Parse(number[number.Length - 1].ToString());
            //--2--//
            if (last % 2 == 0)//2
                return false;
            //--5--//
            if (last == 5)//5 (0 is already covered by 2)
                return false;

            //--3--//
            int total = 0;
            foreach (char x in number)
            {
                total += int.Parse(x.ToString());
                if (total > 10)
                    total -= 9;
            }
            if (total % 3 == 0)
                return false;

            //--7--//
            string sevens = number;
            while (sevens.Length > 2)
            {
                uint secondhalf = uint.Parse(sevens[sevens.Length-1].ToString());
                sevens = (double.Parse(sevens.Substring(0, sevens.Length - 1)) + (secondhalf*5)).ToString();
            }
            if (int.Parse(sevens) % 7 == 0)
                return false;//not prime

            //--11--//
            bool flipflop = false;
            double flip = 0;
            double flop = 0;
            foreach(char x in number)
            {
                if(flipflop)
                {
                    flipflop = false;
                    flop += uint.Parse(x.ToString());
                }
                else
                {
                    flipflop = true;
                    flip += uint.Parse(x.ToString());
                }
            }
            if(Math.Abs((float)flip - flop)%11 == 0)
                return false;

            string[] primes = dbclass.read(number);
            foreach(string x in primes)
            {
                switch(x)
                {
                    case "2":
                    case "3":
                    case "5":
                    case "7":
                    case "11":
                        continue;
                }
                double num = double.Parse(x);
                double num2 = double.Parse(number);
                if (num2 % num == 0)
                    return false;
            }
            return true;
        }
        static void display(double num)
        {
            Console.Clear();
            Console.WriteLine("Process started at " + start.ToLongTimeString() + ".");
            TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
            Console.WriteLine("Running for {0:N0} days, {1} hours, {2} minutes, {3} seconds",
                                elapsedSpan.Days, elapsedSpan.Hours,
                                elapsedSpan.Minutes, elapsedSpan.Seconds);
            Console.WriteLine("Currently running " + String.Format("{0:n0}",num));
            if (writer.Count == 0)
            {
                Console.WriteLine("Process is initializing or writer has recently dumped. Please wait while we catch up.");
            }
            else
            {
                double temp = double.Parse(writer[writer.Count - 1]);
                Console.Write("Last Prime Found: #" + String.Format("{0:n0}", (writer.Count + Double.Parse(dbclass.databasesize))) + " - ");
                Console.WriteLine(String.Format("{0:n0}", temp));
            }
                
            double temp2 = (double)(writer.Count + Double.Parse(dbclass.databasesize));
            Console.WriteLine((temp2 - placeholder) + " primes processed in the last second." + (dbclass.squarestoobig?"We are now not able to calculate squares. This happened on prime number " + dbclass.squarestoobigstart:""));
            placeholder = (double)(writer.Count + Double.Parse(dbclass.databasesize));
        }
    }
}


