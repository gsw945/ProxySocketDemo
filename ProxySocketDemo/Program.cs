using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySocketDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: 目前仅支持http方式（不支持https）
            // string url = "http://www.mitbbs.com/mitbbs_bbsuser.php";
            string url = "http://www.google.com/doodles?hl=zh-CN";
            bool is_post = false;
            List<StringPair> postParas = new List<StringPair>();
            bool needProxy = true;
            string _ip = "45.32.25.109"; // 代理主机，目前只能是ip（不能是域名）
            string _port = "2280";
            string _username = null;
            string _password = null;
            ProxyType _type = ProxyType.SocksV5;
            Proxy proxy = new Proxy(_ip, _port, _username, _password, _type);
            string page_source = WebDataGetter.GetPageSource(url, is_post, postParas, needProxy, proxy);
            Console.WriteLine(page_source);
            Console.Write("Press any key to quit...");
            Console.ReadKey(true);
        }
    }
}
