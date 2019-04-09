using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Apple;

namespace MainCycle.PushServices
{
	public static class ApnsConfigBuilder
	{
		public static ApnsConfiguration Config( ApnsConfiguration.ApnsServerEnvironment serverEnvironment )
		{
			var currentdirectory = @"D:\ITouchPushService"; //	вариант для сервиса

			byte[] appleCert = default(byte[]);
			string appleCertPsw = "**********";
			ApnsConfiguration config = null;

			try
			{
				appleCert = File.ReadAllBytes(currentdirectory +
					@"\cert\*******************.p12");
			}
			catch (Exception ex)
			{
				string description = $"Не удалось считать файл сертификата по адресу [{currentdirectory}]"
					+ Environment.NewLine
					+ ex.Message;
				throw new Exception(description, ex);
			}

			//	Configuration(NOTE: .pfx can also be used here)
			try
			{
				config = new ApnsConfiguration(
					  serverEnvironment,
					  appleCert,
					  appleCertPsw);
			}
			catch ( Exception ex )
			{
				string description = $"Не удалось сконфигурировать соединение к серверу APNS"
					+ Environment.NewLine
					+ ex.Message;
				throw new Exception(description, ex);
			}
#if DEBUG
			Console.WriteLine($"Сертификат успешно подцеплен. Настроено на {serverEnvironment}");
#endif
			return config;
		}
	}
}
