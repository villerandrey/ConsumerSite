using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Be24BLogic
{

    public enum LogLevel
    {
        Debug = 1,
        Verbose = 2,
        Information = 3,
        Warning = 4,
        Error = 5,
        Critical = 6,
        None = int.MaxValue
    }

    public static class ApplicationLogging
    {
        ////public static ILoggerFactory LoggerFactory { get; } =
        ////  new LoggerFactory();

        //private static ILoggerFactory lf = new LoggerFactory();

        //static ILogger lg = new Logger<FileLoggerProvider>(lf);

        //static SourceSwitch sw = new SourceSwitch("SourceSwitch", "Verbose");

        //static ILoggerFactory loggerFactory = new LoggerFactory().AddTraceSource(sw);


        ////public static ILogger CreateLogger<T>() =>
        ////  LoggerFactory.CreateLogger<T>();


        //public static void addProvaider()
        //{


        //    loggerFactory.AddProvider(new FileLoggerProvider("d:\\log5.txt"));

        //}

        //public static   ILogger Loger {
        //    get{ return lg;}
        //       }




        //public static ILoggerFactory AddFile(this ILoggerFactory factory,
        //                               string filePath)
        //{



        //    factory.AddProvider(new FileLoggerProvider(filePath));


        //    return factory;
        //}



        //public static ILoggerFactory AddFile(this ILoggerFactory factory,
        //                              string filePath)
        //{
        //    factory.AddProvider(new FileLoggerProvider(filePath));
        //    return factory;
        //}


    }


    public class FileLoggerProvider : ILoggerProvider
    {
        private string path;
        public FileLoggerProvider(string _path)
        {
            path = _path;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(path);
        }

        public void Dispose()
        {
        }
    }


    public class FileLogger : ILogger
    {
        private string filePath;
        private object _lock = new object();
        public FileLogger(string path)
        {
            filePath = path;
            
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }


        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            //return logLevel == LogLevel.Trace;
            return true;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            //throw new NotImplementedException();
            File.AppendAllText(filePath, state.ToString()  + Environment.NewLine);

        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                lock (_lock)
                {
                    File.AppendAllText(filePath, formatter(state, exception) + Environment.NewLine);
                }
            }
        }
    }



    public class LogTraceListener : TraceListener
    {

        private System.IO.FileStream _fs;

        private System.IO.StreamWriter _wr;

        private string pvLog;


        public string Log
        {
            get { return pvLog; }
            set { pvLog = value; }
        }



        public LogTraceListener()
        {
            string fp = AppContext.BaseDirectory + "\\log.txt";
            _fs = new FileStream(fp, FileMode.Append);
            _wr = new System.IO.StreamWriter(_fs);
            this.Name = "DJL";
        }

        public LogTraceListener(string LogName)
        {
           
            _fs = new System.IO.FileStream(LogName, System.IO.FileMode.Append);
            _wr = new System.IO.StreamWriter(_fs);
           
            this.Name = "DJL";
        }

        public override void Write(string message)
        {
            _wr.Write(message);
            _wr.Flush();
        }

        public delegate void WriteLogHandler(string message);





       public override void WriteLine(string message)
        {
            var resultMsg =    DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "  " + message;
            _wr.WriteLine(resultMsg);
            _wr.Flush();
            pvLog = pvLog + resultMsg + Environment.NewLine;

        }

        public override void WriteLine(object  o,string category)
        {
            Exception e;
            e = (Exception)o;
            var resultMsg = DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "  " + e.ToString();
            _wr.WriteLine(resultMsg);
            _wr.Flush();
            pvLog = pvLog + resultMsg + Environment.NewLine;
        }

       
        private bool skipTime;
       

    } // end of class






}
