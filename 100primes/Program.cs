using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _100primes
{
    class Program
    {
        static double placeholder = 0;
        static DateTime start;
        static string prevnum = "0";
        public static int[] seconds = new int[60];
        static uint[] minutes = new uint[60];
        static uint[] hours = new uint[24];
        public static int process = 0;
        public static int threads = 0;
        static DateTime timer = DateTime.Now;
        static double CPUpercentage = 1; // Added in case it needs to be throttled more then Process Priority can do. Limits currentthreads to (Maxthreads*CPUPercentage)
        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal; //Added in case it needs to be throttled
            dbclass.open(Directory.GetCurrentDirectory(), "db.sqlite");
            try
            {
                Double lowlimit = Double.Parse(dbclass.dbstats.largestnum);
                Double highlimit = Math.Sqrt(Double.MaxValue);
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
                    isprime(i.ToString());
                }
            }
            catch
            {
                dbclass.close();
            }
        }
        static bool isprime(string number)
        {
            //if (number == "169")
            //    Console.Write("");

            //-----------LAST DIGIT CHECKS----------//
            int last = int.Parse(number[number.Length - 1].ToString());
            //--2--//
            if (last % 2 == 0)//2
                return false;
            //--5--//
            if (last == 5)//5 (0 is already covered by 2)
                return false;

            //---------Other Checks-----------------//
            //--3--//
            int total = 0;
            foreach (char x in number)
            {
                total += int.Parse(x.ToString());
                if (total > 10)
                    total -= 9;
            }
            if (total % 3 == 0)
                return false;//not prime

            //--7--//
            string sevens = number;
            while (sevens.Length > 2)
            {
                uint secondhalf = uint.Parse(sevens[sevens.Length - 1].ToString());
                sevens = (double.Parse(sevens.Substring(0, sevens.Length - 1)) + (secondhalf * 5)).ToString();
            }
            if (int.Parse(sevens) % 7 == 0)
                return false;//not prime

            //--11--//
            bool flipflop = false;
            double flip = 0;
            double flop = 0;
            foreach (char x in number)
            {
                if (flipflop)
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
            if (Math.Abs((float)flip - flop) % 11 == 0)
                return false; // not prime
            int workerThreads;
            int portThreads;

            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            while (threads >= (Math.Ceiling(workerThreads * CPUpercentage)))
            {
                if (DateTime.Now.Second != timer.Second)
                {
                    display(Double.Parse(number), timer);
                    timer = DateTime.Now;
                }
                Thread.Sleep(50);
            }
            Interlocked.Increment(ref threads);
            _holder hldr = new _holder();
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { hldr.holder(number, dbclass.find_possible_factors(number)); }));
            return false;
        }

        static void display(double num, DateTime timer)
        {
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

            minutes[0] += (uint)seconds[0];
            hours[0] += (uint)seconds[0];
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
        static uint addval(uint[] val, int num = -1)
        {
            uint ret = 0;
            if (num == -1)
                num = val.Length;
            for (int i = 0; i != num; i++)
                ret += val[i];
            return ret;
        }
        static uint addval(int[] val, int num = -1)
        {
            uint ret = 0;
            if (num == -1)
                num = val.Length;
            for (int i = 0; i != num; i++)
                ret += (uint)val[i];
            return ret;
        }
    }
    class _holder
    {
        public void holder(object number, object primescollection)
        {
            if ((string)number == "2841849473")
                Console.Write("");
            string[] primes = (string[])primescollection;
            double num = double.Parse(number.ToString());
            bool brk = false;
            foreach (string num2 in primes)
            {
                double numb = double.Parse(num2.ToString());
                if (num * num < numb)//too big
                    brk = true;
                if (num % numb == 0)//is divisable
                    brk = true;
                if (brk)
                    break;
            }
            if (!brk)
                dbclass.add_prime(number.ToString());
            Interlocked.Increment(ref Program.seconds[0]);
            Interlocked.Decrement(ref Program.threads);
        }
    }
}




