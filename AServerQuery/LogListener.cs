// LogListener.cs is part of AServerQuery.
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
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AServerQuery
{
    /// <summary>
    /// Used to listen to more than one server using the same endpoint\port.
    /// </summary>
    public sealed class LogListener
    {
        #region Data Members

        /// <summary>
        /// The collection to hold the listening servers.
        /// </summary>
        private Dictionary<IPEndPoint, ValveServer> servers = new Dictionary<IPEndPoint, ValveServer>();

        #region Events

        /// <summary>
        /// An event to handle exceptions being throwen inside the listening thread.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Exception;

        #region Event triggers

        /// <summary>
        /// Calls the <see cref="AServerQuery.LogListener.Exception" /> event with the given <paramref name="args" />.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        private void OnException(ExceptionEventArgs args)
        {
            // Get the event's targets.
            var targets = this.Exception;

            // If the targets are not empty, trigger them.
            if (targets != null)
            {
                targets(this, args);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.UdpClient" /> used to listen to logs from the game server.
        /// </summary>
        /// <value>The log listener for the server.</value>
        private UdpClient Client
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the lock which makes sure the log listening will be safe.
        /// </summary>
        private ReaderWriterLockSlim ClientLocker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether this instance is listening for incoming data or not.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return (this.Client != null);
            }
        }

        /// <summary>
        /// Gets the port this instance is listening on. 0 if not listening.
        /// </summary>
        public int Port
        {
            get
            {
                if (!this.IsListening)
                {
                    return 0;
                }

                return (((IPEndPoint)this.Client.Client.LocalEndPoint).Port);
            }
        }

        /// <summary>
        /// Gets the servers which logs will be processed.
        /// </summary>
        public ReadOnlyCollection<ValveServer> Servers
        {
            get
            {
                return (new ReadOnlyCollection<ValveServer>(new List<ValveServer>(this.servers.Values)));
            }
        }

        #endregion

        #region Contsructors

        /// <summary>
        /// Constructs the <see cref="AServerQuery.LogListener" /> instance.
        /// </summary>
        public LogListener()
        {
            this.ClientLocker   = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        #region Collection Methods

        /// <summary>
        /// Adds the given server to the listening servers collection.
        /// </summary>
        /// <param name="server">The server to listen to.</param>
        public void AddServer(ValveServer server)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (server.IsDisposed)
            {
                throw new ObjectDisposedException(server.GetType().FullName);
            }

            this.servers.Add(server.Server, server);
        }

        /// <summary>
        /// Removes the given server's address and port from the listening servers collection.
        /// </summary>
        /// <param name="server">The server to remove.</param>
        public void RemoveServer(IPEndPoint server)
        {
            this.servers.Remove(server);
        }

        /// <summary>
        /// Removes the given server from the listening servers collection.
        /// </summary>
        /// <param name="server">The server to remove.</param>
        public void RemoveServer(ValveServer server)
        {
            // If the instance has been disposed, throw the appropriate exception.
            if (server.IsDisposed)
            {
                throw new ObjectDisposedException(server.GetType().FullName);
            }

            this.RemoveServer(server.Server);
        }

        #endregion

        #region Listening Methods

        /// <summary>
        /// Starts listening to log data from the servers.
        /// </summary>
        /// <remarks>
        /// Responses are sent to the <see cref="AServerQuery.ValveServer.OnDataReceived" /> event or matched events
        /// on the matching <see cref="AServerQuery.ValveServer" /> instance.
        /// </remarks>
        /// <param name="port">The local port to listen to the Rcon log with.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The port parameter is greater than <see cref="System.Net.IPEndPoint.MaxPort" /> or less
        /// than <see cref="System.Net.IPEndPoint.MinPort" />.
        /// </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.AlreadyListeningException">
        /// The <see cref="AServerQuery.LogListener" /> instance is already listening.
        /// </exception>
        public void Listen(int port)
        {
            this.Listen(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        /// Starts listening to log data from the servers.
        /// </summary>
        /// <remarks>
        /// Responses are sent to the <see cref="AServerQuery.ValveServer.OnDataReceived" /> event or matched events
        /// on the matching <see cref="AServerQuery.ValveServer" /> instance.
        /// </remarks>
        /// <param name="localEP">The local end-point to listen to the Rcon log with.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.AlreadyListeningException">
        /// The <see cref="AServerQuery.LogListener" /> instance is already listening.
        /// </exception>
        public void Listen(IPEndPoint localEP)
        {
            // If the instance is already listening, throw the appropriate exception.
            if (this.IsListening)
            {
                throw new AlreadyListeningException();
            }

            try
            {
                this.ClientLocker.EnterWriteLock();

                // Create a new instance of the UdpClient to listen to the log.
                this.Client = new UdpClient(localEP);

                // Start listening to the server.
                this.Client.BeginReceive(this.WaitForData, null);
            }
            // If there is any socket exception, encapsulate it as an IOException and throw it.
            catch (SocketException exp)
            {
                if (this.Client != null)
                {
                    this.Client.Close();
                }

                this.Client = null;

                throw new IOException("Exception while trying to listen.", exp);
            }
            catch
            {
                if (this.Client != null)
                {
                    this.Client.Close();
                }

                this.Client = null;

                throw;
            }
            finally
            {
                this.ClientLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Stops listening to data received from he server.
        /// </summary>
        /// <remarks>
        /// This method only stops listening to the log, it does NOT remove the server from any registered logging servers.
        /// </remarks>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void Stop()
        {
            this.ClientLocker.EnterWriteLock();

            // If the instance is listening, stops listening.
            if (this.IsListening)
            {
                try
                {
                    this.Client.Client.Shutdown(SocketShutdown.Both);

                    this.Client.Close();
                    this.Client = null;
                }
                // If there is any socket exception, encapsulate it as an IOException and throw it.
                catch (SocketException exp)
                {
                    throw new IOException("Error while trying to stop listening.", exp);
                }
            }

            this.ClientLocker.ExitWriteLock();
        }

        /// <summary>
        /// Waits for data from the servers.
        /// </summary>
        private void WaitForData(IAsyncResult ar)
        {
            // Create an IPEndPoint to record the sender's address.
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                Byte[] data = null;

                this.ClientLocker.EnterReadLock();

                if (this.Client != null)
                {
                    // Receive a response from the server.
                    data = this.Client.EndReceive(ar, ref endPoint);
                }

                this.ClientLocker.ExitReadLock();

                // Only if we've received something and the server exists in the listening servers, process the given data.
                if ((data != null) && (data.Length > 0) && (this.servers.ContainsKey(endPoint)))
                {
                    // Process the received response.
                    this.servers[endPoint].ProcessLog(Util.ConvertToString(data));
                }
            }
            // If any exception was throwen, throw it on the OnException event.
            catch (Exception exp)
            {
                this.OnException(new ExceptionEventArgs(exp));
            }

            this.ClientLocker.EnterReadLock();

            // Only if the client still exists, listen again.
            if (this.Client != null)
            {
                this.Client.BeginReceive(this.WaitForData, null);
            }

            this.ClientLocker.ExitReadLock();
        }

        #endregion

        #endregion
    }
}
