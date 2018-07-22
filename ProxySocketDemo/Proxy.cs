using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySocketDemo
{
    /// <summary>
    /// 代理类
    /// </summary>
    public class Proxy
    {
        /// <summary>
        /// 代理服务器
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 代理服务器端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 代理服务器登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 代理服务器登录密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 代理类型
        /// </summary>
        public ProxyType Type { get; set; }

        public Proxy(string _server,string _port,string _username,string _password,ProxyType _type)
        {
            this.Server = _server;
            this.Port = _port;
            this.UserName = _username;
            this.Password = _password;
            this.Type = _type;
        }
    }

    /// <summary>
    /// 代理方式
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// SOCKS v5方式(适用于Socket)
        /// </summary>
        SocksV5,
        /// <summary>
        /// http方式(适用于HttpWebRequest)
        /// </summary>
        Http
    }
}
