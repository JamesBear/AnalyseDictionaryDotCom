using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictManiac.Sources
{
    class MyRandom
    {
        public static Random random;

        public static void Initialize()
        {
            random = new Random(unchecked((int)DateTime.Now.Ticks));
        }

        public static char NextChar()
        {
            return (char)(((int)'A') + random.Next(26));
        }
    }
}
