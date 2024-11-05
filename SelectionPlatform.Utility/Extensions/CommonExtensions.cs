using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Utility.Extensions
{
    public static class CommonExtensions
    {
        public static double ToPares(this string s)
        {
            double result = 0;
            if (double.TryParse(s,out result))
            {
               
            }
            return result;
        }
    }
}
