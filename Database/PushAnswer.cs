using System;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Xml;

namespace MainCycle.Database
{
	public static class PushAnswer
	{
		static private readonly string procedure = "***********";
		static private string inMsg_id;
		static private string inAnswer;
		static private int inStatus;
		static private string outErrText;
		static private int outErrCode;

		//static private ResponseEntity pushResult;

		public static void SendAnswer( DbEntity record, ResponseEntity pushResult )
		{
#if DEBUG
			Console.WriteLine("Вошли в метод SendAnswer");
#endif
			//pushResult = result;

			inMsg_id = pushResult.MessageId;
			inAnswer = GetResponseXml(pushResult).ToString();
			inStatus = pushResult.Exception == null
							? 1
							: -1;


			OracleConnection conn = DBUtils.GetDBConnection();
#if DEBUG
			Console.WriteLine("Отправляем ответ от сервера PUSH в БД");
#endif

			OracleCommand cmd = new OracleCommand(procedure, conn);
			cmd.CommandType = System.Data.CommandType.StoredProcedure;

			cmd.Parameters.Add(nameof(inMsg_id), OracleDbType.Varchar2);
			cmd.Parameters.Add(nameof(inStatus), OracleDbType.Int32);
			cmd.Parameters.Add(nameof(inAnswer), OracleDbType.Clob);

			cmd.Parameters.Add(new OracleParameter(nameof(outErrCode), OracleDbType.Int32));
			cmd.Parameters.Add(new OracleParameter(nameof(outErrText), OracleDbType.Varchar2));

			cmd.Parameters[nameof(inMsg_id)].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters[nameof(inStatus)].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters[nameof(inAnswer)].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters[nameof(outErrCode)].Direction = System.Data.ParameterDirection.Output;
			cmd.Parameters[nameof(outErrText)].Direction = System.Data.ParameterDirection.Output;

			cmd.Parameters[nameof(inMsg_id)].Value = inMsg_id;
			cmd.Parameters[nameof(inAnswer)].Value = inAnswer;
			cmd.Parameters[nameof(inStatus)].Value = inStatus;

			try
			{
				if ( conn.State != System.Data.ConnectionState.Open )
					conn.Open();
				cmd.ExecuteNonQuery();
#if DEBUG
				Console.WriteLine($"Процедура {procedure} извлечена успешно");
#endif
				inMsg_id = cmd.Parameters[nameof(inMsg_id)].Value.ToString();
				inStatus = Convert.ToInt32(cmd.Parameters[nameof(inStatus)].Value?.ToString());
				inAnswer = cmd.Parameters[nameof(inAnswer)].Value.ToString();
				outErrCode = Convert.ToInt32(cmd.Parameters[nameof(outErrCode)].Value?.ToString());
				outErrText = cmd.Parameters[nameof(outErrText)].Value.ToString();

#if DEBUG
				Console.WriteLine(cmd.Parameters[nameof(inMsg_id)].ParameterName + "\t:\t" + inMsg_id);
				Console.WriteLine(cmd.Parameters[nameof(inStatus)].ParameterName + "\t:\t" + inStatus);
				Console.WriteLine(cmd.Parameters[nameof(inAnswer)].ParameterName + "\t:\t" + inAnswer);
				Console.WriteLine(cmd.Parameters[nameof(outErrCode)].ParameterName + "\t:\t" + outErrCode);
				Console.WriteLine(cmd.Parameters[nameof(outErrText)].ParameterName + "\t:\t" + outErrText);
#endif

				record.outErrCode = Convert.ToInt32(cmd.Parameters[nameof(outErrCode)].Value.ToString());
				record.outErrText = cmd.Parameters[nameof(outErrText)].Value?.ToString();
			}
			catch ( Exception ex )
			{
				Console.WriteLine("Error: " + ex);
				//Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				if ( conn.State != System.Data.ConnectionState.Closed )
					conn.Close();
				conn?.Dispose();

				//ErrorInfoClass.Dispose(responseEntity);
				//if ( ErrorInfoClass.ResponseEntities.All(en => en.Equals(responseEntity)) )
				//{
				//	ErrorInfoClass.ResponseEntities = new System.Collections.Generic.List<ResponseEntity>();
				//}
			}
		}

		private static string GetResponseXml( ResponseEntity input )
		{
			const string rootSection = "response";
			const string newPushIdSection = "new_push_id";
			const string oldPushIdSection = "old_push_id";
			const string serviceNameSection = "service_name";
			const string messageIdSection = "message_id";
			const string errorDescriptionSection = "error_message";
			const string errorTypeSection = "error_type";
			const string errorCodeSection = "error_code";

			string resultXml = string.Empty;

			string messageId =		$"<{messageIdSection}>{input.MessageId}</{messageIdSection}>";
			string serviceName =	$"<{serviceNameSection}>{input.PushServiceName}</{serviceNameSection}>";
			string oldPushId =		$"<{oldPushIdSection}>{input.OldPushId}</{oldPushIdSection}>";
			string newPushId =		$"<{newPushIdSection}>{input.NewPushId}</{newPushIdSection}>";
			string errortype =		$"<{errorTypeSection}>{input.ExceptionType}</{errorTypeSection}>";
			string errorMessage =	$"<{errorDescriptionSection}>{input.ExceptionMessage}</{errorDescriptionSection}>";
			string errorCode =		$"<{errorCodeSection}>{input.ResultCode}</{errorCodeSection}>";


			resultXml = $"<{rootSection}>" +
											$"{messageId}" +
											$"{serviceName}" +
											$"{oldPushId}" +
											$"{newPushId}" +
											$"{errortype}" +
											$"{errorMessage}" +
											$"{errorCode}" +
						$"</{rootSection}>";

			return resultXml;
		}

		private static XmlDocument GetResponseXml2( ResponseEntity input )
		{
			var xmlDoc = new XmlDocument();
			XmlElement rootElement, element;

			rootElement = xmlDoc.CreateElement("response");
			xmlDoc.AppendChild(rootElement);

			element = xmlDoc.CreateElement("new_push_id");
			element.InnerText = input.NewPushId;
			rootElement.AppendChild(element);

			element = xmlDoc.CreateElement("old_push_id");
			element.InnerText = input.OldPushId;
			rootElement.AppendChild(element);

			element = xmlDoc.CreateElement("service_name");
			element.InnerText = input.PushServiceName;
			rootElement.AppendChild(element);

			element = xmlDoc.CreateElement("message_id");
			element.InnerText = input.MessageId;
			rootElement.AppendChild(element);

			element = xmlDoc.CreateElement("error_message");
			element.InnerText = input.ExceptionMessage;
			rootElement.AppendChild(element);

			element = xmlDoc.CreateElement("error_type");
			element.InnerText = input.ExceptionType;
			rootElement.AppendChild(element);

			element = xmlDoc.CreateElement("error_code");
			element.InnerText = input.ResultCode.ToString();
			rootElement.AppendChild(element);

			return xmlDoc;
		}

		//public static void SendAnswer( string responseMessage, DbEntity record, bool isApns )
		//{
		//	iMsg_id_value = record.Message_id;
		//	iAnswer_value = responseMessage;
		//	iStatus_Value = string.Equals(responseMessage, "Success")
		//					? 1
		//					: -1;


		//	OracleConnection conn = DBUtils.GetDBConnection();
		//	Console.WriteLine("Отправляем ответ от сервера PUSH в БД");

		//	OracleCommand cmd = new OracleCommand(procedure, conn);
		//	cmd.CommandType = System.Data.CommandType.StoredProcedure;

		//	cmd.Parameters.Add("iMsg_id", OracleDbType.Varchar2);
		//	cmd.Parameters.Add("iStatus", OracleDbType.Int32);
		//	cmd.Parameters.Add("iAnswer", OracleDbType.NVarchar2);

		//	cmd.Parameters.Add(new OracleParameter("outErrCode", OracleDbType.Int32));
		//	cmd.Parameters.Add(new OracleParameter("outErrText", OracleDbType.Varchar2));

		//	cmd.Parameters["iMsg_id"].Direction = System.Data.ParameterDirection.Input;
		//	cmd.Parameters["iStatus"].Direction = System.Data.ParameterDirection.Input;
		//	cmd.Parameters["iAnswer"].Direction = System.Data.ParameterDirection.Input;
		//	cmd.Parameters["outErrCode"].Direction = System.Data.ParameterDirection.Output;
		//	cmd.Parameters["outErrText"].Direction = System.Data.ParameterDirection.Output;

		//	cmd.Parameters["iMsg_id"].Value = iMsg_id_value;
		//	cmd.Parameters["iAnswer"].Value = iAnswer_value;
		//	cmd.Parameters["iStatus"].Value = iStatus_Value;

		//	try
		//	{
		//		if ( conn.State != System.Data.ConnectionState.Open )
		//			conn.Open();
		//		cmd.ExecuteNonQuery();
		//		string iMsg_id = cmd.Parameters["iMsg_id"].Value.ToString();
		//		string iStatus = cmd.Parameters["iStatus"].Value.ToString();
		//		string iAnswer = cmd.Parameters["iAnswer"].Value.ToString();
		//		string outErrCode = cmd.Parameters["outErrCode"].Value.ToString();
		//		string outErrText = cmd.Parameters["outErrText"].Value.ToString();

		//		Console.WriteLine(cmd.Parameters["iMsg_id"].ParameterName + "\t:\t" + iMsg_id);
		//		Console.WriteLine(cmd.Parameters["iStatus"].ParameterName + "\t:\t" + iStatus);
		//		Console.WriteLine(cmd.Parameters["iAnswer"].ParameterName + "\t:\t" + iAnswer);
		//		Console.WriteLine(cmd.Parameters["outErrCode"].ParameterName + "\t:\t" + outErrCode);
		//		Console.WriteLine(cmd.Parameters["outErrText"].ParameterName + "\t:\t" + outErrText);

		//		record.outErrCode = Convert.ToInt32(cmd.Parameters["outErrCode"].Value.ToString());
		//		record.outErrText = cmd.Parameters["outErrText"].Value?.ToString();
		//	}
		//	catch ( Exception ex )
		//	{
		//		Console.WriteLine("Error: " + ex);
		//		//Console.WriteLine(ex.StackTrace);
		//	}
		//	finally
		//	{
		//		if ( conn.State != System.Data.ConnectionState.Closed )
		//			conn.Close();
		//		conn?.Dispose();
		//	}
		//}		
	}
}
