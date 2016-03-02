// PlayerOnPlayerEventArgs.cs is part of AServerQuery.
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
    /// Provides data for events 057, 058, 059 and 066.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    public class PlayerOnPlayerEventArgs : GoldSrcEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="AServerQuery.Player" /> representing the player who triggered the action.
        /// </summary>
        public Player Triggerer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="AServerQuery.Player" /> representing the target player on the action.
        /// </summary>
        public Player Target
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the noun in the action (weapon/action/message).
        /// </summary>
        public String Noun
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.PlayerOnPlayerEventArgs" /> class using
        /// the specified arguments.
        /// </summary>
        /// <param name="LogLine">A <see cref="System.String" /> containing the log line which was received.</param>
        /// <param name="Event">A <see cref="AServerQuery.HLEvent" /> representing the event.</param>
        /// <param name="EventName">A <see cref="System.String" /> representing the event.</param>
        /// <param name="Triggerer">The player who triggered the action.</param>
        /// <param name="Target">The target player on the action.</param>
        /// <param name="Noun">The noun in the action (weapon/action/message).</param>
        /// <param name="Properties">
        /// A case-insensative <see cref="System.Collections.Generic.Dictionary{TKey, TValue}" /> of the properties.
        /// </param>
        public PlayerOnPlayerEventArgs(
                            String                      LogLine,
                            HLEvent                     Event,
                            String                      EventName,
                            Player                      Triggerer,
                            Player                      Target,
                            String                      Noun,
                            Dictionary<String, String>  Properties)
            : base(LogLine, Event, EventName, Properties)
        {
            this.Triggerer  = Triggerer;
            this.Target     = Target;
            this.Noun       = Noun;
        }

        #endregion
    }
}