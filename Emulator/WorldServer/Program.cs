﻿using FrameWork;
using FrameWork.Logger;
using System;
using System.Collections.Generic;
using System.Reflection;
using WorldServer.Districts;
using WorldServer.Lobby;
using WorldServer.RpcFile;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;

namespace WorldServer
{
    class Program
    {
        static public FileManager FileMgr;
        static public Dictionary<UInt32, Acc> expectingAccounts = new Dictionary<UInt32, Acc>();
        static public Districts.Listener districtsListener;
        static public List<WorldClient> clients = new List<WorldClient>();

        #region WorldData

        static public Byte ID;
        static public UInt32 WorldId;
        static public String WorldName;
        static public String Password;

        #region IPAddress

        static public Byte IP1;
        static public Byte IP2;
        static public Byte IP3;
        static public Byte IP4;

        #endregion

        static public Int32 Port;

        #endregion

        #region WorldLobbyCommunication

        static public Lobby.Client Lobby;

        #endregion

        static void Main(string[] args)
        {
            Log.Info("WorldServer", "Starting...");
            if (!EasyServer.InitLog("World", "Configs/Logs.conf") || !EasyServer.InitConfig("Configs/World.xml", "World") || !EasyServer.InitConfig("Configs/Database.xml", "Database")) return;
            Port = EasyServer.GetConfValue<int>("World", "WorldServer", "Port");
            IP1 = EasyServer.GetConfValue<Byte>("World", "WorldServer", "IP1");
            IP2 = EasyServer.GetConfValue<Byte>("World", "WorldServer", "IP2");
            IP3 = EasyServer.GetConfValue<Byte>("World", "WorldServer", "IP3");
            IP4 = EasyServer.GetConfValue<Byte>("World", "WorldServer", "IP4");
            if (!EasyServer.Listen<TcpServer>(Port, "WorldInfo")) return;
            Databases.InitDB();
            Databases.Load(false);
            FileMgr = new FileManager();
            Password = EasyServer.GetConfValue<string>("World", "LobbyCommunication", "Password");
            WorldName = EasyServer.GetConfValue<string>("World", "WorldInfo", "Name");
            ID = EasyServer.GetConfValue<Byte>("World", "WorldInfo", "Id");
            Lobby = new Lobby.Client(EasyServer.GetConfValue<String>("World", "LobbyCommunication", "Ip"), EasyServer.GetConfValue<int>("World", "LobbyCommunication", "Port"));
            districtsListener = new Districts.Listener(EasyServer.GetConfValue<String>("World", "DistrictListener", "Ip"), EasyServer.GetConfValue<int>("World", "DistrictListener", "Port"));
            clients.Clear();
            Timer aTimer = new Timer(10000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            Log.Enter();
            Console.WriteLine("For available console commands, type /commands");
            Log.Enter();
            bool done = false;
            while (!done)
            {
                string command;
                command = Console.ReadLine();
                ProccessCommand(command);
            }
            EasyServer.StartConsole();
        }
        

        static void ProccessCommand(string command)
        {
            if (command == "/commands")
            {
                Log.Enter();
                Console.WriteLine("Available commands:");
                Console.WriteLine("/commands - shows this list of commands");
                Console.WriteLine("/shutdown - shuts down server");
                Console.WriteLine("/clear clientstatus - clear 'clientstatus' table in database");
                Console.WriteLine("/clear console - clear console from logs");
                Console.WriteLine("/clients - total clients connected");
                Console.WriteLine("/districts - total districts connected");
                Log.Enter();
            }
            else if (command.Contains("/shutdown"))
            {
                Log.Enter();
                Console.WriteLine("World server shutting down in 3 seconds...");
                System.Threading.Thread.Sleep(3000);
                Environment.Exit(2);
            }
            else if (command.Contains("/clear console"))
            {
                Console.Clear();
            }
            else if (command.Contains("/clients"))
            {
                Log.Enter();
                Byte count = 0;
                foreach (WorldClient client in clients)
                {
                    count++;
                    Console.WriteLine("Name: " + client.Character.Name);
                    Console.WriteLine("ID: " + count);
                    Console.WriteLine("isGM: " + client.Account.IsAdmin);
                    Log.Enter();
                }
                Console.WriteLine("Total clients connected to world server: " + count);
                count = 0;
                Log.Enter();
            }
            else if (command.Contains("/districts"))
            {
                Log.Enter();
                Byte count = 0;
                foreach (KeyValuePair<UInt32, District> district in Program.districtsListener.Districts)
                {
                    count++;
                    Console.WriteLine("IP: " + district.Value.IP);
                    Console.WriteLine("Port: " + district.Value.Port.ToString());
                    Console.WriteLine("ID: " + district.Value.Id);
                    Console.WriteLine("Type: " + district.Value.Type);
                    Console.WriteLine("Enforcers: " + district.Value.Enforcers);
                    Console.WriteLine("Criminals: " + district.Value.Criminals);
                    Console.WriteLine("Full: " + district.Value.isFull());
                    Log.Enter();
                }
                Console.WriteLine("Total districts connected to world server: " + count);
                count = 0;
                Log.Enter();
            }
            else Console.WriteLine("ERROR: Unknown command \"" + command + "\"");
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Databases.Load(false);
        }
    }
}
