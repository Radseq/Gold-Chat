using Autofac;
using Server.Controllers.Lists;
using Server.Interfaces.ClientLists;
using Server.Modules.ListsToUserModule;
using Server.Utilies;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Server
{
    public class server
    {
        public Socket ServerSocket { get; set; }

        //DataBaseManager db = new DataBaseManager();

        private bool runServer = true;

        byte[] byteData = new byte[1024];

        public const int max_users = 50;

        static string dateFile = DateTime.Now.ToString("dd_MM_yyyy");

        static LoggerToFile servLogg = LoggerToFile.Instance;
        ServerManager sm = new ServerManager();

        public static IContainer _container;

        // LifeCycle Container
        // create on start app => like main method
        // load object with sole logic app
        // dispose container when main is exiting (on close app)


        public server()
        {
            try
            {
                var builder = new ContainerBuilder();
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                //get whole class and create 
                builder.RegisterAssemblyTypes(executingAssembly)
                    .AsSelf()
                    .AsImplementedInterfaces();

                // this 3 classes have same implementation, there we chooise implementation class
                builder.RegisterType<GetJoinedChannelsList>()
                    .As<IClientLists>()
                    .AsSelf();
                builder.RegisterType<GetFriendsList>()
                    .As<IClientLists>()
                    .AsSelf();
                builder.RegisterType<GetIgnoredUsersList>()
                    .As<IClientLists>()
                    .AsSelf();

                builder.Register(ctx => new ClientJoinedChannelsListModule(ctx.Resolve<GetJoinedChannelsList>(), ctx.Resolve<ListOfStringToStringWithSeparators>()));
                builder.Register(ctx => new ClientFriendsListModule(ctx.Resolve<GetFriendsList>(), ctx.Resolve<ListOfStringToStringWithSeparators>()));
                builder.Register(ctx => new ClientIgnoredUsersListModule(ctx.Resolve<GetIgnoredUsersList>()));
                //end
                //var container = builder.Build();

                _container = builder.Build();

                ServerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                ServerSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                ServerSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, Settings.SERVER_PORT));
                ServerSocket.Listen(5);

                Console.WriteLine(" >> Server Started");
                servLogg.Log(" >> Server Started"); /*on Adress:" + ((IPEndPoint)ServerSocket.RemoteEndPoint).Address.ToString() + " Port:" + ((IPEndPoint)ServerSocket.RemoteEndPoint).Port.ToString());*/

                //sm.ClientLogin += OnClientLogin;
                //sm.ClientLogin += servLogg.OnClientLoginLogger;
                //sm.ClientRegistration += OnClientRegistration;
                //sm.ClientReSendAckCode += OnClientReSendAckCode;
                //emailSender.EmailSended += servLogg.OnEmaiSended;
                //emailSender.EmailSended += servLogg.OnEmaiNotyficationLoginSended;
                //sm.ClientMessage += OnClientMessage;
                //sm.ClientMessage += servLogg.OnClientMessageLogger;
                //sm.ClientSendMessage += OnClientSendMessage;
                //sm.ListOfClientsOnline += OnClientList;
                //sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                //sm.ClientReceiMessage += OnClientReceiMessage;
                //sm.ClientReceiMessage += servLogg.OnClientReceiMessageLogger;
                // DataBase
                //db.ConnectToDB += servLogg.OnConnectToDB;
                //db.ExecuteNonQuery += servLogg.OnExecuteNonQuery;
                //db.ExecuteReader += servLogg.OnExecuteReader;

                while (runServer)
                {
                    sm.getConnection(ServerSocket);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                servLogg.RunServerLogger(ex);
            }
        }

        private void ConfigureInstancesInContainer()
        {
            // override default registration if needed
            //builder.RegisterType<EmailValidator>()
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            // If someome ask autofu ..ck :D for instance of xxx, return xxx as singleton
            //builder.RegisterType<DataBaseManager>()
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            //builder.Register(cc =>
            //{
            //    var dbConnection = new SqlConnection();
            //    var transaction = dbConnection.BeginTransaction();

            //    var ctx = cc.Resolve<OperationContext>();
            //    ctx.Transaction = transaction;

            //    return dbConnection;
            //})
            //.As<IDbConnection>()
            //.InstancePerLifetimeScope();
        }

        public void Shutdown()
        {
            runServer = false;
            ServerSocket.Close();
            _container.Dispose();
            /*while (clientList.Count > 0)
            {
                clientList[0].KeepProcessing = false;
                clientList[0].cSocket.Close();
                clientList.RemoveAt(0);
            }*/
            //strWriter.Close();
            //strWriter.Dispose();
            //db.closeConnection();
        }

        #region event messages
        private void OnClientReceiMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("ReceivedMessage " + e.clientMessageReciv);
        }

        private void OnClientSendMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("SendMessage " + e.clientMessageToSend);
        }

        private void OnClientList(object sender, ClientEventArgs e)
        {
            Console.WriteLine("SendList " + e.clientMessageToSend + " " + e.clientMessageTwoToSend);
        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("clientMessage " + e.clientMessageReciv);
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has left the room>>>");
        }
        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            server serv = new server(); //after changes on this and ServerManager i think this class can be deleted

            Console.ReadLine();
            serv.Shutdown();


        }
    }
}