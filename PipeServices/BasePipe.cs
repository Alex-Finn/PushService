using System;
using Oracle.ManagedDataAccess.Client;
using MainCycle.Database;

namespace MainCycle.PipeServices
{
	public static class BasePipe
	{
		static readonly string procedure = "*********";
		static readonly string pipe_name = "**********";
		static readonly int pipe_timeout_delay = 10;

		public static bool ListenPipeMethod()
		{
#if DEBUG
			Console.WriteLine("Вошли в метод ListenPipeMethod");
#endif
			bool SuccessFlag = false;

			OracleConnection conn = DBUtils.GetDBConnection();
#if DEBUG
			Console.WriteLine("Инициализация именованного канала");
#endif

			OracleCommand cmd = new OracleCommand(procedure, conn);
			cmd.CommandType = System.Data.CommandType.StoredProcedure;

			cmd.Parameters.Add("pipe_name", OracleDbType.Varchar2).Value = pipe_name;
			cmd.Parameters.Add("TIMEOUT", OracleDbType.Int32).Value = pipe_timeout_delay;   //	длительность прослушки пайпа
			cmd.Parameters.Add("msg", OracleDbType.Varchar2).Value = null;

			cmd.Parameters.Add(new OracleParameter("rc", OracleDbType.Int32));
			cmd.Parameters.Add(new OracleParameter("otext", OracleDbType.Varchar2, 4000));

			cmd.Parameters["pipe_name"].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters["TIMEOUT"].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters["msg"].Direction = System.Data.ParameterDirection.Input;
			cmd.Parameters["rc"].Direction = System.Data.ParameterDirection.Output;
			cmd.Parameters["otext"].Direction = System.Data.ParameterDirection.Output;

			try
			{
				if ( conn.State != System.Data.ConnectionState.Open )
					conn.Open();
#if DEBUG
				Console.WriteLine($"\nСоединяю с пайпом \"{pipe_name}\". Ждём {pipe_timeout_delay}c до таймаута");
#endif

				cmd.ExecuteNonQuery();
				int rc = Convert.ToInt32(cmd.Parameters["rc"].Value.ToString());
				string otext = cmd.Parameters["otext"].Value.ToString();

				if ( rc == 0 )
				{
					Console.WriteLine($"Пайп принял сигнал. Переход к считыванию данных");
				}

#if DEBUG
				if ( rc == 0 )

					System.Console.Beep();


				Console.WriteLine(cmd.Parameters["rc"].ParameterName + " : " + rc.ToString());
				Console.WriteLine(cmd.Parameters["otext"].ParameterName + " : " + otext);
#endif
				SuccessFlag =	( rc == 0 )
								? true
								: false;
			}
			catch ( Exception ex )
			{
				Console.WriteLine("Error: " + ex);
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				if ( conn.State != System.Data.ConnectionState.Closed )
					conn.Close();
				conn?.Dispose();
			}
			return SuccessFlag;
		}
	}
}
