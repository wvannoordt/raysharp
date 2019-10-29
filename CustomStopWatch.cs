using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class CustomStopWatch
    {
        string task_name;
        public string TaskName {get {return task_name;} set {task_name = value;}}
        private DateTime start, end;
        private double elamsped_ms;

        public CustomStopWatch(string _task_name = "")
        {
            task_name = _task_name;
        }
        public void tic()
        {
            start = DateTime.Now;
        }
        public double  toc()
        {
            end = DateTime.Now;
            elamsped_ms = (end - start).TotalMilliseconds;
            return elamsped_ms;
        }
        public void Report(string msg)
        {
            Info.Write(msg + ": ", "cyan");
            Info.WriteLine(elamsped_ms + " ms");
        }
        public void Report()
        {
            Info.Write(task_name + ": ", "cyan");
            Info.WriteLine(elamsped_ms + " ms");
        }
    }
}
