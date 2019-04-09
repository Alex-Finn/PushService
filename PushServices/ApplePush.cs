using System;
using System.IO;
using System.Linq;
using System.Web;
using MainCycle.Database;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;

namespace MainCycle.PushServices
{
	public static class ApplePush
	{
		public static ResponseEntity ApnsSend( DbEntity record )
		{
			const string apns = "[APNS]";
#if DEBUG
			Console.WriteLine($"Вошли в метод ApnsSend");
#endif
			ApnsConfiguration config = null;
			ApnsServiceBroker apnsBroker = null;

			try
			{
				config = ApnsConfigBuilder.Config(ApnsConfiguration.ApnsServerEnvironment.Production);
			}
			catch ( Exception ex )
			{
				Console.WriteLine($"{apns} Возникла критическая ошибка. "
					+ $"Отправка сообщений на сервер {apns} отменена "
					+ Environment.NewLine
					+ $"{ex.Message}");
				return null;
			}

			// Create a new broker

			ResponseEntity pushResult = new ResponseEntity();
			apnsBroker = new ApnsServiceBroker(config);
			var apnsNotification = new ApnsNotification();

			try
			{
				// Wire up events
				apnsBroker.OnNotificationFailed += ( notification, aggregateEx ) =>
				{
#if DEBUG
					Console.WriteLine($"Сервер APNS ответил ошибкой. Попытка определить тип ошибки");
#endif
					aggregateEx.Handle(ex =>
					{

						// See what kind of exception it was to further diagnose
						if ( ex is ApnsNotificationException )
						{
							var notificationException = (ApnsNotificationException)ex;
							// Deal with the failed notification

							var apnsNotificationWithException = notificationException.Notification;
							var statusCode = notificationException.ErrorStatusCode;
							string desc = $"{apns} Notification Failed: Message ID = [{record.Message_id}], Code = [{statusCode}]";
							Console.WriteLine(desc);

							pushResult = BasePush.GetResponseEntity(record, notificationException);

						}
						else if ( ex is ApnsConnectionException )
						{
							var connectionException = (ApnsConnectionException)ex;
							var innerException = connectionException.InnerException;
							var message = connectionException.Message;
							string desc = $"{apns} Notification Failed. Connection failure: \nMessage = [{message}], \nInnerException = [{innerException}], \nMessage Id = [{record.Message_id}]";
							Console.WriteLine(desc);

							pushResult = BasePush.GetResponseEntity(record, connectionException);
						}
						else
						{
							var otherException = ex;
							// Inner exception might hold more useful information like an ApnsConnectionException
							string desc = $"{apns} Notification Failed for some unknown reason : \nMessage = [{otherException.Message}], \nInnerException = [{otherException.InnerException}], \nMessage Id = [{record.Message_id}]";
							Console.WriteLine(desc);

							pushResult = BasePush.GetResponseEntity(record, otherException);
						}
						// Mark it as handled
						return true;    //	обязательное условие такой конструкции (выдать bool)
					});
				};
				apnsBroker.OnNotificationSucceeded += ( notification ) =>
				{
#if DEBUG
					var desc = "Сообщение отправлено на сервер APNS";
					//responseMessage = desc;
					Console.WriteLine(desc);
#endif
					Console.WriteLine($"{apns} Сообщение [{record.Message_id}] успешно отправлено");
					pushResult = BasePush.GetResponseEntity(record);

				};
				// Start the broker
				apnsBroker.Start();

#if DEBUG
				Console.WriteLine($"Брокер отправки сообщений запущен");
#endif

				var jsonString = $"{{\"aps\":{{\"alert\":\"{record.Message_text}\"}}}}";
				var apns_expiration = DateTime.UtcNow.AddSeconds(record.Msg_Ttl);
				apnsNotification = new ApnsNotification
				{
					DeviceToken = record.Push_id,
					Payload = JObject.Parse(jsonString),
					Expiration = apns_expiration
				};
				apnsBroker.QueueNotification(apnsNotification);

				// Stop the broker, wait for it to finish   
				// This isn't done after every message, but after you're
				// done with the broker
#if DEBUG
				Console.WriteLine($"Останавливаем брокер - это действие выполнит отправку сообщения");
#endif
				apnsBroker.Stop();

				return pushResult;  //	выдаём наружу результат отправки пуша
			}
			catch ( Exception ex )
			{
				Console.WriteLine("{apns} ERROR: " + ex.Message);

				return BasePush.GetResponseEntity(record, ex);

			}
		}

		//public static bool ApnsSend( out string response, DbEntity record )
		//{
		//	response = "Apple-kostyl` mode is on";
		//	return true;
		//}
		//public static bool ApnsFeedbackService()
		//{
		//	var config = new ApnsConfiguration(
		//		ApnsConfiguration.ApnsServerEnvironment.Sandbox,
		//		appleCert,
		//		appleCertPsw);

		//	var fbs = new FeedbackService(config);
		//	fbs.FeedbackReceived += ( string deviceToken, DateTime timestamp ) =>
		//	{
		//		Console.WriteLine($"Feedback Serivce message: device token: {deviceToken}, timestamp: {timestamp}");

		//		// Remove the deviceToken from your database
		//		// timestamp is the time the token was reported as expired
		//	};
		//	fbs.Check();
		//	return false;
		//}
	}

}


