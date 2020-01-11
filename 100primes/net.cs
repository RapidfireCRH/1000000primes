using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _100primes
{
    class _net
    {
        /*
         * Storage (struct)
         *      name, ip address, datestamp last heard, assigned from, assigned to, stacksize
         *      
         * Net packet expected:
         *      Name, 1 (primes packet), [primes]
         *      Name, 0, requested stack ammount
         *      
         * net listener
         *      received -> verify IP vs name
         *      if new, register name and ip, pass new assignment from top of stack with requested ammount, upto 200K
         *      If existing and not prime packet, send existing entry in storage
         *      if existing and prime packet, call db save command with stack, send new assignment with stacksize
         *      
         *  stack
         *      stack of primes to send, in stacks of 20 odd numbers. populate stack when it gets below 20% of 500k
         */
    }
    /// <summary>
    /// Add
    /// Read
    /// 
    /// </summary>
    class _storage
    {
        public struct storestruct
        {
            public string name;
            public IPAddress ip;
            public DateTime timestamp;
            public Double from;
            public Double to;
            public UInt64 stacksize;
        }
        private static List<storestruct> store;

        private static bool _init = false;

        public static void init()
        {
            //read from database
            //populate store with each entry
            store = new List<storestruct>();
            _init = true;
        }

        /// <summary>
        /// Add 
        /// </summary>
        /// <param name="entry"></param>
        public static void add(storestruct entry)
        {

        }
        
        public static storestruct read(string name)
        {
            foreach (storestruct x in store)
                if (x.name == name)
                    return x;
            return new storestruct();
        }
        public static storestruct read(double num)
        {
            foreach(storestruct x in store)
                if (x.from < num && x.to > num)
                    return x;
            return new storestruct();
        }

        public static bool verify(string name, IPAddress ip)
        {
            if (!_init)
                init();

            foreach (storestruct x in store) 
                if (x.name == name && x.ip.Equals(ip))
                    return true;
            return false;
        }

        
        private static void load(string filename)
        {
            if (!_init)
                init();
        }

        private static void save(string filename)
        {
            if (!_init)
                init();
        }
    }
}
