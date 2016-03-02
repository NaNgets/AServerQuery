// PlayerInfo.cs is part of AServerQuery.
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
    /// Represents a player's info as responded from the server by an A2S_PLAYER query.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries#A2S_PLAYER" />
    public class PlayerInfo
    {
        #region Properties

        /// <summary>
        /// Gets the player's index.
        /// </summary>
        /// <remarks>The index into [0.. Num Players] for this entry.</remarks>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the player's name.
        /// </summary>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the player's kill count.
        /// </summary>
        public int Kills
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time the player has been connected to the server.
        /// </summary>
        public TimeSpan Time
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
        private PlayerInfo()
        {
        }

        /// <summary>
        /// Constructs the PlayerInfo instance.
        /// </summary>
        /// <param name="Index">The player's index.</param>
        /// <param name="Name">The player's name.</param>
        /// <param name="Kills">The player's kill count.</param>
        /// <param name="Time">The time the player has been connected to the server.</param>
        public PlayerInfo(int Index, String Name, int Kills, TimeSpan Time)
        {
            this.Index  = Index;
            this.Name   = Name;
            this.Kills  = Kills;
            this.Time   = Time;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a player's info to
        /// its <see cref="AServerQuery.PlayerInfo" /> equivalent. A return value indicates
        /// whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A byte array representing a player's info to convert.</param>
        /// <param name="offset">The index from which to start reading the player's info from.</param>
        /// <param name="player">
        /// When this method returns, contains the player's info value equivalent
        /// to the byte array contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
        /// conversion failed. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(Byte[] value, ref int offset, out PlayerInfo player)
        {
            try
            {
                player = PlayerInfo.Parse(value, ref offset);

                return (true);
            }
            catch
            {
                player = null;

                return (false);
            }
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a player's info to
        /// its <see cref="AServerQuery.PlayerInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A byte array representing a player's info to convert.</param>
        /// <param name="offset">The index from which to start reading the player's info from.</param>
        /// <returns>A player's info represented by the given byte array.</returns>
        public static PlayerInfo Parse(Byte[] value, ref int offset)
        {
            var info    = new PlayerInfo();

            int original = offset;

            // Get the player's info.
            info.Index  = value[offset++];
            info.Name   = Util.ReadString(value, ref offset);
            info.Kills  = BitConverter.ToInt32(value, offset);
            offset      += 4;
            info.Time   = TimeSpan.FromSeconds(BitConverter.ToSingle(value, offset));
            offset      += 4;

            // Copy the player's info data to the original data property.
            info.Data   = new Byte[offset - original];
            Array.Copy(value, original, info.Data, 0, offset - original);

            return (info);
        }

        #endregion
    }
}