// DataReceivedEventArgs.cs is part of AServerQuery.
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
    /// Provides data for the <see cref="AServerQuery.ValveServer.DataReceived" /> event.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the received data.
        /// </summary>
        public String Data
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.DataReceivedEventArgs" /> class using
        /// the specified <see cref="System.String" />.
        /// </summary>
        /// <param name="Data">The received data.</param>
        public DataReceivedEventArgs(String Data)
        {
            this.Data   = Data;
        }

        #endregion
    }
}