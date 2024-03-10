using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedCompany
{
    static internal partial class Extensions
    {
        private static Regex OnlyAlphanumericalRegex = new Regex("[^a-zA-Z0-9]");
        public static string OnlyAlphanumerical(this string input)
        {
            return OnlyAlphanumericalRegex.Replace(input, "");
        }
    }
}
