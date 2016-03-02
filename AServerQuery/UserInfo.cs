// UserInfo.cs is part of AServerQuery.
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
using System.Net;
using System.Text.RegularExpressions;

namespace AServerQuery
{
    /// <summary>
    /// Represents a user's info as responded from the server by a "status" Rcon query.
    /// </summary>
    public class UserInfo
    {
        #region Data Members
        
        /// <summary>
        /// The <see cref="System.Text.RegularExpressions.Regex" /> pattern to match the user to.
        /// </summary>
        public const String UserPattern =
            "\"([^\"]+)\"\\s+(\\d+)\\s+(\\S+)\\s+(\\d+)\\s+(\\S+)\\s+(\\d+)\\s+(\\d+)\\s+((?:\\d{1,3}\\.){3}\\d{1,3}):(\\d{1,5})";
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets the user's ID.
        /// </summary>
        public int UserID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the user's name.
        /// </summary>
        public String Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the user's unique ID.
        /// </summary>
        public String UniqueID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the user's frag count.
        /// </summary>
        /// <remarks>Does not exist if the user is HLTV.</remarks>
        public int Frag
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time the user has been connected to the server.
        /// </summary>
        public TimeSpan Time
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the user's ping.
        /// </summary>
        /// <remarks>Does not exist if the user is HLTV.</remarks>
        public int Ping
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the user's loss.
        /// </summary>
        /// <remarks>Does not exist if the user is HLTV.</remarks>
        public int Loss
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the user's address and port.
        /// </summary>
        public IPEndPoint Address
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets whether the user is a HLTV or not.
        /// </summary>
        public bool IsHltv
        {
            get
            {
                return (this.UniqueID == "HLTV");
            }
        }

        /// <summary>
        /// Gets the original given data which was parsed.
        /// </summary>
        public String Data
        {
            get;
            protected set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Protected default constructor to be used by the Parse commands.
        /// </summary>
        protected UserInfo()
        {
        }

        /// <summary>
        /// Constructs the UserInfo instance.
        /// </summary>
        /// <param name="Name">The user's name.</param>
        /// <param name="UserID">The user's ID.</param>
        /// <param name="UniqueID">The user's unique ID (such as AuthID or "HLTV" for HLTVs).</param>
        /// <param name="Frag">The user's frag count.</param>
        /// <param name="Time">The user's time in the game server.</param>
        /// <param name="Ping">The user's ping.</param>
        /// <param name="Loss">The user's packet loss.</param>
        /// <param name="Address">The user's address.</param>
        public UserInfo(String Name, int UserID, String UniqueID, int Frag, TimeSpan Time, int Ping, int Loss, IPEndPoint Address)
        {
            this.Name       = Name;
            this.UserID     = UserID;
            this.UniqueID   = UniqueID;
            this.Frag       = Frag;
            this.Time       = Time;
            this.Ping       = Ping;
            this.Loss       = Loss;
            this.Address    = Address;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a user's info to
        /// its <see cref="AServerQuery.UserInfo" /> equivalent. A return value indicates
        /// whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A string representing a user's info to convert.</param>
        /// <param name="user">
        /// When this method returns, contains the user's info value equivalent
        /// to the string contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
        /// conversion failed. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(String value, out UserInfo user)
        {
            user = UserInfo.InternalParse(value, true);
            return (user != null);
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a user's info to
        /// its <see cref="AServerQuery.UserInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a user's info to convert.</param>
        /// <returns>A user's info represented by the given <paramref name="value" />.</returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.UserInfo" />.</exception>
        public static UserInfo Parse(String value)
        {
            return (UserInfo.InternalParse(value, false));
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a user's info to
        /// its <see cref="AServerQuery.UserInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a user's info to convert.</param>
        /// <param name="tryParse"><see langword="true" /> to suppress exceptions, <see langword="false" /> otherwise.</param>
        /// <returns>
        /// A user's info represented by the given <paramref name="value" />.
        /// <see langword="null" /> if <paramref name="value" /> is not a valid <see cref="AServerQuery.UserInfo" /> and <paramref name="tryParse" /> is <see langword="true" />.
        /// </returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.UserInfo" />.</exception>
        private static UserInfo InternalParse(String value, bool tryParse)
        {
            // Match the user.
            var mchUser = Regex.Match(value, UserPattern);

            // If match failed, throw an error or return null (depending on the tryParse argument).
            if (!mchUser.Success && (mchUser.Groups.Count != 9))
            {
                if (tryParse)
                {
                    return (null);
                }
                else
                {
                    throw new FormatException("Value doesn't match UserInfo pattern.");
                }
            }

            return (new UserInfo(
                        mchUser.Groups[1].Value,
                        int.Parse(mchUser.Groups[2].Value),
                        mchUser.Groups[3].Value,
                        int.Parse(mchUser.Groups[4].Value),
                        TimeSpan.Parse(mchUser.Groups[5].Value),
                        int.Parse(mchUser.Groups[6].Value),
                        int.Parse(mchUser.Groups[7].Value),
                        new IPEndPoint(IPAddress.Parse(mchUser.Groups[8].Value),
                                        int.Parse(mchUser.Groups[9].Value)))
                        { Data = value });
        }

        #endregion
    }
}