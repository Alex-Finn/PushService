using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace MainCycle
{
	[RunInstaller(true)]
	public partial class PushPipeServiceInstaller : System.Configuration.Install.Installer
	{
		ServiceInstaller serviceInstaller;
		ServiceProcessInstaller processInstaller;

		public PushPipeServiceInstaller()
		{
			InitializeComponent();

			serviceInstaller = new ServiceInstaller();
			processInstaller = new ServiceProcessInstaller();

			processInstaller.Account = ServiceAccount.LocalSystem;
			//processInstaller.Username = null;
			//processInstaller.Password = null;
			

			serviceInstaller.DisplayName = Program.ServiceName;
			serviceInstaller.ServiceName = Program.ServiceName;
			serviceInstaller.StartType = ServiceStartMode.Automatic;
			serviceInstaller.Description = "Служба рассылки PUSH-уведомлений 2019";

			Installers.Add(processInstaller);
			Installers.Add(serviceInstaller);
			
		}
	}
}
