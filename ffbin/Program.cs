using System;
using System.ServiceProcess;

namespace ffbin
{
    static class Program
    {
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                var ff = new ffbin();
                ff.Start();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new ffbin() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
