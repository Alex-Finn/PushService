using System;
using System.Linq;
using MainCycle.Database;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;

namespace MainCycle.PushServices
{
	public class BasePush
	{
		ResponseEntity result;

		public void PushMethod( DbEntity record )
		{
			result = new ResponseEntity();
#if DEBUG
			Console.WriteLine($"Вошли в метод PushMethod");
#endif
			if ( record.Service_name == "FCM" )   //	FCM
			{
#if DEBUG
				Console.WriteLine($"Сообщение определено как FCM");				
#endif
				result = AndroidPush.FCMSend( record );


				//PushAnswer.SendAnswer(responseMessage, record, isApns);
			}
			else if ( record.Service_name == "APNS" )	//	APNS
			{
#if DEBUG
				Console.WriteLine($"Сообщение определено как APNS");
#endif
				result = ApplePush.ApnsSend( record );
			}
			PushAnswer.SendAnswer( record, result);
		}

		public static ResponseEntity GetResponseEntity( DbEntity record, Exception ex = null )
		{
			var result = new ResponseEntity();
			result.PushServiceName = record.Service_name ?? null;
			result.MessageId = record.Message_id ?? null;
			//result.ResponseMessage = responseMessage ?? null;
			result.Exception = ex ?? null;
			if ( result.Exception != null )
			{
				result.ExceptionType = ex?.GetType().Name;
				result.NewPushId =
					result.PushServiceName == "FCM"
					? ( ex is DeviceSubscriptionExpiredException
						? ( (DeviceSubscriptionExpiredException)ex ).NewSubscriptionId
						: null )
					: null;
				result.OldPushId =
					result.PushServiceName == "FCM"
					? ( ex is DeviceSubscriptionExpiredException
						? ( (DeviceSubscriptionExpiredException)ex ).OldSubscriptionId
						: record.Push_id )
					: record.Push_id;
				result.StatusCode =
					result.PushServiceName == "APNS"
					? ( ex is ApnsNotificationException
						? ( (ApnsNotificationException)ex ).ErrorStatusCode.ToString()
						: null )
					: null;
				result.ExceptionInnerException = ex?.InnerException?.ToString();
				result.ExceptionMessage = result.StatusCode ?? ex?.Message ?? ex?.InnerException?.Message ?? null;
				result.ExceptionNotificationDescription =
					result.PushServiceName == "FCM"
					? ( ex is FcmNotificationException
						? ( (FcmNotificationException)ex ).Description
						: null )
					: null;
			}
			else
				result.OldPushId = record.Push_id;

			ResponseEntity.ResponseCode result_code_variable = ResponseEntity.ResponseCode.UnknownException;
			if ( ex == null )
				result_code_variable = ResponseEntity.ResponseCode.Success;
			else
			{
				if ( Enum.IsDefined(typeof(ResponseEntity.ResponseCode), result.ExceptionType) )
				{
					result_code_variable =
						(ResponseEntity.ResponseCode)Enum.Parse(
							typeof(ResponseEntity.ResponseCode),
							result.ExceptionType);
				}
			}
			result.ResultCode = (int)result_code_variable;

			return result;
		}
	}
}
