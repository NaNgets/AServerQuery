// StatusInfo.cs is part of AServerQuery.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AServerQuery
{
    /// <summary>
    /// Represents a game server's status info as responded from the server by a "status" Rcon query.
    /// </summary>
    public class StatusInfo
    {
        #region Data Members

        /// <summary>
        /// The <see cref="System.Text.RegularExpressions.Regex" /> pattern to match the GoldSrc engine status to.
        /// </summary>
        public const String GoldSrcStatusPattern  = "hostname:  ([^\\n\\r\\0]+).*" +
                                                    "version :  ([^\\n\\r\\0]+).*" +
                                                    "tcp/ip  :  ((?:\\d{1,3}\\.){3}\\d{1,3}):(\\d{1,5}).*" +
                                                    "map     :  ([^ ]+).*" +
                                                    "players :  (\\d+) active \\((\\d+) max\\).*?" +
                                                    "#.*?" +
                                                    "(# .*)?" +
                                                    "(\\d+) users[\\n\\r\\0]*$";

        /// <summary>
        /// The <see cref="System.Text.RegularExpressions.Regex" /> pattern to match the Source engine status to.
        /// </summary>
        public const String SourceStatusPattern   = "hostname: ([^\\n\\r\\0]+).*" +
                                                    "version : ([^\\n\\r\\0]+).*" +
                                                    "udp/ip  :  ((?:\\d{1,3}\\.){3}\\d{1,3}):(\\d{1,5}).*" +
                                                    "map     : ([^ ]+).*" +
                                                    "players : (\\d+) \\((\\d+) max\\).*?" +
                                                    "#.*?" +
                                                    "(# .*)?";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the game server's hostname.
        /// </summary>
        public String Hostname
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the game server's version.
        /// </summary>
        public String Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the game server's address.
        /// </summary>
        public IPEndPoint Address
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current map.
        /// </summary>
        /// <remarks>X, Y and Z coordinates are sent with the response, however they are not parsed.</remarks>
        public String Map
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of active players reported by the game server.
        /// </summary>
        public int ActivePlayers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the game server's maximum available player slots.
        /// </summary>
        public int MaxPlayers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the users reported by the game server.
        /// </summary>
        public ReadOnlyCollection<UserInfo> Users
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of users reported by the game server.
        /// </summary>
        public int UsersCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the original given data which was parsed.
        /// </summary>
        public String Data
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Private default constructor to be used by the Parse commands.
        /// </summary>
        private StatusInfo()
        {
            this.Users  = new ReadOnlyCollection<UserInfo>(new List<UserInfo>());
        }

        /// <summary>
        /// Constructs the StatusInfo instance.
        /// </summary>
        /// <param name="Hostname">The game server's hostname.</param>
        /// <param name="Version">The game server's version.</param>
        /// <param name="Address">The game server's address.</param>
        /// <param name="Map">The current map.</param>
        /// <param name="ActivePlayers">The number of active players reported by the game server.</param>
        /// <param name="MaxPlayers">The game server's maximum available player slots.</param>
        /// <param name="Users">The users reported by the game server.</param>
        /// <param name="UserCount">The number of users reported by the game server.</param>
        public StatusInfo(String Hostname,
                            String Version,
                            IPEndPoint Address,
                            String Map,
                            int ActivePlayers,
                            int MaxPlayers,
                            List<UserInfo> Users,
                            int UserCount)
            : this()
        {
            this.Hostname       = Hostname;
            this.Version        = Version;
            this.Address        = Address;
            this.Map            = Map;
            this.ActivePlayers  = ActivePlayers;
            this.MaxPlayers     = MaxPlayers;
            this.Users          = new ReadOnlyCollection<UserInfo>(new List<UserInfo>(Users));
            this.UsersCount     = UsersCount;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a status' info to
        /// its <see cref="AServerQuery.StatusInfo" /> equivalent. A return value indicates
        /// whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A string representing a status' info to convert.</param>
        /// <param name="status">
        /// When this method returns, contains the status' info value equivalent
        /// to the string contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
        /// conversion failed. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(String value, out StatusInfo status)
        {
            status = StatusInfo.InternalParse(value, true);
            return (status != null);
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a status's info to
        /// its <see cref="AServerQuery.StatusInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a status' info to convert.</param>
        /// <returns>A status' info represented by the given <paramref name="value" />.</returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.StatusInfo" />.</exception>
        public static StatusInfo Parse(String value)
        {
            return (StatusInfo.InternalParse(value, false));
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a status's info to
        /// its <see cref="AServerQuery.StatusInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A string representing a status' info to convert.</param>
        /// <param name="tryParse"><see langword="true" /> to suppress exceptions, <see langword="false" /> otherwise.</param>
        /// <returns>
        /// A status' info represented by the given <paramref name="value" />.
        /// <see langword="null" /> if <paramref name="value" /> is not a valid <see cref="AServerQuery.StatusInfo" /> and <paramref name="tryParse" /> is <see langword="true" />.
        /// </returns>
        /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.StatusInfo" />.</exception>
        private static StatusInfo InternalParse(String value, bool tryParse)
        {
            StatusInfo status       = null;
            bool matchSuccess       = false;

            var statusMatchSource   = Regex.Match(value,
                                        SourceStatusPattern,
                                        RegexOptions.Singleline);

            // If the status matches a Source engine status.
            if (statusMatchSource.Success)
            {
                matchSuccess        = true;
                status              = InternalParseSource(statusMatchSource);
            }
            // Else, check if the status matches a GoldSrc engine status.
            else
            {
                var statusMatchGoldSrc  = Regex.Match(value,
                                            GoldSrcStatusPattern,
                                            RegexOptions.Singleline);

                if (statusMatchGoldSrc.Success && (statusMatchGoldSrc.Groups.Count == 9))
                {
                    matchSuccess        = true;
                    status              = InternalParseGoldSrc(statusMatchGoldSrc);
                }
            }

            // If match failed, throw an error or return null (depending on the tryParse argument).
            if (!matchSuccess)
            {
                if (tryParse)
                {
                    return (null);
                }
                else
                {
                    throw new FormatException("Value doesn't match StatusInfo pattern.");
                }
            }

            // Copy the status' info to the original data property.
            status.Data = value;

            return (status);
        }

        /// <summary>
        /// Converts the given <paramref name="statusMatch" /> representing a Source engine status's info to
        /// its <see cref="AServerQuery.StatusInfo" /> equivalent.
        /// </summary>
        /// <param name="statusMatch">A <see cref="System.Text.RegularExpressions.Match" /> which matches a Source engine status.</param>
        /// <returns>
        /// A Source engine status' info represented by the given <paramref name="statusMatch" />.
        /// </returns>
        private static StatusInfo InternalParseSource(Match statusMatch)
        {
            var status              = new StatusInfo();

            // Get the status' info.
            status.Hostname         = statusMatch.Groups[1].Value;
            status.Version          = statusMatch.Groups[2].Value;
            status.Address          = new IPEndPoint(
                                        IPAddress.Parse(statusMatch.Groups[3].Value),
                                        int.Parse(statusMatch.Groups[4].Value));
            status.Map              = statusMatch.Groups[5].Value;
            status.ActivePlayers    = int.Parse(statusMatch.Groups[6].Value);
            status.MaxPlayers       = int.Parse(statusMatch.Groups[7].Value);
            status.UsersCount       = int.Parse(statusMatch.Groups[6].Value);

            // Get the users splitted to lines.
            var userLines           = statusMatch.Groups[8].Value.Split("\r\n".ToCharArray(),
                                                                        StringSplitOptions.RemoveEmptyEntries);
            var users               = new List<UserInfo>(status.UsersCount);

            // Go over each user line and parse the user.
            foreach (var currLine in userLines)
            {
                // If the current line matches the user's info pattern, parse it as a user.
                if (Regex.IsMatch(currLine, UserInfo.UserPattern))
                {
                    users.Add(UserInfo.Parse(currLine));
                }
                // Else, parse it as HLTV (there's no other known types of users).
                else
                {
                    users.Add(HltvInfo.Parse(currLine));
                }
            }

            status.Users            = new ReadOnlyCollection<UserInfo>(users);

            return (status);
        }

        /// <summary>
        /// Converts the given <paramref name="statusMatch" /> representing a GoldSrc engine status's info to
        /// its <see cref="AServerQuery.StatusInfo" /> equivalent.
        /// </summary>
        /// <param name="statusMatch">A <see cref="System.Text.RegularExpressions.Match" /> which matches a GoldSrc engine status.</param>
        /// <returns>
        /// A GoldSrc engine status' info represented by the given <paramref name="statusMatch" />.
        /// </returns>
        private static StatusInfo InternalParseGoldSrc(Match statusMatch)
        {
            var status              = new StatusInfo();

            // Get the status' info.
            status.Hostname         = statusMatch.Groups[1].Value;
            status.Version          = statusMatch.Groups[2].Value;
            status.Address          = new IPEndPoint(
                                        IPAddress.Parse(statusMatch.Groups[3].Value),
                                        int.Parse(statusMatch.Groups[4].Value));
            status.Map              = statusMatch.Groups[5].Value;
            status.ActivePlayers    = int.Parse(statusMatch.Groups[6].Value);
            status.MaxPlayers       = int.Parse(statusMatch.Groups[7].Value);
            status.UsersCount       = int.Parse(statusMatch.Groups[9].Value);

            // Get the users splitted to lines.
            var userLines           = statusMatch.Groups[8].Value.Split("\r\n".ToCharArray(),
                                                                        StringSplitOptions.RemoveEmptyEntries);
            var users               = new List<UserInfo>(status.UsersCount);

            // Go over each user line and parse the user.
            foreach (var currLine in userLines)
            {
                // If the current line matches the user's info pattern, parse it as a user.
                if (Regex.IsMatch(currLine, UserInfo.UserPattern))
                {
                    users.Add(UserInfo.Parse(currLine));
                }
                // Else, parse it as HLTV (there's no other known types of users).
                else
                {
                    users.Add(HltvInfo.Parse(currLine));
                }
            }

            status.Users            = new ReadOnlyCollection<UserInfo>(users);

            return (status);
        }

        #endregion
    }
}