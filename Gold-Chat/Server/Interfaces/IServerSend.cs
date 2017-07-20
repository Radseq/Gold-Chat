using CommandClient;

namespace Server.ResponseMessages
{
    interface IServerSend
    {
        Data Send { get; set; }
    }
}
