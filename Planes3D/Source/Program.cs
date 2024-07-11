using System;
using System.Collections.Generic;
using System.Text;
using DXSharp;
using DXSharp.D3D;

namespace Planes3D
{
    class Program
    {

        static void Main(string[] args)
        {
            Log.WriteLine("Initializing");
            
            Engine.Initialize();
            Engine.Current.RunEventLoop();
        }
    }
}
