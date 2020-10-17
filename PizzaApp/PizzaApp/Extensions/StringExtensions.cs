using System;
using System.Collections.Generic;
using System.Text;

namespace PizzaApp.Extensions
{
    public static class StringExtensions
    {
        public static string PremiereLettreMajuscule(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }

            str = str.ToLower();

            str = str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1);

            return str;
        } 
    }
}
