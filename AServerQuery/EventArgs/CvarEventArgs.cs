// CvarEventArgs.cs is part of AServerQuery.
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
    /// Provides data for event 001b.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    public class CvarEventArgs : GoldSrcEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the Cvar which is being changed.
        /// </summary>
        public String Cvar
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the new value of the cvar.
        /// </summary>
        public String Value
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.CvarEventArgs" /> class using
        /// the specified arguments.
        /// </summary>
        /// <param name="LogLine">A <see cref="System.String" /> containing the log line which was received.</param>
        /// <param name="Event">A <see cref="AServerQuery.HLEvent" /> representing the event.</param>
        /// <param name="EventName">A <see cref="System.String" /> representing the event.</param>
        /// <param name="Cvar">The Cvar which is being changed.</param>
        /// <param name="Value">The new value of the cvar.</param>
        /// <param name="Properties">
        /// A case-insensative <see cref="System.Collections.Generic.Dictionary{TKey, TValue}" /> of the properties.
        /// </param>
        public CvarEventArgs(
                            String                      LogLine,
                            HLEvent                     Event,
                            String                      EventName,
                            String                      Cvar,
                            String                      Value,
                            Dictionary<String, String>  Properties)
            : base(LogLine, Event, EventName, Properties)
        {
            this.Cvar   = Cvar;
            this.Value  = Value;
        }

        #endregion
    }
}