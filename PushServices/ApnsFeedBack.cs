using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MainCycle.Database;
using Oracle.ManagedDataAccess.Client;
using PushSharp.Apple;

namespace MainCycle.PushServices
{
	internal class ApnsFeedBack
	{
		TimerCallback tm;
		Timer timer;
		int period;
		
		List<InvalidPush> invalids = null;
		SetInvalidApnsPushId InvalidPushIdClass = null;

		public ApnsFeedBack( int periodInHours )
		{
			period = periodInHours * 1000 * 60 * 60;
			tm = new TimerCallback(CheckFeedBack);
			timer = new Timer(tm, null, 0, period);
			InvalidPushIdClass = new SetInvalidApnsPushId();
		}

		private void CheckFeedBack( object obj )
		{			
			invalids = new List<InvalidPush>();
			var fbs = new FeedbackService(ApnsConfigBuilder.Config(ApnsConfiguration.ApnsServerEnvironment.Production));
#if DEBUG
			Console.WriteLine($"Вошли в метод CheckFeedBack");
#endif

			fbs.FeedbackReceived += ( string deviceToken, DateTime timestamp ) =>
			{
#if DEBUG
				Console.WriteLine($"Получен фидбэк от сервера APNS");
#endif
				invalids.Add(new InvalidPush { PushId = deviceToken, TimeStamp = timestamp });
				// Remove the deviceToken from your database
				// timestamp is the time the token was reported as expired
				
			};

			fbs.Check();

			if ( invalids != null )
			{

				InvalidPushIdClass.SendToDb(invalids);
			}
		}

		private XmlDocument GetInvalidsXML()
		{			
			var xmlDoc = new XmlDocument();
			XmlElement rootElement, element, rowElement;

			rootElement = xmlDoc.CreateElement("invalid_push_id_list");
			xmlDoc.AppendChild(rootElement);

			foreach ( var item in invalids )
			{
				rowElement = xmlDoc.CreateElement("row");
				rootElement.AppendChild(rowElement);

				element = xmlDoc.CreateElement("field");
				element.SetAttribute("name", "push_id");
				element.InnerText = item.PushId;
				rowElement.AppendChild(element);

				element = xmlDoc.CreateElement("field");
				element.SetAttribute("name", "time_stamp");
				element.InnerText = item.TimeStamp.ToString();
				rowElement.AppendChild(element);
			}
			
			return xmlDoc;
		}		

		public void StopTimer()
		{
			timer.Dispose();
			if ( invalids != null )
			{
				SendInvalidsListToDb();
				Console.WriteLine($"Перед остановкой сервиса в базу был отправлен список из [{invalids.Count}] отозванных Push Id, полученных от сервера [APNS]");
			}			
		}
	}
}
