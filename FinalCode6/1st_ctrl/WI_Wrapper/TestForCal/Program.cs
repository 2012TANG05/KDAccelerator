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
            string setuppath = ".\\.\\project\\txh170712\\txh170712.setup";
            string terpath = ".\\.\\project\\txh170712\\txh170712.ter";
            string txpath = ".\\.\\project\\txh170712\\txh170712.tx";
            string rxpath = ".\\.\\project\\txh170712\\txh170712.rx";

            RayTracingProceed.RayTracingProceed.Calculate(setuppath, terpath, txpath, rxpath);

            Console.WriteLine("计算节点跑完了");
        }
    }
}
