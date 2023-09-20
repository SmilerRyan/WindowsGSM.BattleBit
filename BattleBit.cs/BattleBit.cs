﻿using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;

namespace WindowsGSM.Plugins
{
    public class BattleBit : SteamCMDAgent
    {
		// - Plugin Details
        public Functions.Plugin Plugin = new Functions.Plugin
        {
            name = "WindowsGSM.BattleBit", // WindowsGSM.XXXX
            author = "SmilerRyan",
            description = "🧩 WindowsGSM plugin for supporting BattleBit Servers",
            version = "1.0",
            url = "https://github.com/SmilerRyan/WindowsGSM.BMS", // Github repository link (Best practice)
            color = "#9eff99" // Color Hex
        };

        // - Standard Constructor and properties
		public BattleBit(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;

		// - Settings properties for SteamCMD installer
        public override bool loginAnonymous => false;
        public override string AppId => "671860"; // Game server appId

        // - Game server Fixed variables
        public override string StartPath => "BattleBit.exe"; // Game server start path
        public string FullName = "BattleBit Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

        // - Game server default values
		public string Port { get { return "27015"; } } // Default port
        public string QueryPort { get { return "27015"; } } // Default query port
        public string Game { get { return "BattleBit"; } } // Default game name
        public string Defaultmap { get { return "bm_c1a0"; } } // Default map name
        public string Maxplayers { get { return string.Empty; } } // Default maxplayers
        public string Additional { get { return "-nographics -batchmode"; } } // Additional server start parameter

		// - Create a default cfg for the game server after installation
		public async void CreateServerCFG()
        {
            
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string path = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, StartPath);
            if (!File.Exists(path))
            {
                Error = $"{StartPath} not found ({path})";
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"-nographics -batchmode");
            sb.Append(string.IsNullOrWhiteSpace(serverData.ServerParam) ? string.Empty : $" {serverData.ServerParam}");
            string param = sb.ToString();

            Process p;
            if (!AllowsEmbedConsole)
            {
                p = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = Functions.ServerPath.GetServersServerFiles(serverData.ServerID),
                        FileName = path,
                        Arguments = param,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true
                };
                p.Start();
            }
            else
            {
                p = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = Functions.ServerPath.GetServersServerFiles(serverData.ServerID),
                        FileName = path,
                        Arguments = param,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        UseShellExecute = false,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                var serverConsole = new Functions.ServerConsole(serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }

        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "quit");
            });
        }
    }
}
