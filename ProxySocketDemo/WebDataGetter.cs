using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;

using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Services.Description;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Org.Mentalis.Network.ProxySocket;


namespace ProxySocketDemo
{
    /// <summary>
    /// 网络数据获取类
    /// </summary>
    public class WebDataGetter
    {
        #region 数据抓取
        private static CookieContainer cookieContainer = new CookieContainer(); //Cookie容器

        /// <summary>
        /// 根据URL获取页面源码(HttpWebReques方式)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="requestPost">是否要求使用POST方式(默认为GET)</param>
        /// <param name="postParameter">使用POST方式时，所需的参数字符串</param>
        /// <param name="needProxy">是否需要代理</param>
        /// <param name="proxyServer">代理服务器</param>
        /// <param name="proxyUser">代理用户</param>
        /// <returns></returns>
        private static string GetPageSourceHttp(string url, bool requestPost, List<StringPair> postParameter, bool needProxy, StringPair proxyServer, StringPair proxyUser)
        {
            HttpWebRequest req;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //参考 http://zhoufoxcn.blog.51cto.com/792419/561934/
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                req = WebRequest.Create(url) as HttpWebRequest;
                req.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(url);
            }
            if (needProxy)
            {
                if (proxyServer != null)
                {
                    int port = 80;
                    if (RegexHandle.IsMatch(proxyServer.PartTwo, @"^\d{1,5}$"))
                    {
                        port = int.Parse(proxyServer.PartTwo);
                    }
                    WebProxy proxy = new WebProxy(proxyServer.PartOne, port); //定义代理
                    if (proxyUser != null)
                    {
                        string username = proxyUser.PartOne;
                        string password = proxyUser.PartTwo;
                        if (string.IsNullOrEmpty(username) == false) //判断代理服务器登录用户名是否有效
                        {
                            proxy.Credentials = new NetworkCredential(username, password); //定义代理登录用户名和密码

                        }
                        req.UseDefaultCredentials = true; //启用网络认证
                    }
                    req.Proxy = proxy;//设置代理
                }
            }
            req.Timeout = 1000 * 20; //超时时间为20秒
            req.KeepAlive = true;
            req.Accept = "*/*"; //接受任意文件
            req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);//不缓存内容
            req.UserAgent = "	Mozilla/5.0 (Windows NT 6.3; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0"; //浏览器信息
            req.AllowAutoRedirect = true;//允许302重定向
            req.CookieContainer = cookieContainer;//设置cookie容器
            req.Credentials = CredentialCache.DefaultCredentials; //设置身份验证信息
            req.Referer = url; //当前页面的引用
            if (requestPost)
            {
                //Post请求方式
                req.Method = WebRequestMethods.Http.Post;
                //请求内容类型
                req.ContentType = "application/x-www-form-urlencoded";
                //原始代码
                string paraUrlCoded = "";
                StringBuilder sb = new StringBuilder("");
                for (int i = 0; i < postParameter.Count; i++)
                {
                    sb.Append((i < 1 ? "" : "&") + postParameter[i].PartOne + "=" + Uri.EscapeDataString(postParameter[i].PartTwo));
                }
                paraUrlCoded = sb.ToString();
                //paraUrlCoded = paraUrlCoded.Replace(":", "%3A");
                //将URL编码后的字符串转化为字节
                byte[] payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                //设置请求的 ContentLength 
                req.ContentLength = payload.Length;
                //获得请求流
                using (Stream writer = req.GetRequestStream())
                {
                    //将请求参数写入流
                    writer.Write(payload, 0, payload.Length);
                }
            }
            try
            {
                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    string charSet = res.CharacterSet;
                    if (string.IsNullOrEmpty(charSet))
                    {
                        charSet = "utf-8"; //为获取到有效编码时采用UTF-8编码
                    }
                    using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding(charSet)))
                    {
                        string webSource = sr.ReadToEnd();
                        return webSource;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Net.Sockets.SocketException)
                {
                    Console.WriteLine("GetPageSource 网络中断：" + ex.Message);
                }
                Console.WriteLine("GetPageSource Error" + ex.Message);
                return ex.Message;
            }
            finally
            {
                req.Abort();
            }
        }

        /// <summary>
        /// 证书验证
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
        #endregion

        /// <summary>
        /// 根据URL获取页面源码
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="requestPost">是否要求使用POST方式(默认为GET)</param>
        /// <param name="postParameter">使用POST方式时，所需的参数字符串</param>
        /// <param name="needProxy">是否需要代理</param>
        /// <param name="proxyServer">代理服务器</param>
        /// <param name="proxyUser">代理用户</param>
        /// <returns></returns>
        private static string GetPageSourceSocket(string url, bool requestPost, List<StringPair> postParameter, bool needProxy, StringPair proxyServer, StringPair proxyUser)
        {
            HttpRequest hr = new HttpRequest();
            hr.TimeOut = 20;
            if (needProxy)
            {
                if (proxyServer != null)
                {
                    int port = 80;
                    if (RegexHandle.IsMatch(proxyServer.PartTwo, @"^\d{1,5}$"))
                    {
                        port = int.Parse(proxyServer.PartTwo);
                    }
                    string username = null;
                    string password = null;
                    if (proxyUser != null)
                    {
                        username = proxyUser.PartOne;
                        password = proxyUser.PartTwo;
                    }
                    hr.SetProxy(proxyServer.PartOne, port, username, password);
                }
            }
            if (requestPost)
            {
                StringBuilder sb = new StringBuilder("");
                for (int i = 0; i < postParameter.Count; i++)
                {
                    sb.Append((i < 1 ? "" : "&") + postParameter[i].PartOne + "=" + Uri.EscapeDataString(postParameter[i].PartTwo));
                }
                return hr.Post(url, sb.ToString());
            }
            else
            {
                return hr.Get(url);
            }
        }

        /// <summary>
        /// 获取页面源码
        /// </summary>
        /// <param name="url">页面地址</param>
        /// <param name="requestPost">是否需要使用post方式请求</param>
        /// <param name="postParameter">使用post方式时，使用的参数</param>
        /// <param name="needProxy">是否需要代理</param>
        /// <param name="proxyParameter">代理参数</param>
        /// <returns></returns>
        public static string GetPageSource(string url, bool requestPost, List<StringPair> postParameter, bool needProxy, Proxy proxyParameter)
        {
            StringPair proxyServer = null;
            StringPair proxyUser = null;
            if (proxyParameter != null)
            {
                proxyServer = new StringPair(proxyParameter.Server, proxyParameter.Port);
                proxyUser = new StringPair(proxyParameter.UserName, proxyParameter.Password);
            }
            if (needProxy && proxyParameter != null && proxyParameter.Type == ProxyType.SocksV5)
            {
                return GetPageSourceSocket(url, requestPost, postParameter, needProxy, proxyServer, proxyUser);
            }
            else
            {
                return GetPageSourceHttp(url, requestPost, postParameter, needProxy, proxyServer, proxyUser);
            }
        }
    }
}
