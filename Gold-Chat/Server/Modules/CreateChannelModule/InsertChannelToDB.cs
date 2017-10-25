using Server.Interfaces;
using Server.Interfaces.CreateChannel;
using Server.Utilies;

namespace Server.Modules.CreateChannelModule
{
    class InsertChannelToDB : IInsertChannel
    {
        private readonly IDataBase DataBase;

        public InsertChannelToDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public int Insert(Client client, string roomName, string enterPassword, string adminPassword, int maxEnteredUsers, string welcomeMsg)
        {
            DataBase.bind(new string[] { "idUser", client.id.ToString(), "channelName", roomName, "enterPass", enterPassword, "adminPass",
                adminPassword, "maxUsers", maxEnteredUsers.ToString(), "createDate", Utilities.getDataTimeNow(), "welcomeMessage", welcomeMsg });
            return DataBase.executeNonQuery("INSERT INTO channel (id_user_founder, channel_name, enter_password, admin_password, max_users, create_date, welcome_Message) " +
                "VALUES (@idUser, @channelName, @enterPass, @adminPass, @maxUsers, @createDate, @welcomeMessage)");
        }
    }
}
