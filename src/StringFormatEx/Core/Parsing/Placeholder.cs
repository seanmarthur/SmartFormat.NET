﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    public sealed class Placeholder : FormatItem
    {
        public Placeholder(Format parent, int startIndex) : base(parent, startIndex)
        {
            this.parent = parent;
            this.Selectors = new List<Selector>();
        }

        public readonly Format parent;
        public List<Selector> Selectors {get; private set;}
        public Format Format { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder(endIndex - startIndex);
            result.Append("{");
            foreach (var s in Selectors)
            {
                result.Append(s.baseString, s.operatorStart, s.endIndex - s.operatorStart);
            }
            if (Format != null)
            {
                result.Append(":");
                result.Append(Format.ToString());
            }
            result.Append("}");
            return result.ToString();
        }
    }
}
