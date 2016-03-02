// Player.cs is part of AServerQuery.
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
using System.Text.RegularExpressions;

namespace AServerQuery
{
    /// <summary>
    /// Represents a player in the game server.
    /// </summary>
    public struct Player
    {
        #region Data Members

        /// <summary>
        /// The <see cref="System.Text.RegularExpressions.Regex" /> pattern to match the player to.
        /// </summary>
        public const    String  PlayerPattern  = "\"(.*?)<([^>]*)><([^>]*)><([^>]*)>\"";

        /// <summary>
        /// Represents an empty player. This field is read-only.
        /// </summary>
        public static readonly  Player  Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the player's nick.
        /// </summary>
        public String Nick
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the player's unique ID. -1 if empty.
        /// </summary>
        public int UID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the player's AuthID.
        /// </summary>
        public String AuthID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the player's team.
        /// </summary>
        public String Team
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Generates the empty player.
        /// </summary>
        static Player()
        {
            Empty = new Player(String.Empty, -1, String.Empty, String.Empty);
        }

        /// <summary>
        /// Constructs the Player instance.
        /// </summary>
        /// <param name="Nick">The player's nick.</param>
        /// <param name="UID">The player's unique ID.</param>
        /// <param name="AuthID">The player's AuthID.</param>
        /// <param name="Team">The player's team.</param>
        public Player(String Nick, int UID, String AuthID, String Team)
            : this()
        {
            this.Nick   = Nick;
            this.UID    = UID;
            this.AuthID = AuthID;
            this.Team   = Team;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a string representing the player and his details.
        /// </summary>
        /// <returns>A string representing the player and his details.</returns>
        public override string ToString()
        {
            return (String.Format("\"{0}<{1}><{2}><{3}>\"", this.Nick, this.UID, this.AuthID, this.Team));
        }

        /// <summary>
        /// Converts the string representing the player to its <see cref="AServerQuery.Player" /> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A string representing a player to convert.</param>
        /// <param name="player">
        /// When this method returns, contains the player value equivalent to the string contained in
        /// <paramref name="value" />, if the conversion succeeded, or <see cref="AServerQuery.Player.Empty" /> player if the conversion failed.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(String value, out Player player)
        {
            player = Player.InternalParse(value, true);
            return (player.UID != Player.Empty.UID);
        }

        /// <summary>
        /// Converts the string representing the player to its <see cref="AServerQuery.Player" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a player to convert.</param>
        /// <returns>A player represented by the given string.</returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.Player"/>.</exception>
        public static Player Parse(String value)
        {
            return (Player.InternalParse(value, false));
        }

        /// <summary>
        /// Converts the string representing the player to its <see cref="AServerQuery.Player" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a player to convert.</param>
        /// <param name="tryParse"><see langword="true" /> to suppress exceptions, <see langword="false" /> otherwise.</param>
        /// <returns>
        /// A player represented by the given string.
        /// <see cref="AServerQuery.Player.Empty" /> if <paramref name="value" /> is not a valid <see cref="AServerQuery.Player" /> and <paramref name="tryParse" /> is <see langword="true" />.
        /// </returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.Player" />.</exception>
        private static Player InternalParse(String value, bool tryParse)
        {
            // Match the player.
            var mchPlayer = Regex.Match(value, PlayerPattern);

            // If match failed, throw an error or return empty player (depending on the tryParse argument).
            if (!mchPlayer.Success && (mchPlayer.Groups.Count != 5))
            {
                if (tryParse)
                {
                    return (Player.Empty);
                }
                else
                {
                    throw new FormatException("Value doesn't match Player pattern.");
                }
            }

            // Declare a value to hold the unique ID.
            int nUID;

            // Try parsing the unique ID from the given value.
            int.TryParse(mchPlayer.Groups[2].Value, out nUID);

            // Create a new PlayerDetails instance according to the given value, and return it.
            return (new Player(
                            mchPlayer.Groups[1].Value,
                            nUID,
                            mchPlayer.Groups[3].Value,
                            mchPlayer.Groups[4].Value));
        }

        #endregion
    }
}
