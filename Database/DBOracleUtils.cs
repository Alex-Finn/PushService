using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace MainCycle.Database
{
	class DBOracleUtils
	{
		public static OracleConnection GetDBConnection( 
			string host, 
			int port,
			string sid,
			string user,
			string password )
		{
#if DEBUG       
			Console.Write("Сonnection to database...\t");
#endif
			// 'Connection String' подключается напрямую к Oracle.
			string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
				 + host + ")(PORT = " + port + "))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = "
				 + sid + ")));Password=" + password + ";User ID=" + user;


			OracleConnection conn = new OracleConnection();

			conn.ConnectionString = connString;

			return conn;
		}
	}
}
