// SourceServer.cs is part of AServerQuery.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AServerQuery
{
    /// <summary>
    /// Represents a Source game server.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
    /// <seealso href="http://developer.valvesoftware.com/wiki/Source_RCON_Protocol" />
    public class SourceServer : ValveServer, IDisposable, IRconable
    {
        #region Inner Classes
        
        /// <summary>
        /// Contains request and response codes constants.
        /// </summary>
        /// <seealso href="http://developer.valvesoftware.com/wiki/Source_RCON_Protocol" />
        public enum CommandType : int
        {
            /// <summary>
            /// The response command type (for executed commands or other info).
            /// </summary>
            SERVERDATA_RESPONSE_VALUE   = 0,

            /// <summary>
            /// The response command type for authing.
            /// </summary>
            SERVERDATA_AUTH_RESPONSE    = 2,

            /// <summary>
            /// The request command type for executing a command.
            /// </summary>
            SERVERDATA_EXECCOMMAND      = 2,

            /// <summary>
            /// The request command type for authing.
            /// </summary>
            SERVERDATA_AUTH             = 3
        }

        /// <summary>
        /// Represents a Rcon packet.
        /// </summary>
        internal class RconPacket
        {
            #region Properties

            /// <summary>
            /// Gets whether or not the authentication has failed or not. 
            /// </summary>
            internal bool HasAuthenticationFailed
            {
                get
                {
                    return (this.PacketId == -1);
                }
            }

            /// <summary>
            /// Gets the packet size.
            /// </summary>
            internal int Size
            {
                get
                {
                    return (
                        BitConverter.GetBytes(this.PacketId).Length +
                        BitConverter.GetBytes((int)this.Command).Length +
                        Util.ConvertToByteArray(this.Body).Length +
                        this.Suffix.Length);
                }
            }

            /// <summary>
            /// Gets the ID for the packet.
            /// </summary>
            internal int PacketId { get; private set; }

            /// <summary>
            /// Gets the command type for the packet.
            /// </summary>
            internal CommandType Command { get; private set; }

            /// <summary>
            /// Gets the body of the Rcon packet.
            /// </summary>
            internal string Body { get; private set; }
            
            /// <summary>
            /// Gets the string which closes the packet.
            /// </summary>
            internal byte[] Suffix
            {
                get
                {
                    return (new byte[] { 0 });
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructs the Rcon packet with the given details.
            /// </summary>
            /// <param name="packetId">The packet ID.</param>
            /// <param name="command">The command type.</param>
            /// <param name="body">The packet's body.</param>
            internal RconPacket(int packetId, CommandType command, string body)
            {
                this.PacketId   = packetId;
                this.Command    = command;
                this.Body       = body;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Gets a byte array representing the Rcon packet.
            /// </summary>
            /// <returns>A byte array representing the Rcon packet.</returns>
            internal byte[] GetBytes()
            {
                return (Util.ConcatByteArrays(
                            BitConverter.GetBytes(this.Size),
                            BitConverter.GetBytes(this.PacketId),
                            BitConverter.GetBytes((int)this.Command),
                            Util.ConvertToByteArray(this.Body),
                            this.Suffix));
            }

            /// <summary>
            /// Converts the given <paramref name="value" /> representing a Rcon packet to
            /// its <see cref="AServerQuery.SourceServer.RconPacket" /> equivalent. A return value indicates
            /// whether the conversion succeeded.
            /// </summary>
            /// <param name="value">A byte array representing a Rcon packet to convert.</param>
            /// <param name="packet">
            /// When this method returns, contains the Rcon packet value equivalent to the byte array
            /// contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
            /// conversion failed. This parameter is passed uninitialized.
            /// </param>
            /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
            public static bool TryParse(Byte[] value, out RconPacket packet)
            {
                packet = RconPacket.InternalParse(value, true);
                return (packet != null);
            }

            /// <summary>
            /// Converts the given <paramref name="value" /> representing a Rcon packet to
            /// its <see cref="AServerQuery.SourceServer.RconPacket" /> equivalent.
            /// </summary>
            /// <param name="value">A byte array representing a Rcon packet to convert.</param>
            /// <returns>A Rcon packet represented by the given <paramref name="value" />.</returns>
            /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.SourceServer.RconPacket" />.</exception>
            public static RconPacket Parse(Byte[] value)
            {
                return (RconPacket.InternalParse(value, false));
            }

            /// <summary>
            /// Converts the given <paramref name="value" /> representing a Rcon's packet info to
            /// its <see cref="AServerQuery.SourceServer.RconPacket" /> equivalent.
            /// </summary>
            /// <param name="value">A byte array representing a Rcon's packet info to convert.</param>
            /// <param name="tryParse"><see langword="true" /> to suppress exceptions, <see langword="false" /> otherwise.</param>
            /// <returns>
            /// A Rcon packet represented by the given <paramref name="value" />.
            /// <see langword="null" /> if <paramref name="value" /> is not a valid <see cref="AServerQuery.SourceServer.RconPacket" /> and <paramref name="tryParse" /> is <see langword="true" />.
            /// </returns>
            /// <exception cref="System.FormatException"><paramref name="value" /> is not a valid <see cref="AServerQuery.SourceServer.RconPacket" />.</exception>
            private static RconPacket InternalParse(byte[] value, bool tryParse)
            {
                // If match failed, throw an error or return null (depending on the tryParse argument).
                if (value.Length < 10)
                {
                    if (tryParse)
                    {
                        return (null);
                    }
                    else
                    {
                        throw new FormatException("Value doesn't match RconPacket pattern.");
                    }
                }

                // Get the packet's size.
                int size            = BitConverter.ToInt32(value, 0);

                // Initialize the offset.
                int offset          = 4;

                // Get the packet's ID.
                int packetId        = BitConverter.ToInt32(value, offset);
                offset              += 4;

                // Get the packet's command.
                CommandType command = (CommandType)BitConverter.ToInt32(value, offset);
                offset              += 4;

                // Get the packet's body (ignore the suffix).
                string body         = System.Text.Encoding.Default.GetString(value, offset, value.Length - offset - 2);
                offset              += value.Length - offset - 2;

                // Return a new Rcon packet.
                return (new RconPacket(packetId, command, body));
            }

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this instance is connected to the Rcon or not.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return ((this.RconClient != null) && (this.RconClient.Connected));
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.TcpClient" /> used to connect to the Rcon of the game server.
        /// </summary>
        /// <value>The Rcon client for the server.</value>
        public TcpClient RconClient
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the lock which makes sure the Rcon will be safe.
        /// </summary>
        private ReaderWriterLockSlim RconClientLocker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current Packet ID.
        /// </summary>
        /// <see cref="AServerQuery.SourceServer.GetNextPacketId"/>
        public int PacketId
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the <see cref="AServerQuery.SourceServer" /> instance with the given parameters.
        /// </summary>
        /// <param name="server">The <see cref="System.Net.IPEndPoint" /> of the game server.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="server" /> is <see langword="null" />.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">
        /// An error occurred when accessing the socket. See the Remarks section for more information.
        /// </exception>
        public SourceServer(IPEndPoint server)
            : this(server, String.Empty)
        {
        }

        /// <summary>
        /// Constructs the <see cref="AServerQuery.SourceServer" /> instance with the given parameters.
        /// </summary>
        /// <param name="server">The <see cref="System.Net.IPEndPoint" /> of the game server.</param>
        /// <param name="rconPassword">The Rcon password for the game server.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="server" /> is <see langword="null" />.</exception>
        public SourceServer(IPEndPoint server, String rconPassword)
            : base(server, rconPassword)
        {
            this.RconClientLocker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~SourceServer()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        #region IDisposable Members

        /// <summary>
        /// Closes the connection to the server and stops listening.
        /// </summary>
        /// <remarks>
        /// This method does NOT remove the server from any registered logging servers.
        /// </remarks>
        /// <param name="disposing"><see langword="true" /> if the data members should be disposed, <see langword="false" /> otherwise.</param>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        protected override void Dispose(bool disposing)
        {
            try
            {
                this.DisconnectRcon();

                if (disposing)
                {
                    this.RconClientLocker.Dispose();
                    this.RconClientLocker   = null;
                    this.PacketId           = 0;
                }

                base.Dispose(disposing);
            }
            catch { }
        }

        #endregion

        #region Sockets methods


        #endregion

        #region Rcon methods

        /// <summary>
        /// Gets the next request id to be used in the Rcon commands.
        /// </summary>
        /// <returns>The next packet id to be used in the Rcon commands.</returns>
        public int GetNextPacketId()
        {
            return (++this.PacketId);
        }

        /// <summary>
        /// Reads a packet from the stream and returns it.
        /// </summary>
        /// <param name="stream">The stream to read the packet from.</param>
        /// <returns>The Rcon packet read.</returns>
        /// <exception cref="System.FormatException">The bytes read from the stream are not a valid <see cref="AServerQuery.SourceServer.RconPacket" />.</exception>
        private RconPacket ReceivePacket(NetworkStream stream)
        {
            // Get the response size.
            var sizeInBytes = new byte[4];
            stream.Read(sizeInBytes, 0, 4);
            var size = BitConverter.ToInt32(sizeInBytes, 0);

            // Get the rest of the packet.
            var packetData = new byte[size];
            stream.Read(packetData, 0, size);

            return (RconPacket.Parse(Util.ConcatByteArrays(sizeInBytes, packetData)));
        }

        /// <summary>
        /// Connects the instance to the game server's Rcon.
        /// </summary>
        /// <returns>Whether the connection was successful or not.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.AlreadyConnectedException">
        /// The <see cref="AServerQuery.SourceServer" /> instance is already listening.
        /// </exception>
        public bool ConnectRcon()
        {
            return (this.ConnectRcon(new IPEndPoint(IPAddress.Any, 0)));
        }

        /// <summary>
        /// Connects the instance to the game server's Rcon.
        /// </summary>
        /// <param name="localEP">The local end-point to connect to the Rcon from.</param>
        /// <returns>Whether the connection was successful or not.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.AlreadyConnectedException">
        /// The <see cref="AServerQuery.SourceServer" /> instance is already listening.
        /// </exception>
        public bool ConnectRcon(IPEndPoint localEP)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is already connected, throw the appropriate exception.
            if (this.IsConnected)
            {
                throw new AlreadyConnectedException();
            }

            try
            {
                this.RconClientLocker.EnterWriteLock();

                // Create a new instance of the UdpClient to listen to the log.
                this.RconClient                 = new TcpClient(localEP);
                this.RconClient.ReceiveTimeout  = timeOut;

                // Connect the client and get the stream.
                this.RconClient.Connect(this.Server);
                var stream                      = this.RconClient.GetStream();

                // Create the authentication packet.
                var authPacket = new RconPacket(
                                        this.GetNextPacketId(),
                                        CommandType.SERVERDATA_AUTH,
                                        this.RconPassword);

                // Authenticate.
                stream.Write(
                    authPacket.GetBytes(),
                    0,
                    authPacket.GetBytes().Length);
                stream.Flush();
                
                // Disregard the first response.
                this.ReceivePacket(stream);
                // Parse the auth response.
                var authResponse    = this.ReceivePacket(stream);

                // If the authentication has failed, throw an appropriate exception.
                if (authResponse.HasAuthenticationFailed)
                {
                    throw new BadRconPasswordException(authResponse.Body);
                }

                return (authResponse.Command == CommandType.SERVERDATA_AUTH_RESPONSE);
            }
            // If there is any socket exception, encapsulate it as an IOException and throw it.
            catch (SocketException exp)
            {
                if (this.RconClient != null)
                {
                    this.RconClient.Close();
                }

                this.RconClient = null;

                throw new IOException("Exception while Rcon connecting to the server.", exp);
            }
            catch
            {
                if (this.RconClient != null)
                {
                    this.RconClient.Close();
                }

                this.RconClient = null;

                throw;
            }
            finally
            {
                this.RconClientLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disconnects from the game server's Rcon.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        public void DisconnectRcon()
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            this.RconClientLocker.EnterWriteLock();

            // If the instance is connected, disconnect.
            if (this.IsConnected)
            {
                try
                {
                    this.RconClient.Client.Shutdown(SocketShutdown.Both);

                    this.RconClient.Close();
                    this.RconClient = null;
                }
                // If there is any socket exception, encapsulate it as an IOException and throw it.
                catch (SocketException exp)
                {
                    throw new IOException("Error while trying to disconnect the Rcon from the server.", exp);
                }
            }

            this.RconClientLocker.ExitWriteLock();
        }

        /// <summary>
        /// Sends the server an Rcon command and returns the game server's response.
        /// </summary>
        /// <param name="value">The Rcon command to send to the game server.</param>
        /// <returns>The server's response.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        public String QueryRcon(String value)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            try
            {
                this.RconClientLocker.EnterWriteLock();

                // Get the stream.
                var stream      = this.RconClient.GetStream();

                var packetId    = this.GetNextPacketId();
                var flushId     = this.GetNextPacketId();

                // Create the command execution packet.
                var cmdPacket   = new RconPacket(
                                        packetId,
                                        CommandType.SERVERDATA_EXECCOMMAND,
                                        value);

                // Create the flush packet.
                var flushPacket = new RconPacket(
                                        flushId,
                                        CommandType.SERVERDATA_EXECCOMMAND,
                                        String.Empty);

                // Send both packets.
                stream.Write(
                    cmdPacket.GetBytes(),
                    0,
                    cmdPacket.GetBytes().Length);
                stream.Write(
                    flushPacket.GetBytes(),
                    0,
                    flushPacket.GetBytes().Length);
                stream.Flush();

                // Vars to hold the response and current packet.
                StringBuilder response  = new StringBuilder();
                RconPacket currPacket   = null;

                // While we still haven't received the flush packet (and we didn't pass it), continue receiving the packet..
                do
                {
                    currPacket = this.ReceivePacket(stream);

                    // Only if the data is relevant to our packet, collect the response.
                    if (currPacket.PacketId == packetId)
                    {
                        response.Append(currPacket.Body);
                    }
                } while (currPacket.PacketId < flushId);

                return (response.ToString());
            }
            // If there is any socket exception, encapsulate it as an IOException and throw it.
            catch (SocketException exp)
            {
                throw new IOException("Exception while sending a Rcon command to the server.", exp);
            }
            finally
            {
                this.RconClientLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Queries the game server for the requested Cvar and returns its value.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <param name="value">The Cvar name to request its value from the game server.</param>
        /// <returns>The value of the requested Cvar.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.FormatException">If the game server's response was invalid.</exception>
        public String GetCvar(String value)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var response    = this.QueryRcon(value);

            // Extract the cvar value using a regex expression.
            var cvarValue   = Regex.Match(response, String.Format("\"{0}\" = \"([^\"]*)\"", value), RegexOptions.IgnoreCase);

            // If the response contains the cvar's value, return it.
            if (cvarValue.Success)
            {
                return (cvarValue.Groups[1].Value);
            }
            // Else, if the game server didn't return a valid response, throw an exception.
            else
            {
                throw new FormatException(response);
            }
        }

        /// <summary>
        /// Sets the given Cvar with the given value.
        /// </summary>
        /// <param name="cvar">The Cvar name to set its value on the game server.</param>
        /// <param name="value">The value to set the Cvar to.</param>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentNullException">The given <paramref name="cvar" /> name is null or empty.</exception>
        public void SetCvar(String cvar, String value)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            // If the given cvar name is null or empty, throw an appropriate exception.
            if (String.IsNullOrWhiteSpace(cvar))
            {
                throw new ArgumentNullException("cvar");
            }

            this.QueryRcon(String.Format("{0} \"{1}\"", cvar, value));
        }

        /// <summary>
        /// Determins whether the game server is logging or not.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <returns><see langword="true" /> if the game server is logging, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public bool IsLogging()
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var response = this.QueryRcon("log");

            return (!response.Contains("not currently logging"));
        }

        /// <summary>
        /// Starts the log on the game server.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void StartLog()
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            this.QueryRcon("log on");
        }

        /// <summary>
        /// Stops the log on the game server.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void StopLog()
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            this.QueryRcon("log off");
        }

        /// <summary>
        /// Queries the game server for the list of the current address list to which the server is logging to.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="System.Net.IPEndPoint" /> of the log addresses on the server.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public IEnumerable<IPEndPoint> GetLogAddresses()
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            // Get the log addresses from the server.
            var response        = this.QueryRcon("logaddress_list");

            // Separate all the addresses.
            var addressesMatch  = Regex.Matches(response, "((?:\\d{1,3}\\.){3}\\d{1,3}):(\\d{1,5})");

            // Go over each address matched and yield return it.
            for (int currAddress = 0; currAddress < addressesMatch.Count; currAddress++)
            {
                yield return (new IPEndPoint(
                                IPAddress.Parse(addressesMatch[currAddress].Groups[1].Value),
                                int.Parse(addressesMatch[currAddress].Groups[2].Value)));
            }
        }

        /// <summary>
        /// Queries the game server to add the given <paramref name="value" /> to the logging addresses.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to add to the log addresses.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.UnableToResolveException">If the server was unable to resolve the address.</exception>
        /// <exception cref="AServerQuery.AddressAlreadyInListException">If the address is already in the logging list.</exception>
        /// <exception cref="AServerQuery.GameServerException">If the query responded with an unknown response.</exception>
        public void AddLogAddress(IPEndPoint value)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var response = this.QueryRcon(String.Format("logaddress_add {0}:{1}", value.Address, value.Port));

            // If the server was unable to resolve the address, throw the appropriate exception.
            if (response.Contains("unable to resolve"))
            {
                throw new UnableToResolveException(response);
            }
            // Else, if the address is already in the list, throw the appropriate exception.
            else if (response.Contains("is already in the list"))
            {
                throw new AddressAlreadyInListException(response);
            }
            // Else, if the response did not respond with confirm deletion, throw the appropriate exception.
            else if (!response.Contains(String.Format("logaddress_add:  {0}:{1}", value.Address, value.Port)))
            {
                throw new GameServerException(response);
            }
        }

        /// <summary>
        /// Queries the game server to delete the given <paramref name="value" /> from the logging addresses.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to delete from the log addresses.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.UnableToResolveException">If the server was unable to resolve the address.</exception>
        /// <exception cref="AServerQuery.NoAddressesAddedException">If the address list is empty.</exception>
        /// <exception cref="AServerQuery.AddressNotFoundException">If the address couldn't be found in the list.</exception>
        /// <exception cref="AServerQuery.GameServerException">If the query responded with an unknown response.</exception>
        public void DeleteLogAddress(IPEndPoint value)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var response = this.QueryRcon(String.Format("logaddress_del {0} {1}", value.Address, value.Port));

            // If the server was unable to resolve the address, throw the appropriate exception.
            if (response.Contains("unable to resolve"))
            {
                throw new UnableToResolveException(response);
            }
            // Else, if the address couldn't be found in the list, throw the appropriate exception.
            else if (response.Contains("not found in the list"))
            {
                throw new AddressNotFoundException(response);
            }
            // Else, if the response did not respond with confirm deletion, throw the appropriate exception.
            else if (!response.Contains(String.Format("logaddress_del:  {0}:{1}", value.Address, value.Port)))
            {
                throw new GameServerException(response);
            }
        }

        /// <summary>
        /// Queries the game server for the status.
        /// </summary>
        /// <remarks>
        /// The time-out used is the time-out set to the client, which was set in the <see cref="AServerQuery.ValveServer.TimeOut" />.
        /// </remarks>
        /// <returns>A <see cref="AServerQuery.StatusInfo" /> representing the game server's Rcon status.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.SourceServer" /> is disposed.</exception>
        /// <exception cref="AServerQuery.NotConnectedException">If the Rcon is not connected.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public StatusInfo GetStatus()
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }

            // If the instance is not connected, throw the appropriate exception.
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            return (StatusInfo.Parse(this.QueryRcon("status")));
        }

        #endregion

        #endregion
    }
}