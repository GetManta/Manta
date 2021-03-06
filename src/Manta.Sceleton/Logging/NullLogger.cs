﻿namespace Manta.Sceleton.Logging
{
    public class NullLogger : ILogger
    {
        public void Trace(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
        public void Trace(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Trace<T1>(string message, T1 arg1)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1);
        }
        public void Trace<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2);
        }
        public void Trace<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2, arg3);
        }
        public void Debug(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Debug<T1>(string message, T1 arg1)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1);
        }
        public void Debug<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2);
        }
        public void Debug<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2, arg3);
        }
        public void Info(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
        public void Info(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Info<T1>(string message, T1 arg1)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1);
        }
        public void Info<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2);
        }
        public void Info<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2, arg3);
        }
        public void Warn(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
        public void Warn(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Warn<T1>(string message, T1 arg1)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1);
        }
        public void Warn<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2);
        }
        public void Warn<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2, arg3);
        }
        public void Error(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
        public void Error(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Error<T1>(string message, T1 arg1)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1);
        }
        public void Error<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2);
        }
        public void Error<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2, arg3);
        }
        public void Fatal(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
        public void Fatal(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Fatal<T1>(string message, T1 arg1)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1);
        }
        public void Fatal<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2);
        }
        public void Fatal<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
        {
            System.Diagnostics.Debug.WriteLine(message, arg1, arg2, arg3);
        }
    }
}
