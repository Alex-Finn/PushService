using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using MainCycle.Database;
using MainCycle.PipeServices;
using MainCycle.PushServices;

namespace MainCycle
{
	partial class PushPipeService : ServiceBase
	{
		PushPipeServiceCycle service;
		public PushPipeService()
		{
			InitializeComponent();
			this.CanStop = true;
			this.CanPauseAndContinue = false;
			this.AutoLog = false;
		}

		protected override void OnStart( string[] args )
		{
			// TODO: Добавьте код для запуска службы.
			service = new PushPipeServiceCycle();
			Thread serviceThread = new Thread(new ThreadStart(service.Start));
			serviceThread.Start();
			//Program.Start(args);
		}

		protected override void OnStop()
		{
			service.Stop();
			Thread.Sleep(1000);
		}

		protected override void OnShutdown()
		{
			service.Stop();
		}
	}

	class PushPipeServiceCycle
	{
		SelectFromDb selector;
		LifeTimeTimer serviceIsAlive;
		ApnsFeedBack feedbackIsExist;
		BasePush pushHandler;
		bool enabled = true;

		public PushPipeServiceCycle()
		{
			Console.WriteLine();
			Console.WriteLine($"Главный цикл сервиса {Program.ServiceName} создан");
		}

		public void Start()
		{
			serviceIsAlive = new LifeTimeTimer(60); //	период в секундах для перезаписи файла функционирования
			
			//========ЗАГАСИЛ=========
			//feedbackIsExist = new ApnsFeedBack(1);	//	период в часах для проверки фидбэка от сервера APNS

			//PushNotificationsLists _listPush;


			enabled = true;
			Console.WriteLine();
			Console.WriteLine($"Сервис {Program.ServiceName} запущен");
#if DEBUG
			Console.WriteLine($"=====================Режим расширеных логов включен================");
#endif
			while ( enabled )
			{
				selector = new SelectFromDb();
				if ( selector.SelectFromDbMethod() )
				{
					if ( PushNotificationsLists.ListNotificationsForSend.Count != 0 )
					{
//#if DEBUG
						Console.WriteLine($"Количество сообщений для отправки: {PushNotificationsLists.ListNotificationsForSend.Count}");
//#endif
						foreach ( var notification in PushNotificationsLists.ListNotificationsForSend )
						{
							pushHandler = new BasePush();
							pushHandler.PushMethod(notification);
						}

						PushNotificationsLists.ListNotificationsForSend.Clear();
						continue;
					}
#if DEBUG
					else
						Console.WriteLine($"Количество сообщений для отправки: {PushNotificationsLists.ListNotificationsForSend.Count}");
#endif
				}

#if DEBUG
				Console.WriteLine();
				Console.WriteLine($"Запускается пайп");
#endif
				BasePipe.ListenPipeMethod();
			}
#if DEBUG
			Console.WriteLine($"bool Enabled ? {enabled}");
#endif
		}

		public void Stop()
		{
			serviceIsAlive?.MakeStopMark();
			serviceIsAlive?.StopTimer();
			feedbackIsExist?.StopTimer();		
			enabled = false;
			Console.WriteLine();
			Console.WriteLine($"Сервис {Program.ServiceName} остановлен");			
		}
	}
}
