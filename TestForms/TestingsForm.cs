using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AServerQuery;
using System.Net;

namespace TestForms
{
    public partial class TestingsForm : Form
    {
        GoldSrcServer server;

        static Dictionary<String, String> datas = new Dictionary<String, String> {
            { "001a", "1234log L 01/01/2010 - 01:01:01: Server cvars start (TEST)" },
            { "001b", "1234log L 01/01/2010 - 01:01:01: Server cvar \"var\" = \"value\""},
            { "001c", "1234log L 01/01/2010 - 01:01:01: Server cvars end"},
            { "002a", "1234log L 01/01/2010 - 01:01:01: Log file started (file \"filename\") (game \"game\") (version \"protocol/release/build\")"},
            { "002b", "1234log L 01/01/2010 - 01:01:01: Log file closed" },
            { "003a", "1234log L 01/01/2010 - 01:01:01: Loading map \"map\"" },
            { "003b", "1234log L 01/01/2010 - 01:01:01: Started map \"map\" (CRC \"crc\")" },
            { "004a", "1234log L 01/01/2010 - 01:01:01: Rcon: \"rcon challenge \"password\" kick player \"hello world\"\" from \"255.255.255.255:12345\"" },
            { "004b", "1234log L 01/01/2010 - 01:01:01: Bad Rcon: \"rcon challenge \"password\" kick player \"hello world\"\" from \"255.255.255.255:12345\"" },
            { "005", "1234log L 01/01/2010 - 01:01:01: Server name is \"hostname\"" },
            { "006", "1234log L 01/01/2010 - 01:01:01: Server say \"message\"" },
            { "050", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><>\" connected, address \"255.255.255.255:12345\"" },
            { "050b", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><>\" STEAM USERID validated" },
            { "051", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><>\" entered the game" },
            { "052", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" disconnected" },
            { "052b", "1234log L 01/01/2010 - 01:01:01: Kick: \"PlayerName<15><STEAM_0:1:23456><>\" was kicked by \"Console\" (message \"\")" },
            { "053", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" committed suicide with \"weapon\"" },
            { "054", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" joined team \"NewTeam\"" },
            { "055", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" changed role to \"NewRole\"" },
            { "056", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" changed name to \"New Name\"" },
            { "057", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" killed \"Player2<4><STEAM_0:0:252796><OtherName>\" with \"weapon\"" },
            { "058", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" attacked \"Player2<4><STEAM_0:0:252796><OtherName>\" with \"weapon\" (damage \"damage\")" },
            { "059", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" triggered \"action\" against \"Player2<4><STEAM_0:0:252796><OtherName>\"" },
            { "060", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" triggered \"action\"" },
            { "061", "1234log L 01/01/2010 - 01:01:01: Team \"theTeam\" triggered \"action\"" },
            { "062", "1234log L 01/01/2010 - 01:01:01: World triggered \"action\"" },
            { "063a", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" say \"message\"" },
            { "063b", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" say_team \"message\"" },
            { "064", "1234log L 01/01/2010 - 01:01:01: Team \"theTeam\" formed alliance with team \"otherTeam\"" },
            { "065", "1234log L 01/01/2010 - 01:01:01: Team \"Yellow\" scored \"73\" with \"5\" players (kills \"182\") (kills_unaccounted \"4\") (deaths \"217\") (allies \"<Red><Green>\")" },
            { "066", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" tell \"Player2<4><STEAM_0:0:252796><OtherName>\" message \"message\"" },
            { "067", "1234log L 01/01/2010 - 01:01:01: Player \"Joe<123><654321><123>\" scored \"54\" (kills \"36\") (deaths \"11\")" },
            { "068", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" selected weapon \"weapon\"" },
            { "069", "1234log L 01/01/2010 - 01:01:01: \"PlayerName<15><STEAM_0:1:23456><TeamName>\" acquired weapon \"weapon\"" }
        };

        public TestingsForm()
        {
            InitializeComponent();
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            server = new GoldSrcServer(new System.Net.IPEndPoint(IPAddress.Parse(this.txtIPort.Text.Split(':')[0]), int.Parse(this.txtIPort.Text.Split(':')[1])), String.Empty);

            this.txtConsole.AppendText(String.Format("Ping response: {0}{1}", server.Ping(), Environment.NewLine));
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                server = new GoldSrcServer(new System.Net.IPEndPoint(IPAddress.Parse(this.txtIPort.Text.Split(':')[0]), int.Parse(this.txtIPort.Text.Split(':')[1])), String.Empty);

                server.Exception        += new EventHandler<ExceptionEventArgs>(server_OnException);
                server.Attack           += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
                server.BadRcon          += new EventHandler<RconEventArgs>(x_OnRcon);
                server.ChangeName       += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.Chat             += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.TeamChat         += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.Connection       += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.Cvar             += new EventHandler<CvarEventArgs>(x_OnCvar);
                server.CvarEnd          += new EventHandler<InfoEventArgs>(x_OnInfo);
                server.CvarStart        += new EventHandler<InfoEventArgs>(x_OnInfo);
                server.Disconnection    += new EventHandler<PlayerEventArgs>(x_OnPlayer);
                server.EnterGame        += new EventHandler<PlayerEventArgs>(x_OnPlayer);
                server.Kick             += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.Kill             += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
                server.LogFileClosed    += new EventHandler<InfoEventArgs>(x_OnInfo);
                server.LogFileStart     += new EventHandler<InfoEventArgs>(x_OnInfo);
                server.MapLoading       += new EventHandler<ServerEventArgs>(x_OnServer);
                server.MapStarting      += new EventHandler<ServerEventArgs>(x_OnServer);
                server.PlayerAction     += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.PlayerOnPlayer   += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
                server.PlayerScore      += new EventHandler<PlayerScoreEventArgs>(x_OnPlayerScore);
                server.PrivateChat      += new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer);
                server.Rcon             += new EventHandler<RconEventArgs>(x_OnRcon);
                server.RoleSelection    += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.ServerName       += new EventHandler<ServerEventArgs>(x_OnServer);
                server.ServerSay        += new EventHandler<ServerEventArgs>(x_OnServer);
                server.Suicide          += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.TeamAction       += new EventHandler<TeamEventArgs>(x_OnTeamAction);
                server.TeamAlliance     += new EventHandler<TeamEventArgs>(x_OnTeamAction);
                server.TeamScore        += new EventHandler<TeamScoreEventArgs>(x_OnTeamScore);
                server.TeamSelection    += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.Validation       += new EventHandler<PlayerEventArgs>(x_OnPlayer);
                server.WeaponPickup     += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.WeaponSelection  += new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction);
                server.WorldAction      += new EventHandler<ServerEventArgs>(x_OnServer);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Error: " + exp.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void AppendLine(String format, params object[] args)
        {
            this.txtConsole.AppendText(DateTime.Now.ToString("[dd/MM/yyyy HH:mm:ss] ") + String.Format(format, args) + Environment.NewLine);
        }

        void server_OnException(object sender, ExceptionEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<ExceptionEventArgs>(server_OnException), sender, args);
                return;
            }

            this.AppendLine("Exception: {0}", args.Exp.Message);
        }

        void x_OnTeamScore(object sender, TeamScoreEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<TeamScoreEventArgs>(x_OnTeamScore), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Team: {1}; Score: {2}; NumPlayers: {3}; CountProps: {4}", args.EventName, args.Team, args.Score, args.NumPlayers, args.Properties.Count);
        }

        void x_OnTeamAction(object sender, TeamEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<TeamEventArgs>(x_OnTeamAction), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Team: {1}; Noun: {2}; CountProps: {3}", args.EventName, args.Team, args.Noun, args.Properties.Count);
        }

        void x_OnPlayerScore(object sender, PlayerScoreEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<PlayerScoreEventArgs>(x_OnPlayerScore), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Player: {1}; Score: {2}; CountProps: {3}", args.EventName, args.Triggerer, args.Score, args.Properties.Count);
        }

        void x_OnServer(object sender, ServerEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<ServerEventArgs>(x_OnServer), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Noun: {1}; CountProps: {2}", args.EventName, args.Noun, args.Properties.Count);
        }

        void x_OnPlayer(object sender, PlayerEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<PlayerEventArgs>(x_OnPlayer), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Player: {1}; CountProps: {2}", args.EventName, args.Triggerer, args.Properties.Count);
        }

        void x_OnInfo(object sender, InfoEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<InfoEventArgs>(x_OnInfo), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; CountProps: {1}", args.EventName, args.Properties.Count);
        }

        void x_OnCvar(object sender, CvarEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<CvarEventArgs>(x_OnCvar), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Cvar: {1}; Value: {2}; CountProps: {3}", args.EventName, args.Cvar, args.Value, args.Properties.Count);
        }

        void x_OnPlayerAction(object sender, PlayerActionEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<PlayerActionEventArgs>(x_OnPlayerAction), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Player: {1}; Noun: {2}; CountProps: {3}", args.EventName, args.Triggerer, args.Noun, args.Properties.Count);
        }

        void x_OnPlayerOnPlayer(object sender, PlayerOnPlayerEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<PlayerOnPlayerEventArgs>(x_OnPlayerOnPlayer), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; Player1: {1}; Player2: {2}; Noun: {3}; CountProps: {4}", args.EventName, args.Triggerer, args.Target, args.Noun, args.Properties.Count);
        }

        void x_OnRcon(object sender, RconEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<RconEventArgs>(x_OnRcon), sender, args);
                return;
            }

            this.AppendLine("Event: {0}; IsGood: {1}; Challenge: {2}; Password: {3}; Command: {4}; Sender: {5}; CountProps: {6}", args.EventName, args.IsGood, args.Challenge, args.Password, args.Command, args.Sender.ToString(), args.Properties.Count);
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            try
            {
                var info = server.GetInfo();

                txtConsole.AppendText(String.Format("Type\t\t0x{0:x}{1}", info.Type, Environment.NewLine));
                txtConsole.AppendText(String.Format("Version\t\t{0}{1}", info.Version, Environment.NewLine));
                txtConsole.AppendText(String.Format("ServerName\t{0}{1}", info.ServerName, Environment.NewLine));
                txtConsole.AppendText(String.Format("Map\t\t{0}{1}", info.Map, Environment.NewLine));
                txtConsole.AppendText(String.Format("GameDir\t\t{0}{1}", info.GameDir, Environment.NewLine));
                txtConsole.AppendText(String.Format("GameDesc\t{0}{1}", info.GameDesc, Environment.NewLine));
                txtConsole.AppendText(String.Format("NumPlayers\t{0}{1}", info.NumPlayers, Environment.NewLine));
                txtConsole.AppendText(String.Format("MaxPlayers\t{0}{1}", info.MaxPlayers, Environment.NewLine));
                txtConsole.AppendText(String.Format("NumBots\t\t{0}{1}", info.NumBots, Environment.NewLine));
                txtConsole.AppendText(String.Format("Dedicated\t{0}{1}", info.Dedicated, Environment.NewLine));
                txtConsole.AppendText(String.Format("OS\t\t{0}{1}", info.OS, Environment.NewLine));
                txtConsole.AppendText(String.Format("Password\t\t{0}{1}", info.Password, Environment.NewLine));

                if (info.Type == 0x49)
                {
                    txtConsole.AppendText(String.Format("AppID\t\t{0}{1}", info.AppID, Environment.NewLine));
                    txtConsole.AppendText(String.Format("GameVersion\t{0}{1}", info.GameVersion, Environment.NewLine));
                }
                else if (info.Type == 0x6D)
                {
                    txtConsole.AppendText(String.Format("GameIP\t\t{0}{1}", info.GameIP, Environment.NewLine));
                    txtConsole.AppendText(String.Format("IsMod\t\t{0}{1}", info.IsMod, Environment.NewLine));
                    txtConsole.AppendText(String.Format("ModVersion\t{0}{1}", info.Mod.ModVersion, Environment.NewLine));
                    txtConsole.AppendText(String.Format("URLInfo\t{0}{1}", info.Mod.URLInfo, Environment.NewLine));
                    txtConsole.AppendText(String.Format("URLDL\t{0}{1}", info.Mod.URLDL, Environment.NewLine));
                }
            }
            catch (Exception exp)
            {
                txtConsole.AppendText(String.Format("Error:\t{0}{1}", exp.Message, Environment.NewLine));
            }
        }

        private void btnRules_Click(object sender, EventArgs e)
        {
            try
            {
                var rules = server.GetRules();

                if (rules == null)
                {
                    txtConsole.AppendText("NULL!" + Environment.NewLine);
                }
                else
                {
                    foreach (var rule in rules)
                    {
                        txtConsole.AppendText(String.Format("{0}\t\t: {1}{2}", rule.Key, rule.Value, Environment.NewLine));
                    }
                }
            }
            catch (Exception exp)
            {
                txtConsole.AppendText(String.Format("Error:\t{0}{1}", exp.Message, Environment.NewLine));
            }
        }

        private void btnPlayers_Click(object sender, EventArgs e)
        {
            try
            {
                var players = server.GetPlayers();

                if (players == null)
                {
                    txtConsole.AppendText("NULL!" + Environment.NewLine);
                }
                else
                {
                    foreach (var player in players)
                    {
                        txtConsole.AppendText(String.Format("{0}\t\t: {1}\t{2}{3}", player.Name, player.Kills, player.Time, Environment.NewLine));
                    }
                }
            }
            catch (Exception exp)
            {
                txtConsole.AppendText(String.Format("Error:\t{0}{1}", exp.Message, Environment.NewLine));
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                var str = txtQuery.Text.Split(' ');
                var bytes = new Byte[str.Length];

                for (int i = 0; i < str.Length; i++)
                {
                    bytes[i] = byte.Parse(str[i], System.Globalization.NumberStyles.HexNumber);
                }

                var response = server.Query(bytes);

                txtConsole.AppendText(String.Format("{0}{1}", Encoding.Default.GetString(response), Environment.NewLine));
            }
            catch (Exception exp)
            {
                txtConsole.AppendText(String.Format("Error:\t{0}{1}", exp.Message, Environment.NewLine));
            }
        }

        private void btnFakeRcon_Click(object sender, EventArgs e)
        {
            txtConsole.AppendText("In order to fake rcon change the access modifier for the DataReceived function in the GoldSrc class and uncomment the rest of this function." + Environment.NewLine);
            //foreach (var keys in datas)
            //{
            //    this.AppendLine("Sending event {0} with info: {1}", keys.Key, keys.Value);

            //    server.DataReceived(keys.Value);

            //    this.AppendLine("");
            //}
        }

        private void btnPassword_Click(object sender, EventArgs e)
        {
            this.server.RconPassword = txtPassword.Text;
        }

        private void btnChallengeRcon_Click(object sender, EventArgs e)
        {
            this.server.ChallengeRcon();
            this.txtConsole.AppendText(String.Format("Rcon Challenge: {0}{1}", this.server.RconChallenge, Environment.NewLine));
        }

        private void btnIsValid_Click(object sender, EventArgs e)
        {
            this.txtConsole.AppendText(String.Format("Password: {0}; Challenge: {1}; Valid: {2}{3}", this.server.RconPassword, this.server.RconChallenge, this.server.IsRconPasswordValid(), Environment.NewLine));
        }

        private void btnConnectLog_Click(object sender, EventArgs e)
        {
            this.server.AddLogAddress(new IPEndPoint(IPAddress.Parse(this.txtIPAddress.Text.Split(':')[0]), int.Parse(this.txtIPAddress.Text.Split(':')[1])));

            if (!this.server.IsListening)
            {
                this.server.Listen(int.Parse(this.txtIPAddress.Text.Split(':')[1]));
            }
        }

        private void btnDisconnectLog_Click(object sender, EventArgs e)
        {
            this.server.DeleteLogAddress(new IPEndPoint(IPAddress.Parse(this.txtIPAddress.Text.Split(':')[0]), int.Parse(this.txtIPAddress.Text.Split(':')[1])));

            if (this.server.IsListening)
            {
                this.server.Stop();
            }
        }

        private void btnRcon_Click(object sender, EventArgs e)
        {
            this.server.SendRcon(this.txtRcon.Text);
        }

        private void TestingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.server != null)
            {
                this.server.Dispose();
            }
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            try
            {
                var status = this.server.GetStatus();

                this.AppendLine("{0}", status.Hostname);
            }
            catch (Exception exp)
            {
                txtConsole.AppendText(String.Format("Error:\t{0}{1}", exp.Message, Environment.NewLine));
            }
        }

        private void btnCvar_Click(object sender, EventArgs e)
        {
            var cvar = this.server.GetCvar(this.txtCvar.Text);

            this.AppendLine("{0}", cvar);
        }
    }
}
