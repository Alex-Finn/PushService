using System.Collections.Generic;

namespace MainCycle.Database
{
	public class PushNotificationsLists
	{
		public List<DbEntity> Notifications { get; set; }

		public PushNotificationsLists()
		{
			Notifications = new List<DbEntity>();
		}


		public static List<DbEntity> ListNotificationsForSend { get; set; } = new List<DbEntity>();
		public static List<DbEntity> ListIosNotifications { get; set; } = new List<DbEntity>();
		public static List<DbEntity> ListAndroidNotifications { get; set; } = new List<DbEntity>();
		
	}
}
