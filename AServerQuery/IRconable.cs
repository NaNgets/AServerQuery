// IRconable.cs is part of AServerQuery.
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
using System.Net;

namespace AServerQuery
{
    /// <summary>
    /// Defines methods to Rcon a game server.
    /// </summary>
    public interface IRconable
    {
        #region Properties

        /// <summary>
        /// Gets the game server's Rcon password.
        /// </summary>
        /// <value>The game server's Rcon password.</value>
        String RconPassword
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects the instance to the game server's Rcon.
        /// </summary>
        /// <returns>Whether the connection was successful or not.</returns>
        bool ConnectRcon();

        /// <summary>
        /// Disconnects from the game server's Rcon.
        /// </summary>
        void DisconnectRcon();

        /// <summary>
        /// Sends the server an Rcon command and returns the game server's response.
        /// </summary>
        /// <param name="value">The Rcon command to send to the game server.</param>
        /// <returns>The server's response.</returns>
        String QueryRcon(String value);

        /// <summary>
        /// Queries the game server for the requested Cvar and returns its value.
        /// </summary>
        /// <param name="value">The Cvar name to request its value from the game server.</param>
        /// <returns>The value of the requested Cvar.</returns>
        String GetCvar(String value);

        /// <summary>
        /// Sets the given Cvar with the given value.
        /// </summary>
        /// <param name="cvar">The Cvar name to set its value on the game server.</param>
        /// <param name="value">The value to set the Cvar to.</param>
        void SetCvar(String cvar, String value);

        /// <summary>
        /// Determins whether the game server is logging or not.
        /// </summary>
        /// <returns><see langword="true" /> if the game server is logging, <see langword="false" /> otherwise.</returns>
        bool IsLogging();

        /// <summary>
        /// Starts the log on the game server.
        /// </summary>
        void StartLog();

        /// <summary>
        /// Stops the log on the game server.
        /// </summary>
        void StopLog();

        /// <summary>
        /// Queries the game server for the list of the current address list to which the server is logging to.
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="System.Net.IPEndPoint" /> of the log addresses on the server.</returns>
        IEnumerable<IPEndPoint> GetLogAddresses();

        /// <summary>
        /// Queries the game server to add the given <paramref name="value" /> to the logging addresses.
        /// </summary>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to add to the log addresses.</param>
        void AddLogAddress(IPEndPoint value);

        /// <summary>
        /// Queries the game server to delete the given <paramref name="value" /> from the logging addresses.
        /// </summary>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to delete from the log addresses.</param>
        void DeleteLogAddress(IPEndPoint value);

        /// <summary>
        /// Queries the game server for the status.
        /// </summary>
        /// <returns>A <see cref="AServerQuery.StatusInfo" /> representing the game server's Rcon status.</returns>
        StatusInfo GetStatus();

        #endregion
    }
}
