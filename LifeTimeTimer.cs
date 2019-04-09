using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MainCycle
{
	internal class LifeTimeTimer
	{
		TimerCallback tm;
		Timer timer;
		DateTime timerBirthTime;
		int period;
		private string _timeStamp;
		//private string _dateStamp;
		private string _logPath;
		private string _logFile;
		private string _message;

		public LifeTimeTimer(int periodInSeconds)
		{
			period = periodInSeconds * 1000;
			tm = new TimerCallback(MakeAMark);
			timer = new Timer(tm, null/*num*/, 0, period);
			timerBirthTime = DateTime.Now;
		}

		private void MakeAMark( object obj )
		{
			try
			{
				_timeStamp = $"[{DateTime.Now}]";
				//_dateStamp = $"{DateTime.Today.ToString(@"yyyy-MM\m-dd\d")}";
				_logPath = "D:\\ITouchPushService\\Logs";
				_logFile = Path.Combine(_logPath, "Live.txt");
				Directory.CreateDirectory(_logPath);

				_message = $"Last refresh:\t[{_timeStamp}]\t|\tRunning from:\t[{timerBirthTime}] Local";
				File.WriteAllText(_logFile, _message);
			}
			catch { }
		}
		internal void MakeStopMark()
		{
			try
			{
				_timeStamp = $"[{DateTime.Now}]";				
				_logPath = "D:\\ITouchPushService\\Logs";
				_logFile = Path.Combine(_logPath, "Live.txt");
				Directory.CreateDirectory(_logPath);

				_message =	Environment.NewLine 
							+ Environment.NewLine 
							+ $"Service stopped:\t[{_timeStamp}] Local";
				File.AppendAllText(_logFile, _message);
			}
			catch { }
		}

		public void StopTimer()
		{
			timer.Dispose();
			timerBirthTime = default(DateTime);
		}
		//internal static void MakeAMark( object obj )
		//{
		//	//System.Console.WriteLine("Enter the method");	//	срабатывает каждый раз
		//	//int x = (int)obj;

		//	System.Console.WriteLine($"IsAlive\t|\t{DateTime.Now.TimeOfDay}");
		//}
	}
}
