// ModInfo.cs is part of AServerQuery.
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
    /// Represents a mod's info as responded from the server by an A2S_INFO query.
    /// </summary>
    /// <remarks>
    /// ModInfo only exists in an old GoldSrc response where Type == 'm' (0x6D)
    /// and only if the IsMod value is true.
    /// </remarks>
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries#ModInfo" />
    /// <seealso href="http://developer.valvesoftware.com/wiki/Server_queries#A2S_INFO" />
    public class ModInfo
    {
        #region Properties

        /// <summary>
        /// Gets the URL containing information about this mod.
        /// </summary>
        public String URLInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the URL to download this mod from.
        /// </summary>
        public String URLDL
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets the version of the installed mod.
        /// </summary>
        public int ModVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the download size of this mod.
        /// </summary>
        public int ModSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this mod is server side only or not.
        /// </summary>
        public bool SvOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this mod has custom client dll or not.
        /// </summary>
        public bool ClDLL
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the original given data which was parsed.
        /// </summary>
        public Byte[] Data
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Private default constructor to be used by the Parse commands.
        /// </summary>
        private ModInfo()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a mod's info to
        /// its <see cref="AServerQuery.ModInfo" /> equivalent. A return value indicates
        /// whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A byte array representing a mod's info to convert.</param>
        /// <param name="offset">The index from which to start reading the mod's info from.</param>
        /// <param name="info">
        /// When this method returns, contains the mod's info value equivalent to the byte array
        /// contained in <paramref name="value" />, if the conversion succeeded, or <see langword="null" /> if the
        /// conversion failed. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(Byte[] value, ref int offset, out ModInfo info)
        {
            try
            {
                info = ModInfo.Parse(value, ref offset);

                return (true);
            }
            catch
            {
                info = null;

                return (false);
            }
        }

        /// <summary>
        /// Converts the given <paramref name="value" /> representing a mod's info to
        /// its <see cref="AServerQuery.ModInfo" /> equivalent.
        /// </summary>
        /// <param name="value">A byte array representing a mod's info to convert.</param>
        /// <param name="offset">The index from which to start reading the mod's info from.</param>
        /// <returns>A mod's info represented by the given byte array.</returns>
        public static ModInfo Parse(Byte[] value, ref int offset)
        {
            var info        = new ModInfo();

            int original    = offset;

            info.URLInfo    = Util.ReadString(value, ref offset);
            info.URLDL      = Util.ReadString(value, ref offset);

            // Disregard the Nul byte (according to the standard should always be 0x00).
            offset++;

            info.ModVersion = BitConverter.ToInt32(value, offset);
            offset          += 4;
            info.ModSize    = BitConverter.ToInt32(value, offset);
            offset          += 4;

            info.SvOnly     = (value[offset++] == 0x01);
            info.ClDLL      = (value[offset++] == 0x01);

            // Copy the mod's info data to the original data property.
            info.Data       = new Byte[offset - original];
            Array.Copy(value, original, info.Data, 0, offset - original);

            return (info);
        }

        #endregion
    }
}