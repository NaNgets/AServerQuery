// GoldSrcEventArgs.cs is part of AServerQuery.
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

namespace AServerQuery
{
    /// <summary>
    /// AServerQuery.GoldSrcArgs is the base class for classes containing <see cref="AServerQuery.GoldSrcServer" /> event data.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    public class GoldSrcEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="AServerQuery.HLEvent" /> representing the event.
        /// </summary>
        public HLEvent Event
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a <see cref="System.String" /> representing the event.
        /// </summary>
        public String EventName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a <see cref="System.String" /> containing the log line which was received from the game server.
        /// </summary>
        public String LogLine
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the case-insensative <see cref="System.Collections.Generic.Dictionary{TKey, TValue}" /> of the properties.
        /// </summary>
        public Dictionary<String, String> Properties
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.GoldSrcEventArgs" /> class.
        /// </summary>
        /// <param name="LogLine">A <see cref="System.String" /> containing the log line which was received.</param>
        /// <param name="Event">A <see cref="AServerQuery.HLEvent" /> representing the event.</param>
        /// <param name="EventName">A <see cref="System.String" /> representing the event.</param>
        /// <param name="Properties">
        /// A case-insensative <see cref="System.Collections.Generic.Dictionary{TKey, TValue}" /> of the properties.
        /// </param>
        public GoldSrcEventArgs(String LogLine, HLEvent Event, String EventName, Dictionary<String, String> Properties)
        {
            this.LogLine    = LogLine;
            this.Event      = Event;
            this.EventName  = EventName;
            this.Properties = Properties;
        }

        #endregion
    }
}