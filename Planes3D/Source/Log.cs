using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Planes3D
{
    public sealed class Log
    {

        public static void WriteLine(string format, params object[] args)
        {
            StackFrame stack = new StackFrame(1);

            Console.WriteLine(string.Format("[{0}::{1}]: {2}]", stack.GetMethod().DeclaringType.Name, stack.GetMethod().Name, string.Format(format, args)));
        }
    }
}
