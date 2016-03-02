using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AServerQuery;
using System.Net;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new LogListener();

            listener.AddServer(CreateServer("10.0.0.2", 27015, "5"));
            listener.AddServer(CreateServer("10.0.0.2", 27016, "6"));

            foreach (var s in listener.Servers)
            {
                //s.ChallengeRcon();
                //if (!s.GetLogAddresses().Contains(new IPEndPoint(IPAddress.Parse("10.0.0.1"), 7131)))
                //{
                //    s.AddLogAddress(new IPEndPoint(IPAddress.Parse("10.0.0.1"), 7131));
                //}
            }

            listener.Listen(7131);

            Console.Read();
        }

        static GoldSrcServer CreateServer(string ip, int port, string rconPassword)
        {
            var server = new GoldSrcServer(new IPEndPoint(IPAddress.Parse(ip), port), rconPassword);

            server.Exception += new EventHandler<ExceptionEventArgs>(server_OnException);
            server.Attack += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
            server.BadRcon += new EventHandler<RconEventArgs>(x_OnRcon);
            server.ChangeName += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.Chat += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.TeamChat += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.Connection += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.Cvar += new EventHandler<CvarEventArgs>(x_OnCvar);
            server.CvarEnd += new EventHandler<InfoEventArgs>(x_OnInfo);
            server.CvarStart += new EventHandler<InfoEventArgs>(x_OnInfo);
            server.Disconnection += new EventHandler<PlayerEventArgs>(x_OnPlayer);
            server.EnterGame += new EventHandler<PlayerEventArgs>(x_OnPlayer);
            server.Kick += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.Kill += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
            server.LogFileClosed += new EventHandler<InfoEventArgs>(x_OnInfo);
            server.LogFileStart += new EventHandler<InfoEventArgs>(x_OnInfo);
            server.MapLoading += new EventHandler<ServerEventArgs>(x_OnServer);
            server.MapStarting += new EventHandler<ServerEventArgs>(x_OnServer);
            server.PlayerAction += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.PlayerOnPlayer += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
            server.PlayerScore += new EventHandler<PlayerScoreEventArgs>(x_OnPlayerScore);
            server.PrivateChat += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
            server.Rcon += new EventHandler<RconEventArgs>(x_OnRcon);
            server.RoleSelection += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.ServerName += new EventHandler<ServerEventArgs>(x_OnServer);
            server.ServerSay += new EventHandler<ServerEventArgs>(x_OnServer);
            server.Suicide += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.TeamAction += new EventHandler<TeamEventArgs>(x_OnTeamAction);
            server.TeamAlliance += new EventHandler<TeamEventArgs>(x_OnTeamAction);
            server.TeamScore += new EventHandler<TeamScoreEventArgs>(x_OnTeamScore);
            server.TeamSelection += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.Validation += new EventHandler<PlayerEventArgs>(x_OnPlayer);
            server.WeaponPickup += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.WeaponSelection += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
            server.WorldAction += new EventHandler<ServerEventArgs>(x_OnServer);

            return server;
        }

        static void server_OnException(object sender, ExceptionEventArgs args)
        {
            Console.WriteLine("[{0}] Exception: {1}", ((GoldSrcServer)sender).Server, args.Exp.Message);
        }

        static void x_OnTeamScore(object sender, TeamScoreEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Team: {2}; Score: {3}; NumPlayers: {4}; CountProps: {5}", ((GoldSrcServer)sender).Server, args.EventName, args.Team, args.Score, args.NumPlayers, args.Properties.Count);
        }

        static void x_OnTeamAction(object sender, TeamEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Team: {2}; Noun: {3}; CountProps: {4}", ((GoldSrcServer)sender).Server, args.EventName, args.Team, args.Noun, args.Properties.Count);
        }

        static void x_OnPlayerScore(object sender, PlayerScoreEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Player: {2}; Score: {3}; CountProps: {4}", ((GoldSrcServer)sender).Server, args.EventName, args.Triggerer, args.Score, args.Properties.Count);
        }

        static void x_OnServer(object sender, ServerEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Noun: {2}; CountProps: {3}", ((GoldSrcServer)sender).Server, args.EventName, args.Noun, args.Properties.Count);
        }

        static void x_OnPlayer(object sender, PlayerEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Player: {2}; CountProps: {3}", ((GoldSrcServer)sender).Server, args.EventName, args.Triggerer, args.Properties.Count);
        }

        static void x_OnInfo(object sender, InfoEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; CountProps: {2}", ((GoldSrcServer)sender).Server, args.EventName, args.Properties.Count);
        }

        static void x_OnCvar(object sender, CvarEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Cvar: {2}; Value: {3}; CountProps: {4}", ((GoldSrcServer)sender).Server, args.EventName, args.Cvar, args.Value, args.Properties.Count);
        }

        static void x_OnPlayerAction(object sender, PlayerActionEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Player: {2}; Noun: {3}; CountProps: {4}", ((GoldSrcServer)sender).Server, args.EventName, args.Triggerer, args.Noun, args.Properties.Count);
        }

        static void x_OnPlayerOnPlayer(object sender, PlayerOnPlayerEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; Player1: {2}; Player2: {3}; Noun: {4}; CountProps: {5}", ((GoldSrcServer)sender).Server, args.EventName, args.Triggerer, args.Target, args.Noun, args.Properties.Count);
        }

        static void x_OnRcon(object sender, RconEventArgs args)
        {
            Console.WriteLine("[{0}] Event: {1}; IsGood: {2}; Challenge: {3}; Password: {4}; Command: {5}; Sender: {6}; CountProps: {7}", ((GoldSrcServer)sender).Server, args.EventName, args.IsGood, args.Challenge, args.Password, args.Command, args.Sender.ToString(), args.Properties.Count);
        }
    }
}
