using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RayTracingProceed;
using RayCalInfo;
using System.Reflection;
using System.Configuration;
using CalculateModelClasses;

namespace TestForCal
{
    class Program
    {
        static void Main(string[] args)
        {
            string setuppath = ".\\.\\project\\txh170508\\txh170508.setup";
            string terpath = ".\\.\\project\\txh170508\\txh170508.ter";
            string txpath = ".\\.\\project\\txh170508\\txh170508.tx";
            string rxpath = ".\\.\\project\\txh170508\\txh170508.rx";

            RayTracingProceed.RayTracingProceed.Calculate(setuppath, terpath, txpath, rxpath);

            Console.WriteLine("计算节点跑完了");
        }
    }
}
