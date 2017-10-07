using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace MJ_FormsServer
{
    public enum LogType
    {
        None,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class DebugLog
    {
        private static LogType logType = LogType.Debug;

        private static ILog dLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void SetLog(Type type)
        {
            dLog = LogManager.GetLogger(type);
        }

        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Debug level.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        // 备注:
        //     This method first checks if this logger is DEBUG enabled by comparing the level
        //     of this logger with the log4net.Core.Level.Debug level. If this logger is DEBUG
        //     enabled, then it converts the message object (passed as parameter) to a string
        //     by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer. It then proceeds
        //     to call all the registered appenders in this logger and also higher in the hierarchy
        //     depending on the value of the additivity flag.
        //     WARNING Note that passing an System.Exception to this method will print the name
        //     of the System.Exception but no stack trace. To print a stack trace use the Debug(object,Exception)
        //     form instead.
        public static void Debug(ILog log, object message)
        {
            dLog = log;
            Debug(message);
        }
        public static void Debug(object message)
        {
            LogHtml(LogType.Debug, message);
        }
        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Debug level including the stack
        //     trace of the System.Exception passed as a parameter.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        //   exception:
        //     The exception to log, including its stack trace.
        //
        // 备注:
        //     See the Debug(object) form for more detailed information.
        public static void Debug(ILog log, object message, Exception exception)
        {
            dLog = log;
            Debug(message, exception);
        }
        public static void Debug(object message, Exception exception)
        {
            LogHtml(LogType.Debug, message, exception);
        }

        //
        // 摘要:
        //     Logs a message object with the log4net.Core.Level.Error level.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        // 备注:
        //     This method first checks if this logger is ERROR enabled by comparing the level
        //     of this logger with the log4net.Core.Level.Error level. If this logger is ERROR
        //     enabled, then it converts the message object (passed as parameter) to a string
        //     by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer. It then proceeds
        //     to call all the registered appenders in this logger and also higher in the hierarchy
        //     depending on the value of the additivity flag.
        //     WARNING Note that passing an System.Exception to this method will print the name
        //     of the System.Exception but no stack trace. To print a stack trace use the Error(object,Exception)
        //     form instead.
        public static void Error(ILog log, object message)
        {
            dLog = log;
            Error(message);
        }
        public static void Error(object message)
        {
            LogHtml(LogType.Error, message);
        }
        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Error level including the stack
        //     trace of the System.Exception passed as a parameter.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        //   exception:
        //     The exception to log, including its stack trace.
        //
        // 备注:
        //     See the Error(object) form for more detailed information.
        public static void Error(ILog log, object message, Exception exception)
        {
            dLog = log;
            Error(message, exception);
        }
        public static void Error(object message, Exception exception)
        {
            LogHtml(LogType.Error, message, exception);
        }

        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Fatal level.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        // 备注:
        //     This method first checks if this logger is FATAL enabled by comparing the level
        //     of this logger with the log4net.Core.Level.Fatal level. If this logger is FATAL
        //     enabled, then it converts the message object (passed as parameter) to a string
        //     by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer. It then proceeds
        //     to call all the registered appenders in this logger and also higher in the hierarchy
        //     depending on the value of the additivity flag.
        //     WARNING Note that passing an System.Exception to this method will print the name
        //     of the System.Exception but no stack trace. To print a stack trace use the Fatal(object,Exception)
        //     form instead.
        public static void Fatal(ILog log, object message)
        {
            dLog = log;
            Fatal(message);
        }
        public static void Fatal(object message)
        {
            LogHtml(LogType.Fatal, message);
        }
        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Fatal level including the stack
        //     trace of the System.Exception passed as a parameter.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        //   exception:
        //     The exception to log, including its stack trace.
        //
        // 备注:
        //     See the Fatal(object) form for more detailed information.
        public static void Fatal(ILog log, object message, Exception exception)
        {
            dLog = log;
            Fatal(message, exception);
        }
        public static void Fatal(object message, Exception exception)
        {
            LogHtml(LogType.Fatal, message, exception);
        }

        //
        // 摘要:
        //     Logs a message object with the log4net.Core.Level.Info level.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        // 备注:
        //     This method first checks if this logger is INFO enabled by comparing the level
        //     of this logger with the log4net.Core.Level.Info level. If this logger is INFO
        //     enabled, then it converts the message object (passed as parameter) to a string
        //     by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer. It then proceeds
        //     to call all the registered appenders in this logger and also higher in the hierarchy
        //     depending on the value of the additivity flag.
        //     WARNING Note that passing an System.Exception to this method will print the name
        //     of the System.Exception but no stack trace. To print a stack trace use the Info(object,Exception)
        //     form instead.
        public static void Info(ILog log, object message)
        {
            dLog = log;
            Info(message);
        }
        public static void Info(object message)
        {
            LogHtml(LogType.Info, message);
        }
        //
        // 摘要:
        //     Logs a message object with the INFO level including the stack trace of the System.Exception
        //     passed as a parameter.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        //   exception:
        //     The exception to log, including its stack trace.
        //
        // 备注:
        //     See the Info(object) form for more detailed information.
        public static void Info(ILog log, object message, Exception exception)
        {
            dLog = log;
            Info(message, exception);
        }
        public static void Info(object message, Exception exception)
        {
            LogHtml(LogType.Info, message, exception);
        }

        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Warn level.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        // 备注:
        //     This method first checks if this logger is WARN enabled by comparing the level
        //     of this logger with the log4net.Core.Level.Warn level. If this logger is WARN
        //     enabled, then it converts the message object (passed as parameter) to a string
        //     by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer. It then proceeds
        //     to call all the registered appenders in this logger and also higher in the hierarchy
        //     depending on the value of the additivity flag.
        //     WARNING Note that passing an System.Exception to this method will print the name
        //     of the System.Exception but no stack trace. To print a stack trace use the Warn(object,Exception)
        //     form instead.
        public static void Warn(ILog log, object message)
        {
            dLog = log;
            Warn(message);
        }
        public static void Warn(object message)
        {
            LogHtml(LogType.Warn, message);
        }
        //
        // 摘要:
        //     Log a message object with the log4net.Core.Level.Warn level including the stack
        //     trace of the System.Exception passed as a parameter.
        //
        // 参数:
        //   message:
        //     The message object to log.
        //
        //   exception:
        //     The exception to log, including its stack trace.
        //
        // 备注:
        //     See the Warn(object) form for more detailed information.
        public static void Warn(ILog log, object message, Exception exception)
        {
            dLog = log;
            Warn(message, exception);
        }
        public static void Warn(object message, Exception exception)
        {
            LogHtml(LogType.Warn, message, exception);
        }

        private static void LogHtml(LogType lType, object message, Exception exception = null)
        {
            //字符串拼接
            //string htmlStr = "<meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\"><br>";
            string htmlStr = "<meta http-equiv=\"content-type\" content=\"text/html;charset=gb2312\"><br>";

            if (logType >= lType)
            {
                switch (lType)
                {
                    case LogType.Debug:
                        htmlStr += "<font color=\"green\">==> " + message + "</font><br>";
                        if (exception == null)
                        {
                            Console.WriteLine(message);
                            dLog.Debug(htmlStr);
                        }
                        else
                        {
                            Console.WriteLine(message + " - " + exception.Message);
                            dLog.Debug(htmlStr, exception);
                        }
                        break;
                    case LogType.Info:
                        htmlStr += "<font color=\"green\">==> " + message + "</font><br>";
                        if (exception == null)
                        {
                            Console.WriteLine(message);
                            dLog.Info(htmlStr);
                        }
                        else
                        {
                            Console.WriteLine(message + " - " + exception.Message);
                            dLog.Info(htmlStr, exception);
                        }
                        break;
                    case LogType.Warn:
                        htmlStr += "<font color=\"yellow\">==> " + message + "</font><br>";
                        if (exception == null)
                        {
                            Console.WriteLine(message);
                            dLog.Warn(htmlStr);
                        }
                        else
                        {
                            Console.WriteLine(message + " - " + exception.Message);
                            dLog.Warn(htmlStr, exception);
                        }
                        break;
                    case LogType.Error:
                        htmlStr += "<font color=\"red\">==> " + message + "</font><br>";
                        if (exception == null)
                        {
                            Console.WriteLine(message);
                            dLog.Error(htmlStr);
                        }
                        else
                        {
                            Console.WriteLine(message + " - " + exception.Message);
                            dLog.Error(htmlStr, exception);
                        }
                        break;
                    case LogType.Fatal:
                        htmlStr += "<font color=\"red\">==> " + message + "</font><br>";
                        if (exception == null)
                        {
                            Console.WriteLine(message);
                            dLog.Fatal(htmlStr);
                        }
                        else
                        {
                            Console.WriteLine(message + " - " + exception.Message);
                            dLog.Fatal(htmlStr, exception);
                        }
                        break;
                }
            }
        }
    }
}
