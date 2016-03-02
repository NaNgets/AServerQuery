// HLEvent.cs is part of AServerQuery.
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

namespace AServerQuery
{
    /// <summary>
    /// Represents an HL log event according to the log standrads.
    /// </summary>
    /// <remarks>
    /// Note that some events have more than one representation,
    /// or that one representation has more than one event.
    /// </remarks>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard"/>
    public enum HLEvent
    {
        #region HL Engine

        /// <summary>
        /// Event 001. Occurs when server starts Cvars (001a), changes a Cvar (001b) or ends Cvars (001c).
        /// </summary>
        /// <remarks>
        /// <para>In TFC, if tfc_clanbattle is 1, event 001a doesn't happen.</para>
        /// <para>Event 001b occurs both at the beginning of a round and whenever someone changes a cvar over rcon.</para>
        /// <para>In TFC, if tfc_clanbattle is 0, event 001c doesn't happen. You can instead use event 005, since it comes right after.</para>
        /// </remarks>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#001._Cvars" />
        Cvar            = 001,

        /// <summary>
        /// Event 002. Occurs when log file has started (002a) or closed (002b).
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#002._Log_Files" />
        LogFileInfo     = 002,

        /// <summary>
        /// Event 003. Occurs when map is loaded (003a) or started (003b).
        /// </summary>
        /// <remarks>
        /// <para>Event 003a replaces the current "Spawning server" message.</para>
        /// <para>Event 003b replaces the current "Map CRC" message. The message should appear AFTER all PackFile messages, to indicate when the game actually commences.</para>
        /// </remarks>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#003._Change_Map" />
        ChangeMap       = 003,

        /// <summary>
        /// Event 004. Occurs when an Rcon command is executed successfully (004a) or bad (004b).
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#004._Rcon" />
        Rcon            = 004,

        /// <summary>
        /// Event 005. Occurs when the server displays its name.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#005._Server_Name" />
        ServerName      = 005,

        /// <summary>
        /// Event 006. Occurs when the server says a message.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#006._Server_Say" />
        ServerSay       = 006,

        #endregion

        #region Game

        /// <summary>
        /// Event 050. Occurs when a player connects to the server.
        /// </summary>
        /// <remarks>
        /// This event number is identical to the Validation event (050b),
        /// make sure not to get mixed up with it.
        /// </remarks>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#050._Connection" />
        Connection      = 050,

        /// <summary>
        /// Event 050b. Occurs when a player is validated.
        /// </summary>
        /// <remarks>
        /// This event number is identical to the Connection event (050),
        /// make sure not to get mixed up with it.
        /// </remarks>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#050b._Validation" />
        Validation      = 050,

        /// <summary>
        /// Event 051. Occurs when a player enters the game.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#051._Enter_Game" />
        EnterGame       = 051,

        /// <summary>
        /// Event 052. Occurs when a player disconnects.
        /// </summary>
        /// <remarks>
        /// This event number is identical to the Kick event (052b),
        /// make sure not to get mixed up with it.
        /// </remarks>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#052._Disconnection" />
        Disconnection   = 052,

        /// <summary>
        /// Event 052b. Occurs when a player is kicked from the server.
        /// </summary>
        /// <remarks>
        /// This event number is identical to the Disconnection event (052),
        /// make sure not to get mixed up with it.
        /// </remarks>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#052b._Kick" />
        Kick            = 052,

        /// <summary>
        /// Event 053. Occurs when a player commits suicide.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#053._Suicides" />
        Suicide         = 053,

        /// <summary>
        /// Event 054. Occurs when a player selects a team.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#054._Team_Selection" />
        TeamSelection   = 054,

        /// <summary>
        /// Event 055. Occurs when a player selects a role.
        /// <para>This event covers classes in games like TFC, FLF and DOD.</para>
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#055._Role_Selection" />
        RoleSelection   = 055,

        /// <summary>
        /// Event 056. Occurs when a player changes his name.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#056._Change_Name" />
        ChangeName      = 056,

        /// <summary>
        /// Event 057. Occurs when a player kills another player.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#057._Kills" />
        Kill            = 057,
        
        /// <summary>
        /// Event 058. Occurs when a player attacks another player.
        /// <para>This event allows for recording of partial kills and teammate friendly-fire injuries.
        /// If the injury results in a kill, a Kill message (057) should be logged instead/also.</para>
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#058._Injuring" />
        Attack          = 058,

        /// <summary>
        /// Event 059. Occurs when a player triggers an action on another player.
        /// <para>This event allows for logging of a wide range of events where one player performs an action on another player.
        /// For example, in TFC this event may cover medic healings and infections, sentry gun destruction, spy uncovering, etc.</para>
        /// <para>More detail about the action can be given by appending more properties to the end of the event.</para>
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#059._Player-Player_Actions" />
        PlayerOnPlayer  = 059,

        /// <summary>
        /// Event 060. Occurs when a player commits an action.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#060._Player_Objectives.2FActions" />
        PlayerAction    = 060,

        /// <summary>
        /// Event 061. Occurs when a team commits an action.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#061._Team_Objectives.2FActions" />
        TeamAction      = 061,

        /// <summary>
        /// Event 062. Occurs when the wolrd commits an action.
        /// <para>This event allows logging of anything which does not happen in response to the actions of a player or team.
        /// For example a gate opening at the start of a round.</para>
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#062._World_Objectives.2FActions" />
        WorldAction     = 062,

        /// <summary>
        /// Event 063. Occurs when a player sends a message, either to the server (063a) or to his team (063b).
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#063._Chat" />
        Chat            = 063,

        /// <summary>
        /// Event 064. Occurs when a team forms an alliance.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#064._Team_Alliances" />
        TeamAlliance    = 064,

        /// <summary>
        /// Event 065. Occurs when a team's score is displayed.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#065._Round-End_Team_Score_Report" />
        TeamScore       = 065,

        /// <summary>
        /// Event 066. Occurs when a player private messages another player.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#066._Private_Chat" />
        PrivateChat     = 066,

        /// <summary>
        /// Event 067. Occurs when a player's score is displayed.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#067._Round-End_Player_Score_Report" />
        PlayerScore     = 067,

        /// <summary>
        /// Event 068. Occurs when a player selects a weapon.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#068._Weapon_Selection" />
        WeaponSelection = 068,

        /// <summary>
        /// Event 069. Occurs when a player picks up a weapon.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard#069._Weapon_Pickup" />
        WeaponPickup    = 069

        #endregion
    }
}