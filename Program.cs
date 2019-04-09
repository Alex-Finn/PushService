using System;
using System.Configuration;
using System.Linq;
using MainCycle.Database;
using MainCycle.PipeServices;
using MainCycle.PushServices;
using System.IO;
using System.ServiceProcess;
using MainCycle;

namespace MainCycle
{
	class Program
	{
		public const string ServiceName = "PushPipeService";

		static void Main( string[] args )
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
			new PushPipeService()
			};
			ServiceBase.Run(ServicesToRun);

		}
	}
}
