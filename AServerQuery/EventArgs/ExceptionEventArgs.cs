// ExceptionEventArgs.cs is part of AServerQuery.
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
    /// Provides data for the <see cref="AServerQuery.ValveServer.Exception" /> event.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the thrown exception.
        /// </summary>
        public Exception Exp
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.ExceptionEventArgs" /> class using
        /// the specified <see cref="System.Exception" />.
        /// </summary>
        /// <param name="Exp">The thrown <see cref="System.Exception" />.</param>
        public ExceptionEventArgs(Exception Exp)
        {
            this.Exp    = Exp;
        }

        #endregion
    }
}