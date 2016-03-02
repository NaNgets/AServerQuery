// ValveServer.cs is part of AServerQuery.
//
// AServerQuery is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3 as
// published by the Free Software Foundation.
//
// AServerQuery is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public version 3 License for more details.
//
// You should have received a copy of the GNU General Public License
// along with AServerQuery. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace AServerQuery
{
    /// <summary>
    /// Represents a Valve game server.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
    public class ValveServer : IDisposable
    {
        #region Inner Classes

        /// <summary>
        /// Contains Valve server queries and useful query constants.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
        public static class Queries
        {
            /// <summary>
            /// The GoldSrc engine split packets' header's length in bytes.
            /// </summary>
            public const int GoldSrcSplitPacketsHeaderLength    = 9;

            /// <summary>
            /// The Source engine split packets' header's length in bytes.
            /// </summary>
            public const int SourceSplitPacketsHeaderLength     = 10;

            /// <summary>
            /// The OrangeBox engine split packets' header's length in bytes.
            /// </summary>
            public const int OrangeBoxSplitPacketsHeaderLength  = 12;

            /// <summary>
            /// A packet header to indicate that only one packet was sent or received, according to the HL protocol.
            /// </summary>
            /// <remarks>After a one packet header comes the data. Split packets have a longer header.</remarks>
            /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries#Protocol" />
            public static readonly Byte[] OnePacketHeader = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

            /// <summary>
            /// A packet header to indicate that more than one packet was sent or received, according to the HL protocol.
            /// </summary>
            /// <remarks>
            /// <para>Split packets have a longer header with more information in it.</para>
            /// <para>For more information see the protocol section of the server queries information page.</para>
            /// </remarks>
            /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries#Protocol" />
            public static readonly Byte[] SplitPacketsHeader = new Byte[] { 0xFE, 0xFF, 0xFF, 0xFF };

            /// <summary>
            /// Ping the server to see if it exists, this can be used to calculate the latency to the server.
            /// </summary>
            public static readonly Byte[] A2A_PING =
                Util.ConcatByteArrays(ValveServer.Queries.OnePacketHeader, new Byte[] { 0x69 });

            /// <summary>
            /// Challenge values are required for A2S_PLAYER and A2S_RULES requests, you can use this request to get one.
            /// </summary>
            public static readonly Byte[] A2S_SERVERQUERY_GETCHALLENGE =
                Util.ConcatByteArrays(ValveServer.Queries.OnePacketHeader, new Byte[] { 0x55 }, BitConverter.GetBytes(ValveServer.EmptyChallenge));

            /// <summary>
            /// Server info can be requested by sending the following byte values in a UDP packet to the server.
            /// </summary>
            public static readonly Byte[] A2S_INFO =
                Util.ConcatByteArrays(ValveServer.Queries.OnePacketHeader, new Byte[] { 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 });

            /// <summary>
            /// The challenge number can either be set to -1 (0xFF FF FF FF) to have the server reply with S2C_CHALLENGE, or use the value from a previous A2S_SERVERQUERY_GETCHALLENGE request.
            /// </summary>
            public static readonly Byte[] A2S_PLAYER_NO_CHALLENGE =
                Util.ConcatByteArrays(ValveServer.Queries.OnePacketHeader, new Byte[] { 0x55 });

            /// <summary>
            /// The challenge number can either be set to -1 (0xFF FF FF FF) to have the server reply with S2C_CHALLENGE, or use the value from a previous A2S_SERVERQUERY_GETCHALLENGE request.
            /// </summary>
            public static readonly Byte[] A2S_RULES_NO_CHALLENGE =
                Util.ConcatByteArrays(ValveServer.Queries.OnePacketHeader, new Byte[] { 0x56 });
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The receive time-out in milliseconds for queries. 0 or -1 is infinite time-out period.
        /// </summary>
        protected   int                     timeOut         = DefaultTimeOut;

        #region Const Members

        /// <summary>
        /// The default timeout for query requests.
        /// </summary>
        public const int        DefaultTimeOut              = 5000;

        /// <summary>
        /// The date and time format sent from the game server.
        /// </summary>
        public const String     DateTimeFormat              = "MM/dd/yyyy - HH:mm:ss";

        /// <summary>
        /// Represents an empty challenge.
        /// </summary>
        public const long       EmptyChallenge              = -1;

        /// <summary>
        /// The log line's regular expression's match command group index.
        /// </summary>
        public const int        LogLineCommandIndex         = 2;

        /// <summary>
        /// The log line's regular expression's match date and time group index.
        /// </summary>
        public const int        LogLineDateTimeIndex        = 1;

        /// <summary>
        /// A regular expression pattern to match a log line.
        /// </summary>
        /// <remarks>
        /// <para>The command index for the match should be referenced in the <see cref="AServerQuery.ValveServer.LogLineCommandIndex" />.</para>
        /// <para>The date and time index for the match should be referenced in the <see cref="AServerQuery.ValveServer.LogLineDateTimeIndex" />.</para>
        /// </remarks>
        public const String     LogLineRegex
            = "L ((?:0[1-9]|1[0-2])/(?:0[1-9]|[12][0-9]|3[01])/[0-9]{4} - (?:[01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]): (.*)[\\n\\r\\0]*";

        #endregion

        #region Events

        /// <summary>
        /// An event to handle all received data from the server before it is being parsed and processed.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs>    DataReceived;

        /// <summary>
        /// An event to handle exceptions being throwen inside the listening thread.
        /// </summary>
        public event EventHandler<ExceptionEventArgs>       Exception;

        #region HL Engine

        /// <summary>
        /// Event 001a. Occurs when the server cvars start.
        /// <para>In TFC, if tfc_clanbattle is 1, this doesn't happen.</para>
        /// </summary>
        public event EventHandler<InfoEventArgs>            CvarStart;          // 001a

        /// <summary>
        /// Event 001b. Occurs when a cvar value is changed.
        /// <para>This is executed both at the beginning of a round and whenever someone changes a cvar over rcon.</para>
        /// </summary>
        public event EventHandler<CvarEventArgs>            Cvar;               // 001b

        /// <summary>
        /// Event 001c. Occurs when the server cvars end.
        /// <para>In TFC, if tfc_clanbattle is 0, this doesn't happen. You can instead use 005, since it comes right after.</para>
        /// </summary>
        public event EventHandler<InfoEventArgs>            CvarEnd;            // 001c

        /// <summary>
        /// Event 002a. Occurs when a log file starts.
        /// </summary>
        public event EventHandler<InfoEventArgs>            LogFileStart;       // 002a

        /// <summary>
        /// Event 002b. Occurs when a log file closes.
        /// </summary>
        public event EventHandler<InfoEventArgs>            LogFileClosed;      // 002b

        /// <summary>
        /// Event 003a. Occurs when a map loads.
        /// </summary>
        public event EventHandler<ServerEventArgs>          MapLoading;         // 003a

        /// <summary>
        /// Event 003b. Occurs when a map starts.
        /// </summary>
        public event EventHandler<ServerEventArgs>          MapStarting;        // 003b

        /// <summary>
        /// Event 004a. Occurs when a rcon command is sent.
        /// </summary>
        public event EventHandler<RconEventArgs>            Rcon;               // 004a

        /// <summary>
        /// Event 004b. Occurs when a bad rcon command is sent.
        /// </summary>
        public event EventHandler<RconEventArgs>            BadRcon;            // 004b

        /// <summary>
        /// Event 005. Occurs when the server displays its name.
        /// </summary>
        public event EventHandler<ServerEventArgs>          ServerName;         // 005

        /// <summary>
        /// Event 006. Occurs when the server says a message.
        /// </summary>
        public event EventHandler<ServerEventArgs>          ServerSay;          // 006

        #endregion

        #region Game

        /// <summary>
        /// Event 050. Occurs when a player connects to the server.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    Connection;         // 050

        /// <summary>
        /// Event 050b. Occurs when a player is validated.
        /// </summary>
        public event EventHandler<PlayerEventArgs>          Validation;         // 050b

        /// <summary>
        /// Event 051. Occurs when a player enters the game.
        /// </summary>
        public event EventHandler<PlayerEventArgs>          EnterGame;          // 051

        /// <summary>
        /// Event 052. Occurs when a player disconnects.
        /// </summary>
        public event EventHandler<PlayerEventArgs>          Disconnection;      // 052

        /// <summary>
        /// Event 052b. Occurs when a player is kicked from the server.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    Kick;               // 052b

        /// <summary>
        /// Event 053. Occurs when a player commits suicide.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    Suicide;            // 053

        /// <summary>
        /// Event 054. Occurs when a player selects a team.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    TeamSelection;      // 054

        /// <summary>
        /// Event 055. Occurs when a player selects a role.
        /// <para>This event covers classes in games like TFC, FLF and DOD.</para>
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    RoleSelection;      // 055

        /// <summary>
        /// Event 056. Occurs when a player changes his name.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    ChangeName;         // 056

        /// <summary>
        /// Event 057. Occurs when a player kills another player.
        /// </summary>
        public event EventHandler<PlayerOnPlayerEventArgs>  Kill;               // 057

        /// <summary>
        /// Event 058. Occurs when a player attacks another player.
        /// <para>This event allows for recording of partial kills and teammate friendly-fire injuries.
        /// If the injury results in a kill, a Kill message (057) should be logged instead/also.</para>
        /// </summary>
        public event EventHandler<PlayerOnPlayerEventArgs>  Attack;             // 058

        /// <summary>
        /// Event 059. Occurs when a player triggers an action on another player.
        /// <para>This event allows for logging of a wide range of events where one player performs an action on another player.
        /// For example, in TFC this event may cover medic healings and infections, sentry gun destruction, spy uncovering, etc.</para>
        /// <para>More detail about the action can be given by appending more properties to the end of the event.</para>
        /// </summary>
        public event EventHandler<PlayerOnPlayerEventArgs>  PlayerOnPlayer;     // 059

        /// <summary>
        /// Event 060. Occurs when a player commits an action.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    PlayerAction;       // 060

        /// <summary>
        /// Event 061. Occurs when a team commits an action.
        /// </summary>
        public event EventHandler<TeamEventArgs>            TeamAction;         // 061

        /// <summary>
        /// Event 062. Occurs when the wolrd commits an action.
        /// <para>This event allows logging of anything which does not happen in response to the actions of a player or team.
        /// For example a gate opening at the start of a round.</para>
        /// </summary>
        public event EventHandler<ServerEventArgs>          WorldAction;        // 062

        /// <summary>
        /// Event 063a. Occurs when a player sends a message to the server.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    Chat;               // 063a

        /// <summary>
        /// Event 063b. Occurs when a player sends a message to his team.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    TeamChat;           // 063b

        /// <summary>
        /// Event 064. Occurs when a team forms an alliance.
        /// </summary>
        public event EventHandler<TeamEventArgs>            TeamAlliance;       // 064

        /// <summary>
        /// Event 065. Occurs when a team's score is displayed.
        /// </summary>
        public event EventHandler<TeamScoreEventArgs>       TeamScore;          // 065

        /// <summary>
        /// Event 066. Occurs when a player private messages another player.
        /// </summary>
        public event EventHandler<PlayerOnPlayerEventArgs>  PrivateChat;        // 066

        /// <summary>
        /// Event 067. Occurs when a player's score is displayed.
        /// </summary>
        public event EventHandler<PlayerScoreEventArgs>     PlayerScore;        // 067

        /// <summary>
        /// Event 068. Occurs when a player selects a weapon.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    WeaponSelection;    // 068

        /// <summary>
        /// Event 069. Occurs when a player picks up a weapon.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs>    WeaponPickup;       // 069

        #endregion

        #region Event triggers

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.DataReceived" /> event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnDataReceived(DataReceivedEventArgs args)
        {
            // Get the event's targets.
            var targets = this.DataReceived;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Exception" /> event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnException(ExceptionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Exception;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        #region HL Engine event triggers

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.CvarStart" /> (001a) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnCvarStart(InfoEventArgs args)
        {
            // Get the event's targets.
            var targets = this.CvarStart;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Cvar" /> (001b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnCvar(CvarEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Cvar;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.CvarEnd" /> (001c) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnCvarEnd(InfoEventArgs args)
        {
            // Get the event's targets.
            var targets = this.CvarEnd;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.LogFileStart" /> (002a) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnLogFileStart(InfoEventArgs args)
        {
            // Get the event's targets.
            var targets = this.LogFileStart;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.LogFileClosed" /> (002b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnLogFileClosed(InfoEventArgs args)
        {
            // Get the event's targets.
            var targets = this.LogFileClosed;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.MapLoading" /> (003a) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnMapLoading(ServerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.MapLoading;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.MapStarting" /> (003b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnMapStarting(ServerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.MapStarting;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Rcon" /> (004a) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnRcon(RconEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Rcon;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.BadRcon" /> (004b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnBadRcon(RconEventArgs args)
        {
            // Get the event's targets.
            var targets = this.BadRcon;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.ServerName" /> (005) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnServerName(ServerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.ServerName;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.ServerSay" /> (006) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnServerSay(ServerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.ServerSay;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        #endregion

        #region Game event triggers

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Connection" /> (050) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnConnection(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Connection;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Validation" /> (050b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnValidation(PlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Validation;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.EnterGame" /> (051) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnEnterGame(PlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.EnterGame;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Disconnection" /> (052) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnDisconnection(PlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Disconnection;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Kick" /> (052b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnKick(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Kick;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Suicide" /> (053) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnSuicide(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Suicide;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.TeamSelection" /> (054) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnTeamSelection(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.TeamSelection;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.RoleSelection" /> (055) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnRoleSelection(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.RoleSelection;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.ChangeName" /> (056) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnChangeName(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.ChangeName;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Kill" /> (057) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnKill(PlayerOnPlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Kill;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Attack" /> (058) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnAttack(PlayerOnPlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Attack;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.PlayerOnPlayer" /> (059) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnPlayerOnPlayer(PlayerOnPlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.PlayerOnPlayer;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.PlayerAction" /> (060) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnPlayerAction(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.PlayerAction;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.TeamAction" /> (061) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnTeamAction(TeamEventArgs args)
        {
            // Get the event's targets.
            var targets = this.TeamAction;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.WorldAction" /> (062) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnWorldAction(ServerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.WorldAction;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.Chat" /> (063a) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnChat(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Chat;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.TeamChat" /> (063b) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnTeamChat(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.TeamChat;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.TeamAlliance" /> (064) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnTeamAlliance(TeamEventArgs args)
        {
            // Get the event's targets.
            var targets = this.TeamAlliance;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.TeamScore" /> (065) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnTeamScore(TeamScoreEventArgs args)
        {
            // Get the event's targets.
            var targets = this.TeamScore;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.PrivateChat" /> (066) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnPrivateChat(PlayerOnPlayerEventArgs args)
        {
            // Get the event's targets.
            var targets = this.PrivateChat;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.PlayerScore" /> (067) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnPlayerScore(PlayerScoreEventArgs args)
        {
            // Get the event's targets.
            var targets = this.PlayerScore;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.WeaponSelection" /> (068) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnWeaponSelection(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.WeaponSelection;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        /// <summary>
        /// Calls the <see cref="AServerQuery.ValveServer.WeaponPickup" /> (069) event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnWeaponPickup(PlayerActionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.WeaponPickup;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this instance is disposed or not.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return (this.Server == null);
            }
        }

        /// <summary>
        /// Gets whether this instance is listening for incoming data or not.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return ((this.LogClient != null) && (this.LogClient.Client.Connected));
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.UdpClient" /> used to listen to logs from the game server.
        /// </summary>
        /// <value>The log listener for the server.</value>
        public UdpClient LogClient
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the lock which makes sure the log listening will be safe.
        /// </summary>
        private ReaderWriterLockSlim LogClientLocker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the server's Rcon password.
        /// </summary>
        /// <value>The server's Rcon password.</value>
        public String RconPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.IPEndPoint" /> of the remote server.
        /// </summary>
        /// <value>The remote server's address.</value>
        public IPEndPoint Server
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the receive time-out in milliseconds for queries.
        /// </summary>
        /// <value>The receive time-out in milliseconds for queries. 0 or -1 is infinite time-out period.</value>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="value" /> is less than -1.</exception>
        public int TimeOut
        {
            get
            {
                return (this.timeOut);
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.timeOut = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// A default private constructor to set the default values.
        /// </summary>
        private ValveServer()
        {
            this.LogClientLocker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Constructs the <see cref="AServerQuery.ValveServer" /> instance with the given parameters.
        /// </summary>
        /// <param name="Server">The <see cref="System.Net.IPEndPoint" /> of the game server.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="Server" /> is <see langword="null" />.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">
        /// An error occurred when accessing the socket. See the Remarks section for more information.
        /// </exception>
        public ValveServer(IPEndPoint Server)
            : this(Server, String.Empty)
        {
        }

        /// <summary>
        /// Constructs the <see cref="AServerQuery.ValveServer" /> instance with the given parameters.
        /// </summary>
        /// <param name="Server">The <see cref="System.Net.IPEndPoint" /> of the game server.</param>
        /// <param name="RconPassword">The Rcon password for the game server.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="Server" /> is <see langword="null" />.</exception>
        public ValveServer(IPEndPoint Server, String RconPassword)
            : this()
        {
            // If the given server is null, throw an exception.
            if (Server == null)
            {
                throw new ArgumentNullException("Server");
            }

            // Set the server IPEndPoint to the given value.
            this.Server         = Server;

            // Set the password to the given value.
            this.RconPassword   = RconPassword;
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~ValveServer()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        #region IDisposable Members

        /// <summary>
        /// Closes the connection to the server and stops listening.
        /// </summary>
        /// <remarks>
        /// This method does NOT remove the server from any registered logging servers.
        /// </remarks>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes the connection to the server and stops listening.
        /// </summary>
        /// <remarks>
        /// This method does NOT remove the server from any registered logging servers.
        /// </remarks>
        /// <param name="disposing"><see langword="true" /> if the data members should be disposed, <see langword="false" /> otherwise.</param>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                this.Stop();

                if (disposing)
                {
                    this.LogClientLocker.Dispose();
                    this.LogClientLocker    = null;
                    this.Server             = null;
                    this.RconPassword       = String.Empty;
                }
            }
            catch { }
        }

        /// <summary>
        /// If the instance has been disposed, throws an appropriate exception.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> instance is disposed.</exception>
        protected virtual void CheckDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        #endregion

        #region Sockets methods

        /// <summary>
        /// Receives a splitted packet from the client. By default, this uses the OrangeBox engine's multi-packet response format (unless overridden).
        /// </summary>
        /// <param name="data">The packet containing the header.</param>
        /// <param name="client">The client to read the splitted packets from.</param>
        /// <returns>All of the data in the packets combined to one packet.</returns>
        /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
        protected virtual Byte[] ReceiveSplitPacket(Byte[] data, UdpClient client)
        {
                // Gets the total number of packets.
                int numPackets      = data[8];

                // Create a byte-array array to hold all the responses.
                var allResponses    = new Byte[numPackets][];

                // Decrease the number of received packets (first packet was already received).
                numPackets--;

                // Receive all packets and set them in the correct order.
                do
                {
                    // Get the current packet length without its header.
                    int currPacketLength    = data.Length - ValveServer.Queries.OrangeBoxSplitPacketsHeaderLength;

                    // Gets the current packet number.
                    int currPacketNum       = data[9];

                    // Sets the already received response in its correct place in the array.
                    allResponses[currPacketNum] = new Byte[currPacketLength];
                    Array.Copy(data, ValveServer.Queries.OrangeBoxSplitPacketsHeaderLength, allResponses[currPacketNum], 0, currPacketLength);

                    // Receives the next packets if there should be one.
                    if (numPackets > 0)
                    {
                        // Create an endpoint to store the server's address.
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                        data    = client.Receive(ref endPoint);
                    }
                } while (numPackets-- > 0);

                // Create the final response by concating all the received data with the Type set first as the header (0xFFFFFFFE).
                return (Util.ConcatByteArrays(allResponses));
        }

        /// <summary>
        /// Sends the given <paramref name="value" /> to the server
        /// and returns the response combined to one <see cref="System.Byte" /> array.
        /// </summary>
        /// <remarks>
        /// This method receives packets from the server according to the standard and arranges them in the correct order,
        /// whilst if there were more than one packet received, combines them to one and alters the 4-byte header to match
        /// the one packet response header (-1 or 0xFF, 0xFF, 0xFF, 0xFF).
        /// </remarks>
        /// <param name="value">The value to send to the game server.</param>
        /// <returns>The response sent back from the game server combined to one <see cref="System.Byte" /> array.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        /// <exception cref="AServerQuery.UnknownHeaderException">Unknown header was received from the server.</exception>
        /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
        public Byte[] Query(Byte[] value)
        {
            return (this.Query(value, this.TimeOut));
        }

        /// <summary>
        /// Sends the given <paramref name="value" /> to the server using the given receive time-out value
        /// and returns the response combined to one <see cref="System.Byte" /> array.
        /// </summary>
        /// <remarks>
        /// This method receives packets from the server according to the standard and arranges them in the correct order,
        /// whilst if there were more than one packet received, combines them to one and alters the 4-byte header to match
        /// the one packet response header (-1 or 0xFF, 0xFF, 0xFF, 0xFF).
        /// </remarks>
        /// <param name="value">The value to send to the game server.</param>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>The response sent back from the game server combined to one <see cref="System.Byte" /> array.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.UnknownHeaderException">Unknown header was received from the server.</exception>
        /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
        public Byte[] Query(Byte[] value, int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            UdpClient client = null;

            try
            {
                client = new UdpClient();

                // Set the given timeout.
                client.Client.ReceiveTimeout = timeOut;

                // Create an endpoint to store the server's address.
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                // Connect to the game server so unwanted data won't get in the way.
                client.Connect(this.Server);

                // Send the query.
                client.Send(value, value.Length);

                // Receive the first response.
                var response        = client.Receive(ref endPoint);

                // If there's only one packet according to the standard.
                if (BitConverter.ToInt32(response, 0) == BitConverter.ToInt32(ValveServer.Queries.OnePacketHeader, 0))
                {
                    return (response);
                }
                // Else, if there's more than one packet according to the standard.
                else if (BitConverter.ToInt32(response, 0) == BitConverter.ToInt32(ValveServer.Queries.SplitPacketsHeader, 0))
                {
                    // Receive the split packets.
                    response    = this.ReceiveSplitPacket(response, client);

                    return (response);
                }
                // Else, if there's an unknown header, throw an exception.
                else
                {
                    throw new UnknownHeaderException(Util.ConvertToString(response));
                }
            }
            // If there is any socket exception, encapsulate it as an IOException and throw it.
            catch (SocketException exp)
            {
                throw new IOException("Error while trying to query the server.", exp);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }

                client = null;
            }
        }

        /// <summary>
        /// Starts listening to log data from the servers.
        /// </summary>
        /// <remarks>Responses are sent to the <see cref="AServerQuery.ValveServer.OnDataReceived" /> event or matched events.</remarks>
        /// <param name="port">The local port to listen to the Rcon log with.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The port parameter is greater than <see cref="System.Net.IPEndPoint.MaxPort" /> or less
        /// than <see cref="System.Net.IPEndPoint.MinPort" />.
        /// </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.AlreadyListeningException">
        /// The <see cref="AServerQuery.ValveServer" /> instance is already listening.
        /// </exception>
        public void Listen(int port)
        {
            this.Listen(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        /// Starts listening to log data from the servers.
        /// </summary>
        /// <remarks>Responses are sent to the <see cref="AServerQuery.ValveServer.OnDataReceived" /> event or matched events.</remarks>
        /// <param name="localEP">The local end-point to listen to the Rcon log with.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.AlreadyListeningException">
        /// The <see cref="AServerQuery.ValveServer" /> instance is already listening.
        /// </exception>
        public void Listen(IPEndPoint localEP)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // If the instance is already listening, throw the appropriate exception.
            if (this.IsListening)
            {
                throw new AlreadyListeningException();
            }

            try
            {
                this.LogClientLocker.EnterWriteLock();

                // Create a new instance of the UdpClient to listen to the log.
                this.LogClient = new UdpClient(localEP);

                // Connect to the wanted server.
                this.LogClient.Connect(this.Server);

                // Start listening to the server.
                this.LogClient.BeginReceive(this.WaitForData, null);
            }
            // If there is any socket exception, encapsulate it as an IOException and throw it.
            catch (SocketException exp)
            {
                if (this.LogClient != null)
                {
                    this.LogClient.Close();
                }

                this.LogClient = null;

                throw new IOException("Exception while listening to the server.", exp);
            }
            catch
            {
                if (this.LogClient != null)
                {
                    this.LogClient.Close();
                }

                this.LogClient = null;

                throw;
            }
            finally
            {
                this.LogClientLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Stops listening to data received from the server.
        /// </summary>
        /// <remarks>
        /// This method only stops listening to the log, it does NOT remove the server from any registered logging servers.
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void Stop()
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            this.LogClientLocker.EnterWriteLock();

            // If the instance is listening, stops listening.
            if (this.IsListening)
            {
                try
                {
                    this.LogClient.Client.Shutdown(SocketShutdown.Both);

                    this.LogClient.Close();
                    this.LogClient = null;
                }
                // If there is any socket exception, encapsulate it as an IOException and throw it.
                catch (SocketException exp)
                {
                    throw new IOException("Error while trying to stop listening to the server.", exp);
                }
            }

            this.LogClientLocker.ExitWriteLock();
        }

        /// <summary>
        /// Waits for data from the server.
        /// </summary>
        private void WaitForData(IAsyncResult ar)
        {
            // Create an IPEndPoint to record the sender's address.
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                Byte[] data = null;

                this.LogClientLocker.EnterReadLock();

                if (this.LogClient != null)
                {
                    // Receive a response from the server.
                    data = this.LogClient.EndReceive(ar, ref endPoint);
                }

                this.LogClientLocker.ExitReadLock();

                // Only if we've received something, process the given data.
                if ((data != null) && (data.Length > 0))
                {
                    // Process the received response.
                    this.ProcessLog(Util.ConvertToString(data));
                }
            }
            // If any exception was throwen, throw it on the OnException event.
            catch (Exception exp)
            {
                this.OnException(new ExceptionEventArgs(exp));
            }

            this.LogClientLocker.EnterReadLock();

            // Only if the client still exists, listen again.
            if (this.LogClient != null)
            {
                this.LogClient.BeginReceive(this.WaitForData, null);
            }

            this.LogClientLocker.ExitReadLock();
        }

        #endregion

        #region Query methods

        /// <summary>
        /// Pings the given game server.
        /// </summary>
        /// <param name="server">The game server to ping.</param>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns><see langword="true" /> if server has responded, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        public static bool Ping(IPEndPoint server, int timeOut)
        {
            using (var game = new ValveServer(server))
            {
                return (game.Ping(timeOut));
            }
        }

        /// <summary>
        /// Pings the given game server.
        /// </summary>
        /// <param name="server">The game server to ping.</param>
        /// <remarks>The time-out used is <see cref="AServerQuery.ValveServer.DefaultTimeOut" />.</remarks>
        /// <returns><see langword="true" /> if server has responded, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public static bool Ping(IPEndPoint server)
        {
            return (ValveServer.Ping(server, ValveServer.DefaultTimeOut));
        }

        /// <summary>
        /// Pings the game server.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns><see langword="true" /> if server has responded, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public bool Ping()
        {
            return (this.Ping(this.TimeOut));
        }

        /// <summary>
        /// Pings the game server.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns><see langword="true" /> if server has responded, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        public bool Ping(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // If server is responding to ping, return true.
            try
            {
                var response = this.Query(ValveServer.Queries.A2A_PING, timeOut);

                return ((response.Length >= 6) && (response[4] == 0x6A));
            }
            catch (IOException exp)
            {
                // Only if the catched exception has an inner exception which is a socket exception.
                if ((exp.InnerException != null) && (exp.InnerException is SocketException))
                {
                    // If the socket exception is of timeout - return false.
                    if (((SocketException)exp.InnerException).SocketErrorCode == SocketError.TimedOut)
                    {
                        return (false);
                    }

                    // Otherwise, encapsulate it as a ping IOException and throw it.
                    throw new IOException("Error while trying to ping the server.", exp);
                }

                throw;
            }
        }

        /// <summary>
        /// Queries the game server for a query challenge.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns>A challenge to be used in queries.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public long GetQueryChallenge()
        {
            return (this.GetQueryChallenge(this.TimeOut));
        }

        /// <summary>
        /// Queries the game server for a query challenge.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>A challenge to be used in queries.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        public long GetQueryChallenge(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            var response = this.Query(ValveServer.Queries.A2S_SERVERQUERY_GETCHALLENGE, timeOut);

            // If the header is correct, get the challenge.
            if ((response.Length == 9) && (response[4] == 0x41))
            {
                return (BitConverter.ToInt32(response, 5));
            }

            // Otherwise, return the no challenge value.
            return (ValveServer.EmptyChallenge);
        }

        /// <summary>
        /// Queries the server for info.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns>A <see cref="AServerQuery.ServerInfo" /> with the game server's info.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public ServerInfo GetInfo()
        {
            return (this.GetInfo(this.TimeOut));
        }

        /// <summary>
        /// Queries the server for info.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>A <see cref="AServerQuery.ServerInfo" /> with the game server's info.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        public ServerInfo GetInfo(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            var response = this.Query(ValveServer.Queries.A2S_INFO, timeOut);

            return (ServerInfo.Parse(response));
        }

        /// <summary>
        /// Queries the server for the players.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns>
        /// A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="AServerQuery.PlayerInfo" /> representing the
        /// players in the server.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadQueryChallengeException">Server has returned an empty query challenge.</exception>
        /// <exception cref="AServerQuery.UnknownHeaderException">Response was not the expected type.</exception>
        public IEnumerable<PlayerInfo> GetPlayers()
        {
            return (this.GetPlayers(this.TimeOut));
        }

        /// <summary>
        /// Queries the server for the players.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="AServerQuery.PlayerInfo" /> representing the
        /// players in the server.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadQueryChallengeException">Server has returned an empty query challenge.</exception>
        /// <exception cref="AServerQuery.UnknownHeaderException">Response was not the expected type.</exception>
        public IEnumerable<PlayerInfo> GetPlayers(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // Get the challenge to request the players with.
            var challenge = this.GetQueryChallenge();

            // If the challenge from the server is empty, throw a bad challenge exception.
            if (challenge == ValveServer.EmptyChallenge)
            {
                throw new BadQueryChallengeException(challenge.ToString());
            }

            // Query the server for the players with the challenge.
            var response        = this.Query(Util.ConcatByteArrays(ValveServer.Queries.A2S_PLAYER_NO_CHALLENGE, BitConverter.GetBytes(challenge)), timeOut);

            // Initialize the offset.
            int responseOffset  = 4;

            // If the given response is not the wanted one, throw the appropriate exception.
            if (response[responseOffset++] != 0x44)
            {
                throw new UnknownHeaderException(Util.ConvertToString(response));
            }

            // Get the number of players being read.
            int numPlayers      = response[responseOffset++];

            // If the number of players received by the server is 0 - yield break.
            if (numPlayers == 0)
            {
                yield break;
            }

            // Go over each given player and yield return it.
            while (numPlayers-- > 0)
            {
                yield return (PlayerInfo.Parse(response, ref responseOffset));
            }
        }

        /// <summary>
        /// Queries the server for rules.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="KeyValuePair{T, T}" /> (String, String) of the game server's rules.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadQueryChallengeException">Server has returned an empty query challenge.</exception>
        /// <exception cref="AServerQuery.UnknownHeaderException">Response was not the expected type.</exception>
        public IEnumerable<KeyValuePair<String, String>> GetRules()
        {
            return (this.GetRules(this.TimeOut));
        }

        /// <summary>
        /// Queries the server for rules.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="KeyValuePair{T, T}" /> (String, String) of the game server's rules.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.ValveServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadQueryChallengeException">Server has returned an empty query challenge.</exception>
        /// <exception cref="AServerQuery.UnknownHeaderException">Response was not the expected type.</exception>
        public IEnumerable<KeyValuePair<String, String>> GetRules(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // Get the challenge to request the rules with.
            var challenge       = this.GetQueryChallenge();

            // If the challenge from the server is empty, throw a bad challenge exception.
            if (challenge == ValveServer.EmptyChallenge)
            {
                throw new BadQueryChallengeException(challenge.ToString());
            }

            // Query the server for the rules with the challenge.
            var response        = this.Query(Util.ConcatByteArrays(ValveServer.Queries.A2S_RULES_NO_CHALLENGE, BitConverter.GetBytes(challenge)), timeOut);

            // Initialize the offset.
            int responseOffset  = 4;

            // If the given response is not the wanted one, throw the appropriate exception.
            if (response[responseOffset++] != 0x45)
            {
                throw new UnknownHeaderException(Util.ConvertToString(response));
            }

            // Read the number of rules being read.
            int nCount          = BitConverter.ToInt16(response, responseOffset);
            responseOffset      += 2;

            // Go over each given rule and yield return it.
            while (nCount-- > 0)
            {
                // Yield return the key and value.
                yield return (new KeyValuePair<String, String>(
                                Util.ReadString(response, ref responseOffset),      // Key
                                Util.ReadString(response, ref responseOffset)));    // Value
            }
        }

        #endregion

        #region Log Methods

        /// <summary>
        /// Parse the given properties according to the log standard.
        /// </summary>
        /// <param name="value">The properties string to parse.</param>
        /// <returns>
        /// A case insensative <see cref="System.Collections.Generic.Dictionary{String, String}" />
        /// containing the properties and their values.
        /// </returns>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#Notes" />
        public static Dictionary<String, String> ParseProperties(string value)
        {
            // Separate all the properties.
            var matches = Regex.Matches(value, "\\((\\S+)(?: \"([^\"]*)\")?\\)");

            return (matches.Cast<Match>().ToDictionary(
                        k => k.Groups[1].Value,
                        v => v.Groups[2].Success ? v.Groups[2].Value : bool.TrueString,
                        StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes the given log data.
        /// </summary>
        /// <remarks>
        /// Unknown events are thrown to the <see cref="AServerQuery.ValveServer.Exception" /> event
        /// with an <see cref="AServerQuery.UnknownEventException" /> exception.
        /// </remarks>
        /// <param name="value">A log line to process.</param>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
        internal virtual void ProcessLog(String value)
        {
            // Avoid any chance that the data received is empty.
            if (String.IsNullOrWhiteSpace(value)) return;

            this.OnDataReceived(new DataReceivedEventArgs(value));

            var logMatch    = Regex.Match(value, ValveServer.LogLineRegex);

            // If the line received is not a log line, disregard it.
            if (!logMatch.Success)
            {
                return;
            }

            // Save the original received line to use on the events.
            var original    = value;

            // Extract the date and time the server reports this event occured.
            // Uncomment if needed.
            // var time        = DateTime.ParseExact(logMatch.Groups[1].Value, ValveServer.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);

            // Get the log itself.
            value           = logMatch.Groups[ValveServer.LogLineCommandIndex].Value;

            // If value is a comment, disregard it.
            if (value.StartsWith("//"))
                return;

            // Matches event 001b.
            else if (value.StartsWith("Server cvar "))
            {
                var mchEvent    = Regex.Match(value, "^Server cvar \"([^\"]+)\" = \"([^\"]+)\"(.*)$");

                var cvarName    = mchEvent.Groups[1].Value;
                var cvarValue   = mchEvent.Groups[2].Value;
                var props       = ParseProperties(mchEvent.Groups[3].Value);

                this.OnCvar(new CvarEventArgs(original, HLEvent.Cvar, "001b", cvarName, cvarValue, props));
            }
            // Matches events 004a, 004b.
            else if (value.StartsWith("Bad Rcon: ") || value.StartsWith("Rcon: "))
            {
                var mchEvent    = Regex.Match(value, "^(Bad )?Rcon: \"[^\"]+ ([^\"]+) \"([^\"]+)\" (.*)\" [^\"\\(]+ \"([^\"]+):([^\"]+)\"(.*)$");

                // Parse the challenge safely.
                long challenge;
                long.TryParse(mchEvent.Groups[2].Value, out challenge);

                var rconPassword = mchEvent.Groups[3].Value;
                var command     = mchEvent.Groups[4].Value;

                // Safely get the sender's address and port.
                IPEndPoint sender   = null;
                IPAddress ipAddress = null;

                // Only if both the IP address and the port has been successfuly parsed, store the sender.
                if (IPAddress.TryParse(mchEvent.Groups[5].Value, out ipAddress))
                {
                    int port;

                    if (int.TryParse(mchEvent.Groups[6].Value, out port))
                    {
                        sender  = new IPEndPoint(ipAddress, port);
                    }
                }

                var props       = ParseProperties(mchEvent.Groups[7].Value);

                // Bad Rcon
                if (mchEvent.Groups[1].Value == "Bad")
                {
                    this.OnBadRcon(new RconEventArgs(original, HLEvent.Rcon, "004b", false, challenge, rconPassword, command, sender, props));
                }
                else
                {
                    this.OnRcon(new RconEventArgs(original, HLEvent.Rcon, "004a", true, challenge, rconPassword, command, sender, props));
                }
            }
            // Matches event 052b.
            else if (value.StartsWith("Kick: "))
            {
                var mchEvent = Regex.Match(value, "^Kick: (\"[^\"]+\") ([^\"]+) \"([^\"]+)\"(.*)$");

                // Safely get the player.
                Player player;
                Player.TryParse(mchEvent.Groups[1].Value, out player);

                // var noun        = mchEvent.Groups[2].Value;
                var kicker      = mchEvent.Groups[3].Value;
                var props       = ParseProperties(mchEvent.Groups[4].Value);

                this.OnKick(new PlayerActionEventArgs(original, HLEvent.Kick, "052b", player, kicker, props));
            }
            // Matches event 065.
            else if (Regex.IsMatch(value, "^Team \"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\"(.*)$"))
            {
                var mchEvent    = Regex.Match(value, "^Team \"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\"(.*)$");

                var team        = mchEvent.Groups[1].Value;

                // Parse the values safely.
                int score, numPlayers;
                int.TryParse(mchEvent.Groups[3].Value, out score);
                int.TryParse(mchEvent.Groups[5].Value, out numPlayers);

                var props       = ParseProperties(mchEvent.Groups[7].Value);

                this.OnTeamScore(new TeamScoreEventArgs(original, HLEvent.TeamScore, "065", team, score, numPlayers, props));
            }
            // Matches event 067.
            else if (Regex.IsMatch(value, "^Player (\"[^\"]+\") ([^\"\\(]+) \"([^\"]+)\"(.*)$"))
            {
                var mchEvent    = Regex.Match(value, "^Player (\"[^\"]+\") ([^\"\\(]+) \"([^\"]+)\"(.*)$");

                // Safely get the player.
                Player player;
                Player.TryParse(mchEvent.Groups[1].Value, out player);

                // Parse the score safely.
                int score;
                int.TryParse(mchEvent.Groups[3].Value, out score);

                var props       = ParseProperties(mchEvent.Groups[4].Value);

                this.OnPlayerScore(new PlayerScoreEventArgs(original, HLEvent.PlayerScore, "067", player, score, props));
            }
            // Matches events 057, 058, 059, 066.
            else if (Regex.IsMatch(value, "^(\"[^\"]+\") ([^\"\\(]+) (\"[^\"]+\") ([^\"\\(]+) \"([^\"]+)\"(.*)$"))
            {
                var mchEvent = Regex.Match(value, "^(\"[^\"]+\") ([^\"\\(]+) (\"[^\"]+\") ([^\"\\(]+) (\"[^\"]+\")(.*)$");

                var firstEvent  = mchEvent.Groups[2].Value;
                // var secondEvent = mchEvent.Groups[4].Value;

                // Safely get the first player.
                Player firstPlayer;
                Player.TryParse(mchEvent.Groups[1].Value, out firstPlayer);

                var props       = ParseProperties(mchEvent.Groups[6].Value);

                // Safely get the second player and the noun.
                Player secondPlayer;
                String noun     = String.Empty;

                // Parse the second player's details.
                if (Player.TryParse(mchEvent.Groups[3].Value, out secondPlayer))
                {
                    noun    = mchEvent.Groups[5].Value;
                }
                else
                {
                    Player.TryParse(mchEvent.Groups[5].Value, out secondPlayer);
                    noun    = mchEvent.Groups[3].Value;
                }

                noun        = noun.Trim('\"');

                switch (firstEvent.ToLower())
                {
                    case "killed":
                        {
                            // Event 057.
                            this.OnKill(new PlayerOnPlayerEventArgs(original, HLEvent.Kill, "057", firstPlayer, secondPlayer, noun, props));

                            break;
                        }
                    case "attacked":
                        {
                            // Event 058.
                            this.OnAttack(new PlayerOnPlayerEventArgs(original, HLEvent.Attack, "058", firstPlayer, secondPlayer, noun, props));

                            break;
                        }
                    case "triggered":
                        {
                            // Event 059.
                            this.OnPlayerOnPlayer(new PlayerOnPlayerEventArgs(original, HLEvent.PlayerOnPlayer, "059", firstPlayer, secondPlayer, noun, props));

                            break;
                        }
                    case "tell":
                        {
                            // Event 066.
                            this.OnPrivateChat(new PlayerOnPlayerEventArgs(original, HLEvent.PrivateChat, "066", firstPlayer, secondPlayer, noun, props));

                            break;
                        }
                    default:
                        {
                            // Unknown event - throw an exception with the server's resposne.
                            this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));

                            break;
                        }
                }
            }
            // Matches events 050, 053, 054, 055, 056, 060, 063a, 063b, 068, 069.
            else if (Regex.IsMatch(value, "^\"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\"(.*)$"))
            {
                var mchEvent    = Regex.Match(value, "^(\"[^\"]+\") ([^\"\\(]+) \"([^\"]+)\"(.*)$");

                // Safely get the player.
                Player player;
                Player.TryParse(mchEvent.Groups[1].Value, out player);

                var currEvent   = mchEvent.Groups[2].Value;
                var noun        = mchEvent.Groups[3].Value;
                var props       = ParseProperties(mchEvent.Groups[4].Value);

                switch (currEvent.ToLower())
                {
                    case "connected, address":
                        {
                            // Event 050.
                            this.OnConnection(new PlayerActionEventArgs(original, HLEvent.Connection, "050", player, noun, props));

                            break;
                        }
                    case "committed suicide with":
                        {
                            // Event 053.
                            this.OnSuicide(new PlayerActionEventArgs(original, HLEvent.Suicide, "053", player, noun, props));

                            break;
                        }
                    case "joined team":
                        {
                            // Event 054.
                            this.OnTeamSelection(new PlayerActionEventArgs(original, HLEvent.TeamSelection, "054", player, noun, props));

                            break;
                        }
                    case "changed role to":
                        {
                            // Event 055.
                            this.OnRoleSelection(new PlayerActionEventArgs(original, HLEvent.RoleSelection, "055", player, noun, props));

                            break;
                        }
                    case "changed name to":
                        {
                            // Event 056.
                            this.OnChangeName(new PlayerActionEventArgs(original, HLEvent.ChangeName, "056", player, noun, props));

                            break;
                        }
                    case "triggered":
                        {
                            // Event 060.
                            this.OnPlayerAction(new PlayerActionEventArgs(original, HLEvent.PlayerAction, "060", player, noun, props));

                            break;
                        }
                    case "say":
                        {
                            // Event 063a.
                            this.OnChat(new PlayerActionEventArgs(original, HLEvent.Chat, "063a", player, noun, props));

                            break;
                        }
                    case "say_team":
                        {
                            // Event 063b.
                            this.OnTeamChat(new PlayerActionEventArgs(original, HLEvent.Chat, "063b", player, noun, props));

                            break;
                        }
                    case "selected weapon":
                        {
                            // Event 068.
                            this.OnWeaponSelection(new PlayerActionEventArgs(original, HLEvent.WeaponSelection, "068", player, noun, props));

                            break;
                        }
                    case "acquired weapon":
                        {
                            // Event 069.
                            this.OnWeaponPickup(new PlayerActionEventArgs(original, HLEvent.WeaponPickup, "069", player, noun, props));

                            break;
                        }
                    default:
                        {
                            // Unknown event - throw an exception with the server's resposne.
                            this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));

                            break;
                        }
                }
            }
            // Matches events 050b, 051, 052.
            else if (Regex.IsMatch(value, "^\"([^\"]+)\" ([^\\(]+)(.*)$"))
            {
                var mchEvent    = Regex.Match(value, "^(\"[^\"]+\") ([^\\(]+)(.*)$");

                // Safely get the player.
                Player player;
                Player.TryParse(mchEvent.Groups[1].Value, out player);

                var currEvent   = mchEvent.Groups[2].Value;
                var props       = ParseProperties(mchEvent.Groups[3].Value);

                switch (currEvent.ToLower())
                {
                    case "steam userid validated":
                        {
                            // Event 050b.
                            this.OnValidation(new PlayerEventArgs(original, HLEvent.Validation, "050b", player, props));

                            break;
                        }
                    case "entered the game":
                        {
                            // Event 051.
                            this.OnEnterGame(new PlayerEventArgs(original, HLEvent.EnterGame, "051", player, props));

                            break;
                        }
                    case "disconnected":
                        {
                            // Event 052.
                            this.OnDisconnection(new PlayerEventArgs(original, HLEvent.Disconnection, "052", player, props));

                            break;
                        }
                    default:
                        {
                            // Unknown event - throw an exception with the server's resposne.
                            this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));

                            break;
                        }
                }
            }
            // Matches events 061, 064.
            else if (Regex.IsMatch(value, "^Team \"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\"(.*)$"))
            {
                var mchEvent    = Regex.Match(value, "^Team \"([^\"]+)\" ([^\"\\(]+) \"([^\"]+)\"(.*)$");

                var team        = mchEvent.Groups[1].Value;
                var currEvent   = mchEvent.Groups[2].Value;
                var noun        = mchEvent.Groups[3].Value;
                var props       = ParseProperties(mchEvent.Groups[4].Value);

                switch (currEvent.ToLower())
                {
                    case "triggered":
                        {
                            // Event 061.
                            this.OnTeamAction(new TeamEventArgs(original, HLEvent.TeamAction, "061", team, noun, props));

                            break;
                        }
                    case "formed alliance with team":
                        {
                            // Event 064.
                            this.OnTeamAlliance(new TeamEventArgs(original, HLEvent.TeamAlliance, "064", team, noun, props));

                            break;
                        }
                    default:
                        {
                            // Unknown event - throw an exception with the server's resposne.
                            this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));

                            break;
                        }
                }
            }
            // Matches events 062, 003a, 003b, 005, 006.
            else if (Regex.IsMatch(value, "^([^\"\\(]+) \"([^\"]+)\"(.*)$"))
            {
                var mchEvent    = Regex.Match(value, "^([^\"\\(]+) \"([^\"]+)\"(.*)$");

                var currEvent   = mchEvent.Groups[1].Value;
                var noun        = mchEvent.Groups[2].Value;
                var props       = ParseProperties(mchEvent.Groups[3].Value);

                switch (currEvent.ToLower())
                {
                    case "world triggered":
                        {
                            // Event 062.
                            this.OnWorldAction(new ServerEventArgs(original, HLEvent.WorldAction, "062", noun, props));

                            break;
                        }
                    case "loading map":
                        {
                            // Event 003a.
                            this.OnMapLoading(new ServerEventArgs(original, HLEvent.ChangeMap, "003a", noun, props));

                            break;
                        }
                    case "started map":
                        {
                            // Event 003b.
                            this.OnMapStarting(new ServerEventArgs(original, HLEvent.ChangeMap, "003b", noun, props));

                            break;
                        }
                    case "server name is":
                        {
                            // Event 005.
                            this.OnServerName(new ServerEventArgs(original, HLEvent.ServerName, "005", noun, props));

                            break;
                        }
                    case "server say":
                        {
                            // Event 006.
                            this.OnServerSay(new ServerEventArgs(original, HLEvent.ServerSay, "006", noun, props));

                            break;
                        }
                    default:
                        {
                            // Unknown event - throw an exception with the server's resposne.
                            this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));

                            break;
                        }
                }
            }
            // Matches events 001a, 001c, 002a, 002b.
            else if (Regex.IsMatch(value, "^([^\"\\(]+)(.*)$"))
            {
                var mchEvent = Regex.Match(value, "^([^\"\\(]+)(.*)$");

                var currEvent = mchEvent.Groups[1].Value.Trim();
                var props = ParseProperties(mchEvent.Groups[2].Value);

                switch (currEvent.ToLower())
                {
                    case "server cvars start":
                        {
                            // Event 001a.
                            this.OnCvarStart(new InfoEventArgs(original, HLEvent.Cvar, "001a", props));

                            break;
                        }
                    case "server cvars end":
                        {
                            // Event 001c.
                            this.OnCvarEnd(new InfoEventArgs(original, HLEvent.Cvar, "001c", props));

                            break;
                        }
                    case "log file started":
                        {
                            // Event 002a.
                            this.OnLogFileStart(new InfoEventArgs(original, HLEvent.LogFileInfo, "002a", props));

                            break;
                        }
                    case "log file closed":
                        {
                            // Event 002b.
                            this.OnLogFileClosed(new InfoEventArgs(original, HLEvent.LogFileInfo, "002b", props));

                            break;
                        }
                    default:
                        {
                            // Unknown event - throw an exception with the server's resposne.
                            this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));

                            break;
                        }
                }
            }
            // If the event wasn't found, throw an exception.
            else
            {
                this.OnException(new ExceptionEventArgs(new UnknownEventException(value)));
            }
        }

        #endregion

        #endregion
    }
}