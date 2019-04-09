using System;
using MainCycle.Database;
using Newtonsoft.Json.Linq;

using PushSharp.Core;
using PushSharp.Google;

namespace MainCycle.PushServices
{
	public static class AndroidPush
	{
		public static ResponseEntity FCMSend(DbEntity record)
		{
			const string fcm = "[FCM ]";
			string GoogleAppID;
			string SENDER_ID;
			FcmConfiguration config;
			ResponseEntity pushResult = new ResponseEntity();
#if DEBUG
			Console.WriteLine($"Вошли в метод FCMSend");
#endif
			try
			{
				GoogleAppID = "************";
				SENDER_ID = "**********";


				//Configuration
				config = new FcmConfiguration(SENDER_ID, GoogleAppID, null);
				config.FcmUrl = "https://fcm.googleapis.com/fcm/send";
				//Create a new broker
				var fcmBroker = new FcmServiceBroker(config);

				//Wire up events
				fcmBroker.OnNotificationFailed += ( notification, aggregateEx ) =>
				{
					aggregateEx.Handle(ex =>
					{
						//See what kind of exception it was to further diagnose
						if ( ex is FcmNotificationException )
						{
							var notificationException = (FcmNotificationException)ex;

							//Deal with the failed notifications
							var fcmNotification = notificationException.Notification;
							var description = notificationException.Description;
							string desc = $"{fcm} Notification Failed: ID = [{fcmNotification.MessageId}], Desc = [{description}]";
							Console.WriteLine(desc);

							pushResult = BasePush.GetResponseEntity(record, notificationException);
						}
						else if ( ex is FcmMulticastResultException )
						{
							var multicastException = (FcmMulticastResultException)ex;
							
							foreach ( var succeededNotification in multicastException.Succeeded )
							{
								string desc = $"{fcm} Notification Succeeded: ID = [{succeededNotification.MessageId}]";
								Console.WriteLine(desc);
							}

							foreach ( var failedKvp in multicastException.Failed )
							{
								var n = failedKvp.Key;
								var e = failedKvp.Value;
								string desc = $"{fcm} Notification Failed: ID = [{n.MessageId}], Desc = [{e.Message}]";
								Console.WriteLine(desc);
							}

							pushResult = BasePush.GetResponseEntity(record, multicastException);
						}
						else if ( ex is DeviceSubscriptionExpiredException )
						{
							var expiredException = (DeviceSubscriptionExpiredException)ex;
							
							var oldId = expiredException.OldSubscriptionId;
							var newId = expiredException.NewSubscriptionId;
							string desc = $"{fcm} Device RegistrationId Expired: [{oldId}], \nMessage Id = [{record.Message_id}]";
							Console.WriteLine(desc);

							if ( !string.IsNullOrWhiteSpace(newId) )
							{
								string desc2 = $"{fcm} Device RegistrationId Changes To: [{newId}]";

								Console.WriteLine(desc2);
								//If this value isn`t null, our subscription changed and we should update our databse
							}

							pushResult = BasePush.GetResponseEntity(record, expiredException);
						}
						else if ( ex is RetryAfterException )
						{
							var retryException = (RetryAfterException)ex;
							//If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
							string desc = $"{fcm} Rate Limited, don`t send more until after [{retryException.RetryAfterUtc}], \nMessage Id = [{record.Message_id}]";
							Console.WriteLine(desc);

							pushResult = BasePush.GetResponseEntity(record, retryException);
						}
						else
						{
							var otherException = ex;
							string desc = $"{fcm} Notification failed for some unknown reason. Message: [{otherException.Message}], \nMessage Id = [{record.Message_id}]";
							Console.WriteLine(desc);


							pushResult = BasePush.GetResponseEntity(record, otherException);
						}
						//Mark it as handled
						return true;	//	обязательное условие такой конструкции (выдать bool)
					});
				};
				fcmBroker.OnNotificationSucceeded += ( notification ) =>
				{
#if DEBUG
					string desc = "Success";
					Console.WriteLine(desc);
#endif
					
					Console.WriteLine($"{fcm} Сообщение [{record.Message_id}] успешно отправлено");

					pushResult = BasePush.GetResponseEntity(record);
				};

				//Start the Broker
				fcmBroker.Start();

				//Queue a notification to send
				fcmBroker.QueueNotification(new FcmNotification
				{
					To = record.Push_id,
					Data = MakeFcmData(record.Message_text, "info"),
					TimeToLive = record.Msg_Ttl					
				});

				//Stop the Broker, wait for it to finish
				//This isn`t done after every message, but after you`re
				//done with the Broker
				fcmBroker.Stop();
				

				return pushResult;	//	выдаём наружу результат отправки пуша
				
			}
			catch ( Exception ex )
			{
				Console.WriteLine("{fcm} ERROR: " + ex.Message);

				return BasePush.GetResponseEntity(record, ex);				
			}
		}
		private static JObject MakeFcmData( string mes, string typ/*, long? regId */)
		{
			JObject res = new JObject();
			if ( !string.IsNullOrEmpty(mes) )
				res["message"] = new JValue(mes);
			if ( !string.IsNullOrEmpty(typ) )
				res["type"] = new JValue(typ);
			//if ( regId.HasValue )
			//	res["registryId"] = new JValue(regId.Value);
			res["click_action"] = new JValue("FCM_ACTION");
			return res;

		}
	}
}
