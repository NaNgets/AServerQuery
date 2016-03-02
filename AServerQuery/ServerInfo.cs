// ServerInfo.cs is part of AServerQuery.
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

namespace AServerQuery
{
    /// <summary>
    /// Represents a server's info as responded from the server by an A2S_INFO query.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries#A2S_INFO" />
    public class ServerInfo
    {
        #region Properties

        /// <summary>
        /// Gets the type of the info.
        /// </summary>
        public Byte Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the network version. Also refered to as protocol version.
        /// </summary>
        public Byte Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the game server IP address and port.
        /// <para>Exists only in old GoldSrc servers where Type == 'm' (0x6D).</para>
        /// </summary>
        public String GameIP
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the server's name.
        /// </summary>
        public String ServerName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current map being played.
        /// </summary>
        public String Map
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the folder containing the game files.
        /// </summary>
        public String GameDir
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a friendly name for the game type.
        /// </summary>
        public String GameDesc
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Steam Application ID.
        /// <para>Exists only in new GoldSrc servers or Source servers where Type == 'I' (0x49).</para>
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/Steam_Application_IDs" />
        public short AppID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of players currently on the server.
        /// </summary>
        public int NumPlayers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum allowed players for the server.
        /// </summary>
        public int MaxPlayers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of bot players currently on the server.
        /// </summary>
        public int NumBots
        {
            get;
            private set;
        }

        /// <summary>
        /// 'l' for listen, 'd' for dedicated, 'p' for HLTV\SourceTV.
        /// </summary>
        public Char Dedicated
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the host operating system. 'l' for Linux, 'w' for Windows.
        /// </summary>
        public Char OS
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether a password is required to join this server.
        /// </summary>
        public bool Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the ModInfo is also received.
        /// <para>Exists only in old GoldSrc servers where Type == 'm' (0x6D).</para>
        /// </summary>
        public bool IsMod
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ModInfo that was received with the info.
        /// <para>Exists only in old GoldSrc servers where Type == 'm' (0x6D).</para>
        /// </summary>
        public ModInfo Mod
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this server is VAC secured.
        /// </summary>
        public bool Secure
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the version of the game.
        /// <para>Exists only in new GoldSrc servers or Source servers where Type == 'I' (0x49).</para>
        /// </summary>
        public String GameVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// If present, this specifies which additional data fields is included.
        /// <para>Exists only in new GoldSrc servers or Source servers where Type == 'I' (0x49).</para>
        /// </summary>
        public Byte EDF
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the original given data which was parsed.
        /// </summary>
        public Byte[] Data
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Private default constructor to be used by the Parse commands.
        /// </summary>
        private ServerInfo()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a server's info to
        /// its <see cref="AServerQuery.ServerInfo" /> equivalent. A return value indicates
        /// whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A byte array representing a server's info to convert.</param>
        /// <param name="info">
        /// When this method returns, contains the server's info value equivalent to the byte array
        /// contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
        /// conversion failed. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(Byte[] value, out ServerInfo info)
        {
            info = ServerInfo.InternalParse(value, true);
            return (info != null);
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a server's info to
        /// its <see cref="AServerQuery.ServerInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A byte array representing a server's info to convert.</param>
        /// <returns>A server's info represented by the given <paramref name="value" />.</returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.ServerInfo" />.</exception>
        public static ServerInfo Parse(Byte[] value)
        {
            return (ServerInfo.InternalParse(value, false));
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a server's info to
        /// its <see cref="AServerQuery.ServerInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A byte array representing a server's info to convert.</param>
        /// <param name="tryParse"><see langword="true" /> to suppress exceptions, <see langword="false" /> otherwise.</param>
        /// <returns>
        /// A server's info represented by the given <paramref name="value" />.
        /// <see langword="null" /> if <paramref name="value" /> is not a valid <see cref="AServerQuery.ServerInfo" /> and <paramref name="tryParse" /> is <see langword="true" />.
        /// </returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.ServerInfo" />.</exception>
        private static ServerInfo InternalParse(Byte[] value, bool tryParse)
        {
            // If match failed, throw an error or return null (depending on the tryParse argument).
            if (value.GetLength(0) < 5)
            {
                if (tryParse)
                {
                    return (null);
                }
                else
                {
                    throw new FormatException("Value doesn't match ServerInfo pattern.");
                }
            }

            var info    = new ServerInfo();

            // Copy the given value to a new Byte[] and save it.
            info.Data   = (value.Clone() as Byte[]);

            // Initialize the offset.
            int offset = 4;

            info.Type   = value[offset++];

            // According to http://developer.valvesoftware.com/wiki/Server_queries#Source_servers_2
            if (info.Type == 0x49)
            {
                info.Version    = value[offset++];
                info.ServerName = Util.ReadString(value, ref offset);
                info.Map        = Util.ReadString(value, ref offset);
                info.GameDir    = Util.ReadString(value, ref offset);
                info.GameDesc   = Util.ReadString(value, ref offset);

                info.AppID      = BitConverter.ToInt16(value, offset);
                offset          += 2;

                info.NumPlayers = value[offset++];
                info.MaxPlayers = value[offset++];
                info.NumBots    = value[offset++];
                info.Dedicated  = (Char)value[offset++];
                info.OS         = (Char)value[offset++];
                info.Password   = (value[offset++] == 0x01);
                info.Secure     = (value[offset++] == 0x01);
                info.GameVersion = Util.ReadString(value, ref offset);
                info.EDF        = value[offset++];
            }
            // According to http://developer.valvesoftware.com/wiki/Server_queries#Goldsource_servers_2
            else if (info.Type == 0x6D)
            {
                info.GameIP     = Util.ReadString(value, ref offset);
                info.ServerName = Util.ReadString(value, ref offset);
                info.Map        = Util.ReadString(value, ref offset);
                info.GameDir    = Util.ReadString(value, ref offset);
                info.GameDesc   = Util.ReadString(value, ref offset);
                info.NumPlayers = value[offset++];
                info.MaxPlayers = value[offset++];
                info.Version    = value[offset++];
                info.Dedicated  = (Char)value[offset++];
                info.OS         = (Char)value[offset++];
                info.Password   = (value[offset++] == 0x01);
                info.IsMod      = (value[offset++] == 0x01);

                if (info.IsMod)
                {
                    info.Mod    = ModInfo.Parse(value, ref offset);
                }

                info.Secure     = (value[offset++] == 0x01);
                info.NumBots    = value[offset++];
            }

            return (info);
        }

        #endregion
    }
}