using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySocketDemo
{
    /// <summary>
    /// 成对出现的字符串(用于传递数据，比如 url参数和对应的值 等)
    /// </summary>
    public class StringPair
    {
        public string PartOne { get; set; }
        public string PartTwo { get; set; }

        public StringPair(string part1, string part2)
        {
            this.PartOne = part1;
            this.PartTwo = part2;
        }
    }
}
