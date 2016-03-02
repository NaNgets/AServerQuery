// Util.cs is part of AServerQuery.
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
using System.Text;

namespace AServerQuery
{
    /// <summary>
    /// Utility class containing useful methods.
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Converts a given string to a representing byte array.
        /// </summary>
        /// <param name="value">The string to convert to byte array.</param>
        /// <returns>A byte array representing the given string.</returns>
        public static Byte[] ConvertToByteArray(String value)
        {
            return (Encoding.Default.GetBytes(value));
        }

        /// <summary>
        /// Converts a given byte array to a representing string.
        /// </summary>
        /// <param name="value">The byte array to convert to string.</param>
        /// <returns>A string representing the given byte array.</returns>
        public static String ConvertToString(Byte[] value)
        {
            return (Encoding.Default.GetString(value));
        }

        /// <summary>
        /// Concat given byte arrays to one byte array.
        /// </summary>
        /// <param name="arrbtValues">Byte arrays to concat one after another.</param>
        /// <returns>The concatinated byte array.</returns>
        public static Byte[] ConcatByteArrays(params Byte[][] arrbtValues)
        {
            // Create a value to represent the total byte count.
            var nLength = 0;

            // Calculate the total byte count.
            foreach (Byte[] arrbtCurr in arrbtValues)
            {
                nLength += arrbtCurr.Length;
            }

            // Create a byte array to hold the concatinated byte arrays.
            var arrbtDest = new Byte[nLength];

            // Create a value to point to the current index.
            var nIndex = 0;

            // Go over each given array and concat it to the end of the current byte array.
            foreach (var arrbtCurr in arrbtValues)
            {
                Array.Copy(arrbtCurr, 0, arrbtDest, nIndex, arrbtCurr.Length);

                // Increment the index.
                nIndex += arrbtCurr.Length;
            }

            return (arrbtDest);
        }

        /// <summary>
        /// Reads the string from the given value starting from the given offset.
        /// </summary>
        /// <param name="value">The value to read the string from.</param>
        /// <param name="offset">The starting offset to read the string from.</param>
        /// <returns>The string read from the value.</returns>
        public static String ReadString(Byte[] value, ref int offset)
        {
            // Create a new StringBuilder to hold the string.
            var sbString = new StringBuilder();

            // Go over the array until its end.
            for (; offset < value.Length; offset++)
            {
                // If the current byte is an empty byte (string's end), break.
                if (value[offset] == 0)
                {
                    offset++;
                    break;
                }

                // Append the current char to the string.
                sbString.Append((char)value[offset]);
            }

            return (sbString.ToString());
        }
    }
}