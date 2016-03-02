// RconEventArgs.cs is part of AServerQuery.
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
    /// Provides data for event 004 (a+b).
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    public class RconEventArgs : GoldSrcEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets whether the Rcon is good or not.
        /// </summary>
        public bool IsGood
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the Rcon challenge.
        /// </summary>
        public long Challenge
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the Rcon password.
        /// </summary>
        public String Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Rcon command.
        /// </summary>
        public String Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.IPEndPoint"/> of the Rcon sender.
        /// </summary>
        public IPEndPoint Sender
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.RconEventArgs" /> class using
        /// the specified arguments.
        /// </summary>
        /// <param name="LogLine">A <see cref="System.String" /> containing the log line which was received.</param>
        /// <param name="Event">A <see cref="AServerQuery.HLEvent" /> representing the event.</param>
        /// <param name="EventName">A <see cref="System.String" /> representing the event.</param>
        /// <param name="IsGood">true if the Rcon is good, false otherwise.</param>
        /// <param name="Challenge">The Rcon challenge.</param>
        /// <param name="Password">The Rcon password.</param>
        /// <param name="Command">The Rcon command.</param>
        /// <param name="Sender">The <see cref="System.Net.IPEndPoint"/> of the Rcon sender.</param>
        /// <param name="Properties">
        /// A case-insensative <see cref="System.Collections.Generic.Dictionary{TKey, TValue}" /> of the properties.
        /// </param>
        public RconEventArgs(
                            String                      LogLine,
                            HLEvent                     Event,
                            String                      EventName,
                            bool                        IsGood,
                            long                        Challenge,
                            String                      Password,
                            String                      Command,
                            IPEndPoint                  Sender,
                            Dictionary<String, String>  Properties)
            : base(LogLine, Event, EventName, Properties)
        {
            this.IsGood     = IsGood;
            this.Challenge  = Challenge;
            this.Password   = Password;
            this.Command    = Command;
            this.Sender     = Sender;
        }

        #endregion
    }
}