/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Spark.Engine.Search.Support
{
    public static class StringExtensions
    {
        public static string[] SplitNotEscaped(this string value, char separator)
        {
            var word = string.Empty;
            var result = new List<string>();
            var seenEscape = false;

            for (var i = 0; i < value.Length; i++)
            {
                if (i == '\\')
                {
                    seenEscape = true;
                    continue;
                }
               
                if (i == separator && !seenEscape)
                {
                    result.Add(word);
                    word = string.Empty;
                    continue;
                }

                if (seenEscape)
                {
                    word += '\\';
                    seenEscape = false;
                }

                word += i;
            }

            result.Add(word);

            return result.ToArray<string>();
        }

        public static Tuple<string,string> SplitLeft(this string text, char separator)
        {
            var pos = text.IndexOf(separator);

            if (pos == -1)
                return Tuple.Create(text, (string)null);     // Nothing to split
            var key = text.Substring(0, pos);
            var value = text.Substring(pos + 1);

            return Tuple.Create(key, value);
        }
    }
}
