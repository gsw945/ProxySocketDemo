using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using Org.Mentalis.Network.ProxySocket;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.IO;

namespace Org.Mentalis.Network.ProxySocket
{
    public class HttpRequest
    {
        private ProxySocket ps;
        private string proxyHost = null;
        private int proxyPort;
        private string proxyUser = null;
        private string proxyPassword = null;
        private bool useProxy = false;

        /// <summary>
        /// 超时限制(单位: 秒)
        /// </summary>
        public int TimeOut = 20;

        public HttpRequest()
        {
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="host">代理主机</param>
        /// <param name="port">代理端口</param>
        /// <param name="username">代理用户名</param>
        /// <param name="password">代理密码</param>
        public void SetProxy(string host, int port, string username, string password)
        {
            if (string.IsNullOrEmpty(host) == false)
            {
                this.proxyHost = host;
                if (port >= 0 && port <= 65535)
                {
                    this.useProxy = true;
                    this.proxyPort = port;
                    this.proxyUser = username;
                    this.proxyPassword = password;
                }
                else
                {
                    throw new ProxyException("port must be between 0 and 65535");
                }
            }
            else
            {
                throw new ProxyException("host is invalid");
            }
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public string Get(string url)
        {
            return Request(url, false, null, false);
        }

        /// <summary>
        /// 发送Post请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">Post数据</param>
        /// <returns></returns>
        public string Post(string url, string data)
        {
            return Request(url, true, data, false);
        }

        /// <summary>
        /// 发送Http请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="isPost">是否是Post方式</param>
        /// <param name="postData">使用Post方式时的数据</param>
        /// <param name="hasHeader">结果是否包含头信息</param>
        /// <returns></returns>
        private string Request(string url, bool isPost, string postData, bool hasHeader)
        {
            ps = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (this.useProxy)
            {
                ps.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(this.proxyHost), this.proxyPort);
                if (string.IsNullOrEmpty(this.proxyUser) == false)
                {
                    ps.ProxyUser = this.proxyUser;
                    ps.ProxyPass = this.proxyPassword;
                }
                ps.ProxyType = ProxyTypes.Socks5;
            }
            Uri uri = new Uri(url);
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            ps.ReceiveTimeout = (this.TimeOut < 0 ? 0 : this.TimeOut) * 1000; //接收超时限制(单位: 毫秒)
            string page = null;
            MemoryStream ms = new MemoryStream();
            bool isError = false;
            try
            {
                ps.Connect(uri.Host, url.Contains(":") ? uri.Port : 80);
                if (!ps.Connected)
                {
                    return "Error: Connect to the target host failed";
                }
                ps.Send(Encoding.ASCII.GetBytes(this.FormatData(url, isPost, postData)));
                int recv = 0;
                byte[] buffer = new byte[1024];
                do
                {
                    recv = ps.Receive(buffer, buffer.Length, 0);
                    ms.Write(buffer, 0, recv);
                    Array.Clear(buffer, 0, buffer.Length);
                } while (recv > 0);

            }
            catch (Exception ex)
            {
                if ((ex is SocketException) && (ex as SocketException).ErrorCode == 10060 && ms.Length > 0)
                {
                    //Console.WriteLine("The data receiving end");
                }
                else
                {
                    if (string.IsNullOrEmpty(page))
                    {
                        isError = true;
                        return "Error: " + ex.Message;
                    }
                }
            }
            finally
            {
                ps.Close();
                if (isError == false)
                {
                    ms.Seek(0, SeekOrigin.Begin); //将流的读写位置移动到开头
                    page = new StreamReader(ms, Encoding.ASCII).ReadToEnd();//将流读入到字符串准备分割 
                    if (string.IsNullOrEmpty(page) == false)
                    {
                        string[] sArray = page.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);//分割web服务器返回代码 分为头域和页面代码
                        // TODO: 如果返回的页面中不含有 Content-Length，后续的内容解析将会出错，此时需要手动计算 Content-Length
                        int tmpIndex = page.IndexOf("Content-Length: ") + 16;
                        if (tmpIndex >= page.Length)
                        {
                            page = "Error: Network error, Please try again";
                        }
                        else
                        {
                            page = page.Substring(tmpIndex);//分割字符串获得页面内容大小
                            page = page.Substring(0, page.IndexOf("\r\n"));
                            long begin = ms.Length - Convert.ToInt64(page);//流长度－页面内容大小就是我们需要的页面内容在流内的起始位置
                            ms.Seek(begin, SeekOrigin.Begin);//移动到此位置
                            if (sArray[0].ToLower().Contains("deflate"))
                            {
                                page = Deflate.DeCode(ms);
                            }
                            else if (sArray[0].ToLower().Contains("gzip"))
                            {

                                page = GZip.DeCode(ms);//将流传递给解压缩方法
                            }
                            else
                            {
                                StreamReader sr = new StreamReader(ms, Encoding.ASCII);
                                page = sr.ReadToEnd();
                                sr.Close();
                            }
                            if (hasHeader)
                            {
                                page = sArray[0] + "\r\n\r\n" + page;//将header与解压缩后的页面内容重新组合
                            }
                        }
                    }
                }
            }
            GC.Collect();
            return page;
        }

        /// <summary>
        /// 组织Http请求数据
        /// </summary>
        /// <param name="url">请求地址(完整地址，包括Protocol、Host、Port、Path和Query)</param>
        /// <param name="post">是否为Post(为true时为POST，为false时为GET)</param>
        /// <param name="postData">使用POST方式时需要提交的数据(为GET方式时请设置为null)</param>
        /// <returns></returns>
        private string FormatData(string url, bool post, string postData)
        {
            StringBuilder sb = new StringBuilder();
            Uri uri = new Uri(url);
            sb.Append(string.Format("{0} {1} HTTP/1.0\r\n", post ? "POST" : "GET", uri.PathAndQuery));
            sb.Append(string.Format("Host: {0}\r\n", uri.Host));
            sb.Append("UserAgent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0\r\n");
            sb.Append("Keep-Alive: 300\r\n");
            sb.Append("Connection: Keep-Alive\r\n");
            sb.Append("Cache-Control: no-cache\r\n");
            sb.Append("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n");
            sb.Append("Accept-Encoding: gzip, deflate\r\n");
            sb.Append("Accept-Language: zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3\r\n");
            if (post)
            {
                sb.Append("Content-Type: application/x-www-form-urlencoded\r\n");
                sb.Append(string.Format("Content-Length: {0}\r\n", string.IsNullOrEmpty(postData) ? 0 : postData.Length));
                sb.Append("\r\n");
                if (string.IsNullOrEmpty(postData) == false)
                {
                    sb.Append(string.Format("{0}\r\n", postData));
                }
            }
            sb.Append("\r\n");
            return sb.ToString();
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

    }
}
