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
        static List<string> primes = new List<string>();
        static List<string> writer = new List<string>();
        static ulong placeholder = 0;
        static DateTime start;
        static void Main(string[] args)
        {
            if (File.Exists("primes.txt"))
                File.Delete("primes.txt");
            uint lowlimit = 2;
            UInt64 highlimit = UInt64.MaxValue;
            DateTime timer = DateTime.Now;
            start = DateTime.Now;
            for (UInt64 i = lowlimit; i != highlimit; i++)
            {
                if (DateTime.Now.Second != timer.Second)
                {
                    display(i);
                    timer = DateTime.Now;
                }
                if (isprime(i.ToString()))
                {
                    primes.Add(i.ToString());
                    writer.Add(i.ToString());
                    if (primes.Count % 1000000 == 0)
                    {
                        Console.Beep();
                        File.AppendAllLines("primes.txt", writer);
                        writer = new List<string>();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
        }
        static void printnum(ulong num)
        {
            Console.WriteLine(num + " - " + (isprime(num.ToString()) ? "Prime" : "Not Prime"));
        }
        static bool isprime(string number)
        {
            if (number == "1619")
                Console.Write("");
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
                sevens = (ulong.Parse(sevens.Substring(0, sevens.Length - 1)) + (secondhalf*5)).ToString();
            }
            if (int.Parse(sevens) % 7 == 0)
                return false;//not prime

            //--11--//
            bool flipflop = false;
            ulong flip = 0;
            ulong flop = 0;
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
                ulong num = ulong.Parse(x);
                ulong num2 = ulong.Parse(number);
                if (Math.Pow(num, 2) <= num2)
                {
                    if (num2 % num == 0)
                        return false;
                }
                else
                    return true;
            }
            return true;
        }
        static void display(ulong num)
        {
            Console.Clear();
            Console.WriteLine("Process started at " + start.ToLongTimeString() + ".");
            TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
            Console.WriteLine("Running for {0:N0} days, {1} hours, {2} minutes, {3} seconds",
                                elapsedSpan.Days, elapsedSpan.Hours,
                                elapsedSpan.Minutes, elapsedSpan.Seconds);
            Console.WriteLine("Currently running " + num);
            Console.WriteLine("Last Prime Found: #" + primes.Count + " - " + primes[primes.Count - 1]);
            uint temp2 = (uint)(primes.Count);
            Console.WriteLine((temp2 - placeholder) + " primes processed in the last second.");
            placeholder = (ulong)primes.Count;
        }
    }
}


