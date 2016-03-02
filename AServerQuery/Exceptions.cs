// Exceptions.cs is part of AServerQuery.
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
    /// The exception that is thrown when a game server error occurs.
    /// </summary>
    public class GameServerException : Exception
    {
        /// <summary>
        /// The game server's original response which raised the exception.
        /// </summary>
        public String Response
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.GameServerException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public GameServerException(String response)
        {
            this.Response = response;
        }
    }

    /// <summary>
    /// The exception that is thrown when the instance is already connected.
    /// </summary>
    public class AlreadyConnectedException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.AlreadyConnectedException" /> class
        /// with the specified game server's response.
        /// </summary>
        public AlreadyConnectedException()
            : base(String.Empty)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the instance is already listening.
    /// </summary>
    public class AlreadyListeningException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.AlreadyListeningException" /> class
        /// with the specified game server's response.
        /// </summary>
        public AlreadyListeningException()
            : base(String.Empty)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the address which is trying to be added is already in the logging list.
    /// </summary>
    public class AddressAlreadyInListException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.AddressAlreadyInListException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public AddressAlreadyInListException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the address which is trying to be deleted is not in the logging list.
    /// </summary>
    public class AddressNotFoundException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.AddressNotFoundException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public AddressNotFoundException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when a Rcon command was sent with the wrong Rcon challenge.
    /// </summary>
    public class BadRconChallengeException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.BadRconChallengeException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public BadRconChallengeException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when a Rcon command was sent with the wrong Rcon password.
    /// </summary>
    public class BadRconPasswordException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.BadRconPasswordException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public BadRconPasswordException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when a query was sent with the wrong query challenge.
    /// </summary>
    public class BadQueryChallengeException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.BadQueryChallengeException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public BadQueryChallengeException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the Rcon is not connected.
    /// </summary>
    public class NotConnectedException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.NotConnectedException" /> class
        /// with the specified game server's response.
        /// </summary>
        public NotConnectedException()
            : base(String.Empty)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when there are no addesses in the list yet.
    /// </summary>
    public class NoAddressesAddedException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.NoAddressesAddedException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public NoAddressesAddedException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the address could not be resolved.
    /// </summary>
    public class UnableToResolveException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.UnableToResolveException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public UnableToResolveException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when an unknown event was received by the Rcon log.
    /// </summary>
    public class UnknownEventException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.UnknownEventException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public UnknownEventException(String response)
            : base(response)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when an unknown header was received as response from a query or a Rcon command.
    /// </summary>
    public class UnknownHeaderException : GameServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AServerQuery.UnknownHeaderException" /> class
        /// with the specified game server's response.
        /// </summary>
        /// <param name="response">The game server's response.</param>
        public UnknownHeaderException(String response)
            : base(response)
        {
        }
    }
}
