﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat
{
    public static class SmartExtensions
    {
        #region: StringBuilder :

        public static void AppendSmart(this StringBuilder sb, string format, params object[] args)
        {
            var so = new StringOutput(sb);
            Smart.Default.FormatInto(so, format, args);
        }

        #endregion

        #region: String :

        public static string Format(this string format, params object[] args)
        {
            return Smart.Format(format, args);
        }

        #endregion

        #region: Enumerable :

        public static string FormatAndJoin<T>(this IEnumerable<T> sources, string separator, string format, Func<T, object[]> args)
        {
            // We're gonna need to count the items anyway, so let's not enumerate twice:
            var sourceList = sources as IList<T> ?? sources.ToList();
            var output = new StringOutput((format.Length + separator.Length) * sourceList.Count);
            var formatter = Smart.Default;
            Format cache = null; ;
            for (int i = 0; i < sourceList.Count; i++)
            {
                if (i > 0) output.Write(separator);
                formatter.FormatWithCacheInto(ref cache, output, format, args(sourceList[i]));
            }
            return output.ToString();
        }

        #endregion
    }
}
