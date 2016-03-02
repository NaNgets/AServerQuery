// HltvInfo.cs is part of AServerQuery.
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
    /// Represents a hltv's info as responded from the server by a "status" Rcon query.
    /// </summary>
    public class HltvInfo : UserInfo
    {
        #region Data Members

        /// <summary>
        /// The <see cref="System.Text.RegularExpressions.Regex" /> pattern to match the HLTV to.
        /// </summary>
        public const String HltvPattern =
            "\"([^\"]+)\"\\s+(\\d+)\\s+(HLTV)\\s+hltv:(\\d+)/(\\d+)\\s+delay:(\\d+)\\s+(\\S+)\\s+((?:\\d{1,3}\\.){3}\\d{1,3}):(\\d{1,5})";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the amount of spectators in the HLTV.
        /// </summary>
        public int Spectators
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the amount of slots used in the HLTV.
        /// </summary>
        public int Slots
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the amount of the HLTV delay in seconds.
        /// </summary>
        public int Delay
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Private default constructor to be used by the Parse commands.
        /// </summary>
        private HltvInfo()
        {
        }

        /// <summary>
        /// Constructs the HltvInfo instance.
        /// </summary>
        /// <param name="Name">The HLTV's name.</param>
        /// <param name="UserID">The HLTV's ID.</param>
        /// <param name="UniqueID">The HLTV's unique ID (should be "HLTV").</param>
        /// <param name="Spectators">The amount of spectators in the HLTV.</param>
        /// <param name="Slots">The amount of slots used in the HLTV.</param>
        /// <param name="Delay">The amount of the HLTV delay in seconds.</param>
        /// <param name="Time">The HLTV's time in the game server.</param>
        /// <param name="Address">The HLTV's address.</param>
        public HltvInfo(String Name, int UserID, String UniqueID, int Spectators, int Slots, int Delay, TimeSpan Time, IPEndPoint Address)
            : base(Name, UserID, UniqueID, -1, Time, -1, -1, Address)
        {
            this.Spectators = Spectators;
            this.Slots      = Slots;
            this.Delay      = Delay;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Converts the given <paramref name="value" /> representing a HLTV's info to
        /// its <see cref="AServerQuery.HltvInfo" /> equivalent. A return value indicates
        /// whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A string representing a HLTV's info to convert.</param>
        /// <param name="user">
        /// When this method returns, contains the HLTV's info value equivalent
        /// to the string contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
        /// conversion failed. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(String value, out HltvInfo user)
        {
            user = HltvInfo.InternalParse(value, true);
            return (user != null);
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a HLTV's info to
        /// its <see cref="AServerQuery.HltvInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a HLTV's info to convert.</param>
        /// <returns>A HLTV's info represented by the given <paramref name="value" />.</returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.HltvInfo"/>.</exception>
        public static new HltvInfo Parse(String value)
        {
            return (HltvInfo.InternalParse(value, false));
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a HLTV's info to
        /// its <see cref="AServerQuery.HltvInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a HLTV's info to convert.</param>
        /// <param name="tryParse"><see langword="true" /> to suppress exceptions, <see langword="false" /> otherwise.</param>
        /// <returns>
        /// A HLTV's info represented by the given <paramref name="value" />.
        /// <see langword="null" /> if <paramref name="value" /> is not a valid <see cref="AServerQuery.HltvInfo" /> and <paramref name="tryParse" /> is <see langword="true" />.
        /// </returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.HltvInfo" />.</exception>
        private static HltvInfo InternalParse(String value, bool tryParse)
        {
            // Match the HLTV.
            var mchHltv = Regex.Match(value, HltvPattern);

            // If match failed, throw an error or return null (depending on the tryParse argument).
            if (!mchHltv.Success && (mchHltv.Groups.Count != 9))
            {
                if (tryParse)
                {
                    return (null);
                }
                else
                {
                    throw new FormatException("Value doesn't match HLTV pattern.");
                }
            }

            return (new HltvInfo(
                        mchHltv.Groups[1].Value,
                        int.Parse(mchHltv.Groups[2].Value),
                        mchHltv.Groups[3].Value,
                        int.Parse(mchHltv.Groups[4].Value),
                        int.Parse(mchHltv.Groups[5].Value),
                        int.Parse(mchHltv.Groups[6].Value),
                        TimeSpan.Parse(mchHltv.Groups[7].Value),
                        new IPEndPoint(IPAddress.Parse(mchHltv.Groups[8].Value),
                                        int.Parse(mchHltv.Groups[9].Value)))
                        { Data = value });
        }

        #endregion
    }
}