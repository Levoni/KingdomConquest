using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace KingdomConquest_Desktop
{
   class Client
   {
      //-----------------------------------------------------------------------
      //START Variable Declaration
      //-----------------------------------------------------------------------

      private readonly char[] DELINEATOR = { '|' };

      //-----------------------------------------------------------------------
      //END Variable Declaration
      //-----------------------------------------------------------------------

      /// <summary>
      /// Constructor.
      /// </summary>
      public Client()
      {

      }

      /// <summary>
      /// Encodes a string into an array of bytes.
      /// </summary>
      /// <param name="message">A string to encode.</param>
      /// <returns>An array of bytes representing the encoded string.</returns>
      private byte[] Encode(string message)
      {
         byte[] tempByteBuffer = Encoding.ASCII.GetBytes(message);

         return tempByteBuffer;
      }

      /// <summary>
      /// Decodes an array of bytes into a string array where each array
      /// element represents an individual command received from the server.
      /// </summary>
      /// <param name="byteBuffer">An array of bytes to decode.</param>
      /// <param name="size">The number of bytes to decode.</param>
      /// <returns>An array of strings representing individual
      /// commands from the server.</returns>
      private string[] Decode(byte[] byteBuffer, int size)
      {
         string decodedMessage = Encoding.ASCII.GetString(byteBuffer, 0, size);
         string[] separatedCommands = decodedMessage.Split(DELINEATOR,
            StringSplitOptions.RemoveEmptyEntries);

         return separatedCommands;
      }
   }
}
