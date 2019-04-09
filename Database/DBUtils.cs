using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace MainCycle.Database
{
	public class DBUtils
	{
		public static OracleConnection GetDBConnection()
		{
			string host = "*********";
			int port = **********;
			string sid = "**********";
			string user = "************";
			string password = "**********";

			return DBOracleUtils.GetDBConnection(host, port, sid, user, password);
		}
	}
}
