using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace ProxySocketDemo
{
    /// <summary>
    /// 正则表达式操作类
    /// </summary>
    public class RegexHandle
    {
        /// <summary>
        /// 正则匹配，返回第一次匹配的值
        /// </summary>
        /// <param name="source">源字符文本</param>
        /// <param name="pattern">正则匹配规则</param>
        /// <returns></returns>
        public static string Match(string source, string pattern)
        {
            Match match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 正则匹配，返回所有匹配的集合
        /// </summary>
        /// <param name="source">源字符文本</param>
        /// <param name="pattern">正则匹配规则</param>
        /// <returns></returns>
        public static List<string> Matches(string source, string pattern)
        {
            List<string> result = new List<string>();
            Match match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            while (match.Success)
            {
                result.Add(match.Value);
                match = match.NextMatch();
            }
            return result;
        }

        /// <summary>
        /// 正则匹配，返回第一个匹配组
        /// </summary>
        /// <param name="source">源字符文本</param>
        /// <param name="pattern">正则匹配规则</param>
        /// <returns></returns>
        public static List<string> Group(string source, string pattern)
        {
            Match match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            List<string> result = null;
            if (match.Success && match.Groups.Count > 1)
            {
                result = new List<string>();
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    result.Add(match.Groups[i].Success ? match.Groups[i].Value : null);
                }
            }
            return result;
        }

        /// <summary>
        /// 正则匹配，返回所有匹配组
        /// </summary>
        /// <param name="source">源字符文本</param>
        /// <param name="pattern">正则匹配规则</param>
        /// <returns></returns>
        public static List<List<string>> Groups(string source, string pattern)
        {
            Match match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            List<List<string>> result = null;
            bool has = false;
            List<string> item = null;
            while (match.Success && match.Groups.Count > 1)
            {
                if (has == false)
                {
                    result = new List<List<string>>();
                    has = true;
                }
                item = new List<string>();
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    item.Add(match.Groups[i].Success ? match.Groups[i].Value : null);
                }
                result.Add(item);
                match = match.NextMatch();
            }
            return result;
        }

        /// <summary>
        /// 正则替换, 将匹配到的文本替换为空字符串
        /// </summary>
        /// <param name="source">源字符文本</param>
        /// <param name="pattern">正则匹配规则</param>
        /// <returns></returns>
        public static string Remove(string source, string pattern)
        {
            Match match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            while (match.Success)
            {
                if (match.Value.Length > 0)
                {
                    source = source.Replace(match.Value, "");
                }
                match = match.NextMatch();
            }
            return source;
        }

        /// <summary>
        /// 验证字符串是否符合指定的正则匹配模式
        /// </summary>
        /// <param name="input">字符串</param>
        /// <param name="pattern">正则匹配模式</param>
        /// <returns></returns>
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
    }
}
