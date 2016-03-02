// GoldSrcServer.cs is part of AServerQuery.
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
using System.Text.RegularExpressions;
using System.Threading;

namespace AServerQuery
{
    /// <summary>
    /// Represents a GoldSource game server.
    /// </summary>
    /// <seealso href="http://developer.valvesoftware.com/wiki/HL_Log_Standard" />
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries" />
    public class GoldSrcServer : ValveServer, IDisposable, IRconable
    {
        #region Properties

        /// <summary>
        /// Gets the Rcon challenge.
        /// </summary>
        /// <value>The Rcon challenge.</value>
        public long RconChallenge
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the <see cref="AServerQuery.GoldSrcServer" /> instance with the given parameters.
        /// </summary>
        /// <param name="server">The <see cref="System.Net.IPEndPoint" /> of the game server.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="server" /> is <see langword="null" />.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">
        /// An error occurred when accessing the socket. See the Remarks section for more information.
        /// </exception>
        public GoldSrcServer(IPEndPoint server)
            : this(server, String.Empty)
        {
        }

        /// <summary>
        /// Constructs the <see cref="AServerQuery.GoldSrcServer" /> instance with the given parameters.
        /// </summary>
        /// <param name="server">The <see cref="System.Net.IPEndPoint" /> of the game server.</param>
        /// <param name="rconPassword">The Rcon password for the game server.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="server" /> is <see langword="null" />.</exception>
        public GoldSrcServer(IPEndPoint server, String rconPassword)
            : base(server, rconPassword)
        {
            // Empty the challenge.
            this.RconChallenge = GoldSrcServer.EmptyChallenge;
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~GoldSrcServer()
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
                if (disposing)
                {
                    this.RconChallenge  = ValveServer.EmptyChallenge;
                }

                base.Dispose(disposing);
            }
            catch { }
        }

        #endregion

        #region Sockets methods

        /// <summary>
        /// Sends the given <paramref name="value" /> to the game server.
        /// </summary>
        /// <param name="value">The value to send to the game server.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        public void Send(Byte[] value)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            UdpClient client = null;

            try
            {
                client = new UdpClient();
                client.Send(value, value.Length, this.Server);
            }
            // If there is any socket exception, encapsulate it as an IOException and throw it.
            catch (SocketException exp)
            {
                throw new IOException("Error while trying to send to the server.", exp);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }

                client = null;
            }
        }

        /// <summary>
        /// Receives a splitted packet from the client.
        /// </summary>
        /// <param name="data">The packet containing the header.</param>
        /// <param name="client">The client to read the splitted packets from.</param>
        /// <returns>All of the data in the packets combined to one packet.</returns>
        protected override Byte[] ReceiveSplitPacket(Byte[] data, UdpClient client)
        {
            // Gets the total number of packets.
            int numPackets = (data[8] & 15);

            // Create a byte-array array to hold all the responses.
            var allResponses = new Byte[numPackets][];

            // Decrease the number of received packets (first packet was already received).
            numPackets--;

            // Receive all packets and set them in the correct order.
            do
            {
                // Get the current packet length without its header.
                int currPacketLength = data.Length - ValveServer.Queries.SourceSplitPacketsHeaderLength;

                // Gets the current packet number.
                int currPacketNum = (data[8] >> 4);

                // Sets the already received response in its correct place in the array.
                allResponses[currPacketNum] = new Byte[currPacketLength];
                Array.Copy(data, ValveServer.Queries.SourceSplitPacketsHeaderLength, allResponses[currPacketNum], 0, currPacketLength);

                // Receives the next packets if there should be one.
                if (numPackets > 0)
                {
                    // Create an endpoint to store the server's address.
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                    data = client.Receive(ref endPoint);
                }
            } while (numPackets-- > 0);

            // Create the final response by concating all the received data with the Type set first as the header (0xFFFFFFFE).
            return (Util.ConcatByteArrays(allResponses));
        }

        #endregion

        #region Rcon methods

        /// <summary>
        /// Connects the instance to the game server's Rcon.
        /// </summary>
        /// <returns>Whether the connection was successful or not.</returns>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public bool ConnectRcon()
        {
            try
            {
                this.ChallengeRcon();
                return (true);
            }
            catch (BadRconChallengeException)
            {
                return (false);
            }
        }

        /// <summary>
        /// Clears the Rcon challenge.
        /// </summary>
        public void DisconnectRcon()
        {
            this.RconChallenge  = GoldSrcServer.EmptyChallenge;
        }

        /// <summary>
        /// Requests the Rcon challenge number and sets it at <see cref="AServerQuery.GoldSrcServer.RconChallenge" />.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">Server has returned an empty Rcon challenge.</exception>
        public void ChallengeRcon()
        {
            this.ChallengeRcon(this.TimeOut);
        }

        /// <summary>
        /// Requests the Rcon challenge number and sets it at <see cref="AServerQuery.GoldSrcServer.RconChallenge" />.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">Server has returned an empty Rcon challenge.</exception>
        public void ChallengeRcon(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // Create the request challenge.
            var request     = Util.ConcatByteArrays(ValveServer.Queries.OnePacketHeader,
                                                        Util.ConvertToByteArray("challenge rcon"));

            // Get the response.
            var response    = Util.ConvertToString(this.Query(request, timeOut));

            // Extract the challenge using a regex expression.
            var challenge   = Regex.Match(response, "^....challenge rcon (\\d+)", RegexOptions.IgnoreCase);

            // If the response contains the challenge, set it.
            if (challenge.Success)
            {
                // Set the local Rcon challenge.
                long rconChallenge;

                // If the try parse didn't succeed, revert to empty rcon challenge.
                if (!long.TryParse(challenge.Groups[1].Value, out rconChallenge))
                {
                    rconChallenge   = ValveServer.EmptyChallenge;
                }

                this.RconChallenge  = rconChallenge;
            }
            // Else, if the response doesn't match a challenge rcon response, throw an exception.
            else
            {
                throw new BadRconChallengeException(response);
            }
        }

        /// <summary>
        /// Sends the game server an Rcon command. 
        /// </summary>
        /// <remarks>
        /// <para>This method ignores the response sent from the server after sending the <paramref name="value" />,
        /// which means if a wrong password is set - there won't be a reply or any exception be thrown.</para>
        /// <para>If you need a reply or exceptions such as <see cref="AServerQuery.BadRconPasswordException" />
        /// or <see cref="AServerQuery.BadRconChallengeException" /> to be thrown, use the
        /// <see cref="AServerQuery.GoldSrcServer.QueryRcon(String)" /> method.</para>
        /// </remarks>
        /// <param name="value">The Rcon command to send to the game server.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void SendRcon(String value)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            this.Send(
                Util.ConcatByteArrays(
                    ValveServer.Queries.OnePacketHeader,
                    Util.ConvertToByteArray(
                        String.Format("rcon {0} \"{1}\" {2}", this.RconChallenge, this.RconPassword, value))));
        }

        /// <summary>
        /// Sends the server an Rcon command and returns the game server's response.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <param name="value">The Rcon command to send to the game server.</param>
        /// <returns>The server's response.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public String QueryRcon(String value)
        {
            return (this.QueryRcon(value, this.TimeOut));
        }

        /// <summary>
        /// Sends the server an Rcon command and returns the game server's response.
        /// </summary>
        /// <param name="value">The Rcon command to send to the game server.</param>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>The server's response.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public String QueryRcon(String value, int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // Build the request.
            var request     = Util.ConcatByteArrays(
                                ValveServer.Queries.OnePacketHeader,
                                Util.ConvertToByteArray(
                                    String.Format("rcon {0} \"{1}\" {2}",
                                        this.RconChallenge,
                                        this.RconPassword,
                                        value)));

            // Query the request.
            var response    = Util.ConvertToString(this.Query(request, timeOut));

            // Check for errors.
            if ((!String.IsNullOrWhiteSpace(response)) && (response.Length > 5))
            {
                // If the response returned a bad challenge error, throw the error.
                if (response.Substring(5).StartsWith("bad challenge.", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BadRconChallengeException(response);
                }
                // Else, if the response returned an bad rcon password error.
                else if (response.Substring(5).StartsWith("bad rcon_password.", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BadRconPasswordException(response);
                }
            }

            return (response);
        }

        /// <summary>
        /// Determins whether the Rcon password is correct and valid.
        /// </summary>
        /// <remarks>
        /// <para>This method challenges the Rcon if it was still empty.</para>
        /// <para>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</para>
        /// </remarks>
        /// <returns><see langword="true" /> if the password is correct, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        public bool IsRconPasswordValid()
        {
            return (this.IsRconPasswordValid(this.TimeOut));
        }

        /// <summary>
        /// Determins whether the Rcon password is correct and valid.
        /// </summary>
        /// <remarks>This method challenges the Rcon if it was still empty.</remarks>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns><see langword="true" /> if the password is correct, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        public bool IsRconPasswordValid(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // Create a value to ask the server to echo it to validate a good password.
            var echoRequest = new Random().Next(10000, int.MaxValue).ToString();

            // If challenge was not set yet, challenge it.
            if (this.RconChallenge == ValveServer.EmptyChallenge)
            {
                this.ChallengeRcon();
            }

            try
            {
                // Query the server to echo a test message and store the response.
                var response = this.QueryRcon(String.Format("echo {0}", echoRequest), timeOut);

                // Return whether the test string was echo'd back.
                return (response.Contains(echoRequest));
            }
            // If the query resulted in a BadRconPasswordException - return false (wrong password).
            // Bubble other exceptions up.
            catch (BadRconPasswordException)
            {
                return (false);
            }
        }

        /// <summary>
        /// Queries the game server for the requested Cvar and returns its value.
        /// </summary>
        /// <remarks>
        /// <para>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</para>
        /// <para>
        /// If the cvar name was mistyped, the game server will not respond.
        /// It's a good practice to set a time-out for this request and not leaving it at infinite.
        /// </para>
        /// </remarks>
        /// <param name="value">The Cvar name to request its value from the game server.</param>
        /// <returns>The value of the requested Cvar.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public String GetCvar(String value)
        {
            return (this.GetCvar(value, this.TimeOut));
        }

        /// <summary>
        /// Queries the game server for the requested Cvar and returns its value.
        /// </summary>
        /// <remarks>
        /// If the cvar name was mistyped, the game server will not respond.
        /// It's a good practice to set a time-out for this request and not leaving it at infinite.
        /// </remarks>
        /// <param name="value">The Cvar name to request its value from the game server.</param>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>The value of the requested Cvar.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        /// <exception cref="System.FormatException">If the game server's response was invalid.</exception>
        public String GetCvar(String value, int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            var response    = this.QueryRcon(value, timeOut);

            // Extract the cvar value using a regex expression.
            var cvarValue   = Regex.Match(response, String.Format("\"{0}\" is \"([^\"]*)\"", value), RegexOptions.IgnoreCase);

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
        /// <para>This method ignores the response sent from the server after sending the Dgram to set the cvar,
        /// which means if a wrong password is set - there won't be a reply or any exception be thrown.</para>
        /// <para>If you need a reply or exceptions such as <see cref="AServerQuery.BadRconPasswordException" />
        /// or <see cref="AServerQuery.BadRconChallengeException" /> to be thrown, use the
        /// <see cref="AServerQuery.GoldSrcServer.QueryRcon(String)" /> method with a value of
        /// "<paramref name="cvar" /> \"<paramref name="value" />\"".</para>
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentNullException">The given <paramref name="cvar" /> name is null or empty.</exception>
        public void SetCvar(String cvar, String value)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // If the given cvar name is null or empty, throw an appropriate exception.
            if (String.IsNullOrWhiteSpace(cvar))
            {
                throw new ArgumentNullException("cvar");
            }

            this.SendRcon(String.Format("{0} \"{1}\"", cvar, value));
        }

        /// <summary>
        /// Determins whether the game server is logging or not.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns><see langword="true" /> if the game server is logging, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public bool IsLogging()
        {
            return (this.IsLogging(this.TimeOut));
        }

        /// <summary>
        /// Determins whether the game server is logging or not.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns><see langword="true" /> if the game server is logging, <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public bool IsLogging(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            var response = this.QueryRcon("log", timeOut);

            return (!response.Contains("not currently logging"));
        }

        /// <summary>
        /// Starts the log on the game server.
        /// </summary>
        /// <remarks>
        /// <para>This method ignores the response sent from the server after sending the Dgram to start the log,
        /// which means if a wrong password is set - there won't be a reply or any exception be thrown.</para>
        /// <para>If you need a reply or exceptions such as <see cref="AServerQuery.BadRconPasswordException" />
        /// or <see cref="AServerQuery.BadRconChallengeException" /> to be thrown, use the
        /// <see cref="AServerQuery.GoldSrcServer.QueryRcon(String)" /> method with a value of "log on".</para>
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void StartLog()
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            this.SendRcon("log on");
        }

        /// <summary>
        /// Stops the log on the game server.
        /// </summary>
        /// <remarks>
        /// <para>This method ignores the response sent from the server after sending the Dgram to start the log,
        /// which means if a wrong password is set - there won't be a reply or any exception be thrown.</para>
        /// <para>If you need a reply or exceptions such as <see cref="AServerQuery.BadRconPasswordException" />
        /// or <see cref="AServerQuery.BadRconChallengeException" /> to be thrown, use the
        /// <see cref="AServerQuery.GoldSrcServer.QueryRcon(String)" /> method with a value of "log off".</para>
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public void StopLog()
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            this.SendRcon("log off");
        }

        /// <summary>
        /// Queries the game server for the list of the current address list to which the server is logging to.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="System.Net.IPEndPoint" /> of the log addresses on the server.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public IEnumerable<IPEndPoint> GetLogAddresses()
        {
            return (this.GetLogAddresses(this.TimeOut));
        }

        /// <summary>
        /// Queries the game server for the list of the current address list to which the server is logging to.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerable{T}" /> of <see cref="System.Net.IPEndPoint" /> of the log addresses on the server.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public IEnumerable<IPEndPoint> GetLogAddresses(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            // Get the log addresses from the server.
            var response        = this.QueryRcon("logaddress_add", timeOut);

            // Separate all the addresses.
            var addressesMatch  = Regex.Matches(response, "current:\\s+((?:\\d{1,3}\\.){3}\\d{1,3}):(\\d{1,5})");

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
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to add to the log addresses.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        /// <exception cref="AServerQuery.UnableToResolveException">If the server was unable to resolve the address.</exception>
        /// <exception cref="AServerQuery.AddressAlreadyInListException">If the address is already in the logging list.</exception>
        /// <exception cref="AServerQuery.GameServerException">If the query responded with an unknown response.</exception>
        public void AddLogAddress(IPEndPoint value)
        {
            this.AddLogAddress(value, this.TimeOut);
        }

        /// <summary>
        /// Queries the game server to add the given <paramref name="value" /> to the logging addresses.
        /// </summary>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to add to the log addresses.</param>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        /// <exception cref="AServerQuery.UnableToResolveException">If the server was unable to resolve the address.</exception>
        /// <exception cref="AServerQuery.AddressAlreadyInListException">If the address is already in the logging list.</exception>
        /// <exception cref="AServerQuery.GameServerException">If the query responded with an unknown response.</exception>
        public void AddLogAddress(IPEndPoint value, int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            var response = this.QueryRcon(String.Format("logaddress_add {0} {1}", value.Address, value.Port), timeOut);

            // If the server was unable to resolve the address, throw the appropriate exception.
            if (response.Contains("unable to resolve"))
            {
                throw new UnableToResolveException(response);
            }
            // Else, if the address is already in the list, throw the appropriate exception.
            else if (response.Contains("address already in list"))
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
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to delete from the log addresses.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        /// <exception cref="AServerQuery.UnableToResolveException">If the server was unable to resolve the address.</exception>
        /// <exception cref="AServerQuery.NoAddressesAddedException">If the address list is empty.</exception>
        /// <exception cref="AServerQuery.AddressNotFoundException">If the address couldn't be found in the list.</exception>
        /// <exception cref="AServerQuery.GameServerException">If the query responded with an unknown response.</exception>
        public void DeleteLogAddress(IPEndPoint value)
        {
            this.DeleteLogAddress(value, this.TimeOut);
        }

        /// <summary>
        /// Queries the game server to delete the given <paramref name="value" /> from the logging addresses.
        /// </summary>
        /// <param name="value">The <see cref="System.Net.IPEndPoint" /> to delete from the log addresses.</param>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        /// <exception cref="AServerQuery.UnableToResolveException">If the server was unable to resolve the address.</exception>
        /// <exception cref="AServerQuery.NoAddressesAddedException">If the address list is empty.</exception>
        /// <exception cref="AServerQuery.AddressNotFoundException">If the address couldn't be found in the list.</exception>
        /// <exception cref="AServerQuery.GameServerException">If the query responded with an unknown response.</exception>
        public void DeleteLogAddress(IPEndPoint value, int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            var response = this.QueryRcon(String.Format("logaddress_del {0} {1}", value.Address, value.Port), timeOut);

            // If the server was unable to resolve the address, throw the appropriate exception.
            if (response.Contains("unable to resolve"))
            {
                throw new UnableToResolveException(response);
            }
            // Else, if the address list is empty, throw the appropriate exception.
            else if (response.Contains("No addresses added yet"))
            {
                throw new NoAddressesAddedException(response);
            }
            // Else, if the address couldn't be found in the list, throw the appropriate exception.
            else if (response.Contains("Couldn't find address in list"))
            {
                throw new AddressNotFoundException(response);
            }
            // Else, if the response did not respond with confirm deletion, throw the appropriate exception.
            else if (!response.Contains(String.Format("deleting:  {0}:{1}", value.Address, value.Port)))
            {
                throw new GameServerException(response);
            }
        }

        /// <summary>
        /// Queries the game server for the status.
        /// </summary>
        /// <remarks>The time-out used is the time-out which was set in <see cref="AServerQuery.ValveServer.TimeOut" />.</remarks>
        /// <returns>A <see cref="AServerQuery.StatusInfo" /> representing the game server's Rcon status.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public StatusInfo GetStatus()
        {
            return (this.GetStatus(this.TimeOut));
        }

        /// <summary>
        /// Queries the game server for the status.
        /// </summary>
        /// <param name="timeOut">Sets the receive time-out in milliseconds. 0 or -1 is infinite time-out period.</param>
        /// <returns>A <see cref="AServerQuery.StatusInfo" /> representing the game server's Rcon status.</returns>
        /// <exception cref="System.ObjectDisposedException">The <see cref="AServerQuery.GoldSrcServer" /> is disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeOut" /> is less than -1.</exception>
        /// <exception cref="AServerQuery.BadRconChallengeException">If the Rcon challenge is incorrect.</exception>
        /// <exception cref="AServerQuery.BadRconPasswordException">If the Rcon password is incorrect.</exception>
        public StatusInfo GetStatus(int timeOut)
        {
            // Validate that the instance is not disposed.
            this.CheckDisposed();

            return (StatusInfo.Parse(this.QueryRcon("status", timeOut)));
        }

        #endregion

        #endregion
    }
}