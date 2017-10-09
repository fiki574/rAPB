using FrameWork;
using FrameWork.Logger;
using System;
using System.Collections.Generic;
using System.Timers;

namespace LobbyServer
{
    class Program
    {
        ///static variables for initalization
        static public World.Listener worldListener;
        static public List<byte> worlds = new List<byte>();
        static public List<LobbyClient> clients = new List<LobbyClient>();

        /// <summary>
        /// Changes the programs behaviour to run as a single thread application, moving through the code one thing at a time.
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            ///Runs on startup
            Log.Info("LobbyServer", "Starting...");
            
            ///Check if the config is loaded, kill program if not.
            if (!EasyServer.InitLog("Lobby", "Configs/Logs.conf") 
                || !EasyServer.InitConfig("Configs/Lobby.xml", "Lobby") 
                || !EasyServer.InitConfig("Configs/Database.xml", "Database")
                )
            {
                return;
            }
            
            ///Check if the server can start using the configured ip address, port and name, if not, kill program.
            if (!EasyServer.Listen<TcpServer>(EasyServer.GetConfValue<int>("Lobby", "LoginServer", "Port"), "LoginServer"))
            {
                return;
            }
            
            ///start up a new world listener.
            worldListener = new World.Listener(EasyServer.GetConfValue<string>("Lobby", "WorldListener", "IP"), EasyServer.GetConfValue<int>("Lobby", "WorldListener", "Port"));

            ///Initalize the database
            Databases.InitDB();
            ///Load the database variables into memory
            Databases.Load(true);

            ///Try to launch the HTTP server to handle user account creation.
            try
            {
                HttpServer.MapHandlers();
                HttpServer server = new HttpServer();
                server.Start();
            }
            catch
            {
                Log.Error("HTTP", "If you want to use HTTP stuff, start this server with Admin rights");
            }

            ///If all the initalisation is complete, spit out the complete message.
            Log.Succes("LobbyServer", "Server initialisation complete!");

            ///Clear the clients and worlds from the list to make sure no residue of previous session remains.
            clients.Clear();
            worlds.Clear();
            
            ///Start a new timer called aTimer that prevents the database from being loaded
            Timer aTimer = new Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            ///Empty space in the log file.
            Log.Enter();
            Console.WriteLine("For available console commands, type /commands");
            Log.Enter();

            ///prevents program from sleeping //infinite loop?
            bool done = false;
            while (!done)
            {
                string command;
                command = Console.ReadLine();
                ProccessCommand(command);
            }
            EasyServer.StartConsole();
        }

        /// <summary>
        /// Processes user input commands
        /// </summary>
        /// <param name="command"></param>
        static void ProccessCommand(string command)
        {
            switch (command)
            {
                ///A list of all available commands.
                case "/commands":
                case "/help":
                    Log.Enter();
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("/commands        - shows this list of commands");
                    Console.WriteLine("/help            - shows this list of commands");
                    Console.WriteLine("/shutdown        - shuts down server");
                    Console.WriteLine("/clients         - total clients connected");
                    Console.WriteLine("/worlds          - total world servers connected");
                    Console.WriteLine("/clear console   - clear console from logs");
                    break;

                 ///Shut down the server using Enviroment.Exit(2)
                case "/shutdown":
                    Console.WriteLine("Login server shutting down in 3 seconds...");
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Login server shutting down in 2 seconds...");
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Login server shutting down in 1 second....");
                    System.Threading.Thread.Sleep(1000);
                    Environment.Exit(2);
                    break;

                ///Check each LobbyClient, to see if they are connected to the server, if so, show that that client is connected, with their ID, Name and Administrator status
                case "/clients":
                    int count = 0;
                    foreach (LobbyClient client in clients)
                    {
                        count++;
                        Console.WriteLine("Name: " + client.Account.Username);
                        Console.WriteLine("ID: " + count);
                        Console.WriteLine("isGM: " + client.Account.IsAdmin);
                    }
                    Log.Enter();
                    Console.WriteLine("Total clients connected to login server: " + count);
                    count = 0;
                    break;
                ///Check how many world servers are connected and display this to the user
                case "/worlds":
                    Console.WriteLine("Total worlds connected to login server: " + worlds.Count);
                    break;
                ///Clears the console.
                case "/clear console":
                    Console.Clear();
                    break;
                ///Default case in case that an invalid command is entered.
                default:
                    Console.WriteLine("ERROR: Unknown command \"" + command + "\"");
                    Console.WriteLine("Use /commands to show all available commands");
                    Console.WriteLine("If you believe this is a error, please notify us.");
                    break;
            }
        }

        /// <summary>
        /// Disables the database from loading when an timed event is triggered.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Databases.Load(false);
        }
    }
}
