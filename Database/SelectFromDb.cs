using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace MainCycle.Database
{
	public class SelectFromDb
	{
		private readonly string procedure1 =
						"*****************";
		public bool SelectFromDbMethod()
		{
			DbEntity result;
			PushNotificationsLists.ListNotificationsForSend.Clear();

			try
			{
				OracleConnection conn = DBUtils.GetDBConnection();
				// Создать объект Command.
				OracleCommand cmd = new OracleCommand();

				// Сочетать Command с Connection.
				cmd.Connection = conn;
				cmd.CommandText = procedure1;
				
				try
				{
					if ( conn?.State != System.Data.ConnectionState.Open )
						conn.Open();

					using ( DbDataReader reader = cmd.ExecuteReader() )
					{
						if ( reader.HasRows )
						{		
							int numberOfColumns = reader.FieldCount;

							while ( reader.Read() )
							{
								result = new DbEntity();

								for ( int i = 0 ; i < numberOfColumns ; i++ )
								{
									string nameOfColumn = reader.GetName(i);
									int indexOfFiled = reader.GetOrdinal(nameOfColumn);
									object fieldValue = reader.GetValue(indexOfFiled);

									switch ( nameOfColumn )
									{
										case "MSG_ID":		result.Message_id = fieldValue.ToString();		break;
										case "MSG_TEXT":	result.Message_text = fieldValue.ToString();	break;
										case "PUSH_ID":		result.Push_id = fieldValue.ToString();			break;
										case "PHONE":		result.Phone = fieldValue.ToString();			break;
										case "SERVICE_ID":	result.Service_id = fieldValue.ToString();		break;
										case "SERVICE_NAME":result.Service_name = fieldValue.ToString();	break;
										case "MSG_TTL":		result.Msg_Ttl = Convert.ToInt32(fieldValue);	break;
										default:
											Console.WriteLine($"Неизвестная ошибка. Поле [{nameOfColumn}] не опознано");
											return false;
									}
								}
								
								PushNotificationsLists.ListNotificationsForSend.Add(result);
							}
						}
						else
						{
							
#if DEBUG
							Console.WriteLine("В БД нет новых данных для считывания");
#endif
							return false;
						}
					}
				}
				catch ( Exception ex )
				{
					Console.WriteLine("## ERROR: " + ex.Message);
					return false;
				}
				finally
				{
					cmd.CommandText = "ROLLBACK";
					if ( conn?.State != System.Data.ConnectionState.Open )
						conn.Open();
					cmd.ExecuteNonQuery();

					if ( conn.State != System.Data.ConnectionState.Closed )
						conn.Close();
					conn?.Dispose();
				}

#if DEBUG
				Console.WriteLine("\n=====================\r\n============List of Android entities: =========");
				foreach ( var item in
					PushNotificationsLists
					.ListNotificationsForSend
					.Where(el => el.Service_name == "FCM") )
				{

					Console.WriteLine("=================");
					Console.WriteLine($"{nameof(item.Message_id)}	:\t	{item.Message_id}");
					Console.WriteLine($"{nameof(item.Message_text)} :\t	{item.Message_text}");
					Console.WriteLine($"{nameof(item.Push_id)}		:\t	{item.Push_id}");
					Console.WriteLine($"{nameof(item.Phone)}		:\t	{item.Phone}");
					Console.WriteLine($"{nameof(item.Service_id)}	:\t	{item.Service_id}");
					Console.WriteLine($"{nameof(item.Service_name)}	:\t	{item.Service_name}");
					Console.WriteLine($"{nameof(item.Msg_Ttl)}		:\t	{item.Msg_Ttl}");

				}
				Console.WriteLine("\n=====================\r\n============End of the List of Android ========");

				Console.WriteLine("\n=====================\r\n================List of IOs entities: =========");
				foreach ( var item in
					PushNotificationsLists
					.ListNotificationsForSend
					.Where(el => el.Service_name == "APNS") )
				{

					Console.WriteLine("=================");
					Console.WriteLine($"{nameof(item.Message_id)}	:\t	{item.Message_id}");
					Console.WriteLine($"{nameof(item.Message_text)} :\t	{item.Message_text}");
					Console.WriteLine($"{nameof(item.Push_id)}		:\t	{item.Push_id}");
					Console.WriteLine($"{nameof(item.Phone)}		:\t	{item.Phone}");
					Console.WriteLine($"{nameof(item.Service_id)}	:\t	{item.Service_id}");
					Console.WriteLine($"{nameof(item.Service_name)}	:\t	{item.Service_name}");
					Console.WriteLine($"{nameof(item.Msg_Ttl)}		:\t	{item.Msg_Ttl}");

				}
				Console.WriteLine("\n=====================\r\n============End of the List of IOs ============");
				Console.WriteLine($"Количество записей для отправки: {PushNotificationsLists.ListNotificationsForSend.Count}");
#endif
				return PushNotificationsLists.ListNotificationsForSend.Count != 0;
			}

			catch ( Exception ex )
			{
				Console.WriteLine($"Error: Exception.Message = [{ex.Message}], Exception.StackTrace = [{ex.StackTrace}]");
				return false;
			}
		}		
	}
}
