using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace MainCycle.Database
{
	public class SetInvalidApnsPushId
	{
		private readonly string procedure = "*************";
		private string inAnswer;
		private string outErrText;
		private int outErrCode;

		public SetInvalidApnsPushId()
		{}

		public void SendToDb( List<InvalidPush> input )
		{
#if DEBUG
			Console.WriteLine("Вошли в метод SendToDb");
#endif

			inAnswer = input.ToString();

			OracleConnection conn = DBUtils.GetDBConnection();
#if DEBUG
			Console.WriteLine("Отправляем ответ ***************в БД");
#endif

			OracleCommand cmd = new OracleCommand(procedure, conn);
			cmd.CommandType = System.Data.CommandType.StoredProcedure;

			cmd.Parameters.Add(nameof(inAnswer), OracleDbType.Clob);

			cmd.Parameters.Add(new OracleParameter(nameof(outErrCode), OracleDbType.Int32));
			cmd.Parameters.Add(new OracleParameter(nameof(outErrText), OracleDbType.Varchar2));

			cmd.Parameters[nameof(inAnswer)].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters[nameof(outErrCode)].Direction = System.Data.ParameterDirection.Output;
			cmd.Parameters[nameof(outErrText)].Direction = System.Data.ParameterDirection.Output;

			cmd.Parameters[nameof(inAnswer)].Value = inAnswer;

			try
			{
				if ( conn.State != System.Data.ConnectionState.Open )
					conn.Open();
				cmd.ExecuteNonQuery();
#if DEBUG
				Console.WriteLine($"Процедура {procedure} извлечена успешно");
#endif
				inAnswer = cmd.Parameters[nameof(inAnswer)].Value.ToString();
				outErrCode = Convert.ToInt32(cmd.Parameters[nameof(outErrCode)].Value?.ToString());
				outErrText = cmd.Parameters[nameof(outErrText)].Value.ToString();

#if DEBUG
				Console.WriteLine(cmd.Parameters[nameof(inAnswer)].ParameterName + "\t:\t" + inAnswer);
				Console.WriteLine(cmd.Parameters[nameof(outErrCode)].ParameterName + "\t:\t" + outErrCode);
				Console.WriteLine(cmd.Parameters[nameof(outErrText)].ParameterName + "\t:\t" + outErrText);
#endif
#if !DEBUG
				if( outErrCode != 0)
					Console.WriteLine(cmd.Parameters[nameof(outErrText)].ParameterName + "\t:\t" + outErrText);
#endif
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
			}
		}
	}
}