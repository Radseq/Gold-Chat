using CommandClient;

namespace Server.ResponseMessages
{
    interface IServerReceive
    {
        Data Received { get; set; }
    }
}
