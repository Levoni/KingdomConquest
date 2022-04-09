using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace KingdomConquestServer
{
   class Player
   {
      //-----------------------------------------------------------------------
      //START Variable Declaration
      //-----------------------------------------------------------------------
      private string username;
      private string desiredTeammate;
      private MATCH_TYPE matchType;
      private Socket socket;
      private NetworkStream stream;

      private Queue<string> commandQueue;

      bool isConnected;

      private const int BUF_SIZE = 2048;

      public enum MATCH_TYPE { RANDOM, FRIEND }
      //-----------------------------------------------------------------------
      //END Variable Declaration
      //-----------------------------------------------------------------------

      /// <summary>
      /// Default constructor.
      /// </summary>
      public Player()
      {
         username = "";
         matchType = MATCH_TYPE.RANDOM;
         commandQueue = new Queue<string>();
         isConnected = true;
      }

      public Player(Socket socket, NetworkStream stream)
      {
         this.socket = socket;
         this.stream = stream;
         commandQueue = new Queue<string>();
         isConnected = true;
      }
      
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="username">The player's username</param>
      /// <param name="matchType">The type of match the player would like to
      /// play (either RANDOM or FRIEND)</param>
      /// <param name="stream">Network stream that connects the server to the
      /// client the player represents</param>
      public Player(string username, MATCH_TYPE matchType, NetworkStream stream)
      {
         this.username = username;
         this.matchType = matchType;
         this.stream = stream;
         commandQueue = new Queue<string>();
         isConnected = true;
      }

      public void ReadFromStream()
      {
         string[] messageArray;
         byte[] byteBuffer = new byte[BUF_SIZE];
         int size = 0;

         try
         {
            WriteToStream("");

            if (stream.DataAvailable)
            {
               size = stream.Read(byteBuffer, 0, BUF_SIZE);

               messageArray = Decode(byteBuffer, size);
               for (int i = 0; i < messageArray.Length; i++)
                  commandQueue.Enqueue(messageArray[i]);

               /*
               IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
               TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(socket.LocalEndPoint) && x.RemoteEndPoint.Equals(socket.RemoteEndPoint)).ToArray();
               if (tcpConnections != null && tcpConnections.Length > 0)
               {
                  TcpState stateOfConnection = tcpConnections.First().State;
                  if (stateOfConnection != TcpState.Closed)
                     Console.WriteLine("Closed");
               }
               */
            }
         }
         catch //(ObjectDisposedException)
         {
            //Console.WriteLine("ReadException.");
            isConnected = false;
         }
      }

      public void WriteToStream(string messageToSend)
      {
         try
         {
            stream.Write(Encode(messageToSend), 0,
               Encoding.ASCII.GetByteCount(messageToSend));
         }
         catch //(ObjectDisposedException)
         {
            //Console.WriteLine("WriteException.");
            isConnected = false;
         }
      }

      public bool CommandsAvailable()
      {
         return commandQueue.Count > 0;
      }

      public string GetCommand()
      {
         return commandQueue.Dequeue();
      }

      /*
      public void Write(byte[] byteBuffer, int size)
      {
         stream.Write(byteBuffer, 0, size);
      }
      */

      public bool IsConnected
      {
         get { return isConnected; }
         set { isConnected = value; }
      }

      public string Username
      {
         get { return username; }
         set { username = value; }
      }
      public MATCH_TYPE MatchType
      {
         get { return matchType; }
         set { matchType = value; }
      }

      public Socket PlayerSocket
      {
         get { return socket; }
         set { socket = value; }
      }
      public NetworkStream Stream
      {
         get { return stream; }
         set { stream = value; }
      }

      public string DesiredTeammate
      {
         get { return desiredTeammate; }
         set { desiredTeammate = value; }
      }



      private byte[] Encode(string message)
      {
         byte[] byteBuffer = new byte[BUF_SIZE];
         byteBuffer = Encoding.ASCII.GetBytes(message);
         return byteBuffer;
      }

      private string[] Decode(byte[] byteMessage, int size)
      {
         string stringMessage = Encoding.ASCII.GetString(byteMessage, 0, size);

         return stringMessage.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
      }
   }
}
