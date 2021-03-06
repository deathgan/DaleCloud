/*******************************************************************************
 * Copyright © 2016 NFine.Framework 版权所有
 * Author: DaleCloud
 * Description: DaleCloud快速开发平台
 * Website：
*********************************************************************************/
using log4net;
using System;
using System.IO;
using System.Web;

namespace DaleCloud.Code
{
    public class LogFactory
    {
        static LogFactory()
        {
            FileInfo configFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory+"\\Configs\\log4net.config");
            log4net.Config.XmlConfigurator.Configure(configFile);
        }
        public static void LogFactoryConfig()
        {
            FileInfo configFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Configs\\log4net.config");
            log4net.Config.XmlConfigurator.Configure(configFile);
        }
        public static Log GetLogger(Type type)
        {
            return new Log(LogManager.GetLogger(type));
        }
        public static Log GetLogger(string str)
        {
            return new Log(LogManager.GetLogger(str));
        }
    }
}
