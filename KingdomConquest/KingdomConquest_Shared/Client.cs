using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace KingdomConquest_Shared
{
   public class Client
   {
      //-----------------------------------------------------------------------
      //START Variable Declaration
      //-----------------------------------------------------------------------

      private readonly char[] DELINEATOR = { '|' };
      private const int PORT_NUM = 5756;
      private const int BUF_SIZE = 2048;
      
      //string serverIP;

      private TcpClient clientConnection;
      private NetworkStream clientStream;
      //private Thread readFromStream;

      private Queue<string> commandQueue;
      bool isConnected;

      bool resendRandomUsername;
      bool resendFriendlyUsername;

      public enum MatchType
      {
         RANDOM,
         FRIEND
      }

      //-----------------------------------------------------------------------
      //END Variable Declaration
      //-----------------------------------------------------------------------

      /// <summary>
      /// Constructor.
      /// </summary>
      public Client()
      {
         //serverIP = "";
         commandQueue = new Queue<string>();
         //isConnected = true;
         isConnected = false;
         resendRandomUsername = false;
         resendFriendlyUsername = false;
         //readFromStream = new Thread(new ThreadStart(ReadFromStream));
         //readFromStream.Start();
      }

      /*
      private void ReadFromStream()
      {
         string[] messageArray;
         byte[] byteBuffer = new byte[BUF_SIZE];
         int size = 0;

         while (clientRunning)
         {
            if (clientStream.DataAvailable)
            {
               size = clientStream.Read(byteBuffer, 0, BUF_SIZE);
               messageArray = Decode(byteBuffer, size);
               EnqueueCommands(messageArray);

               if (messageArray.Contains<string>("DISCONNECT"))
               {
                  clientRunning = false;
                  clientStream.Close();
                  clientConnection.Close();
               }
            }
         }
      }
      */

      private void ReadFromStream()
      {
         string[] messageArray;
         byte[] byteBuffer = new byte[BUF_SIZE];
         int size = 0;

         try
         {
            WriteToStream("");

            if (clientStream.DataAvailable)
            {
               size = clientStream.Read(byteBuffer, 0, BUF_SIZE);
               messageArray = Decode(byteBuffer, size);
               EnqueueCommands(messageArray);
            }
         }
         catch
         {
            isConnected = false;
         }

      }

      private void WriteToStream(string messageToSend)
      {
         try
         {
            clientStream.Write(Encode(messageToSend), 0,
               Encoding.ASCII.GetByteCount(messageToSend));
         }
         catch
         {
            isConnected = false;
         }
      }

      public string SetConnectionType(string username)
      {
         resendFriendlyUsername = false;

         bool connecting = true;
         string messageToSend = "";
         string connectionResult = "";

         while (connecting)
         {
            ReadFromStream();

            if (!isConnected)
            {
               connectionResult = "DISCONNECT";
               clientStream.Close();
               clientConnection.Close();
            }
            else if (resendRandomUsername)
            {
               messageToSend = "USERNAME|" + username + "|";
               WriteToStream(messageToSend);
               resendRandomUsername = false;
            }
            else if (commandQueue.Count > 0)
            {
               string command = commandQueue.Dequeue();

               switch (command)
               {
                  case "REQUEST_USERNAME":
                     messageToSend = "USERNAME|" + username + "|";
                     WriteToStream(messageToSend);
                     //clientStream.Write(Encode(messageToSend), 0,
                     //   Encoding.ASCII.GetByteCount(messageToSend));
                     break;

                  case "REQUEST_MATCH_TYPE":
                     messageToSend = "MATCH_TYPE|RANDOM";
                     WriteToStream(messageToSend);
                     //clientStream.Write(Encode(messageToSend), 0,
                     //   Encoding.ASCII.GetByteCount(messageToSend));
                     break;

                  case "USERNAME_TAKEN":
                     connecting = false;
                     connectionResult = command;
                     resendRandomUsername = true;
                     break;

                  case "WAITING_RANDOM":
                     connecting = false;
                     connectionResult = command;
                     break;
               }
            }
         }

         return connectionResult;
      }

      public bool Connect(string serverIP)
      {
         try
         {
            clientConnection = new TcpClient(serverIP, PORT_NUM);
            clientStream = clientConnection.GetStream();
            isConnected = true;
            return isConnected;
         }
         catch
         {
            return isConnected;
         }
      }

      public bool IsConnected
      {
         get { return isConnected; }
      }

      public string SetConnectionType(string username, string friendUsername)
      {
         resendRandomUsername = false;

         bool connecting = true;
         string messageToSend = "";
         string connectionResult = "";

         while (connecting)
         {
            ReadFromStream();

            if (!isConnected)
            {
               connectionResult = "DISCONNECT";
               clientStream.Close();
               clientConnection.Close();
            }
            else if (resendFriendlyUsername)
            {
               messageToSend = "USERNAME|" + username + "|";
               WriteToStream(messageToSend);
               resendFriendlyUsername = false;
            }
            else if (commandQueue.Count > 0)
            {
               string command = commandQueue.Dequeue();

               switch (command)
               {
                  case "REQUEST_USERNAME":
                     messageToSend = "USERNAME|" + username + "|";
                     WriteToStream(messageToSend);
                     //clientStream.Write(Encode(messageToSend), 0,
                     //   Encoding.ASCII.GetByteCount(messageToSend));
                     break;

                  case "REQUEST_MATCH_TYPE":
                     messageToSend = "MATCH_TYPE|FRIEND|" + friendUsername + "|";
                     WriteToStream(messageToSend);
                     //clientStream.Write(Encode(messageToSend), 0,
                     //   Encoding.ASCII.GetByteCount(messageToSend));
                     break;

                  case "USERNAME_TAKEN":
                     connecting = false;
                     connectionResult = command;
                     resendFriendlyUsername = true;
                     break;

                  case "WAITING_FRIEND":
                     connecting = false;
                     connectionResult = command;
                     break;
               }
            }
         }

         return connectionResult;
      }

      public void SendInitializationData(string[] unitArray)
      {
         string messageToSend = "INITIALIZE|" + unitArray.Count().ToString() + "|";

         for (int i = 0; i < unitArray.Length; i++)
            messageToSend += (unitArray[i] + "|");

         WriteToStream(messageToSend);
         
         //clientStream.Write(Encode(messageToSend), 0,
         //   Encoding.ASCII.GetByteCount(messageToSend));
      }

      /// <summary>
      /// Checks the command queue for a message indicating an opponent has
      /// been found or that a connection interruption has occurred.
      /// </summary>
      /// <returns>A command indicating an opponent has been found or that a
      /// connection interruption has occurred, or an empty string if neither
      /// command is present.</returns>
      public string CheckForOpponent()
      {
         string command = "";

         ReadFromStream();

         if (!isConnected)
         {
            command = "DISCONNECT";
            clientStream.Close();
            clientConnection.Close();
         }

         else if (commandQueue.Count > 0)
         {
            command = commandQueue.Peek();
            if (command.Equals("OPPONENT_FOUND") || command.Equals("DISCONNECT"))
               commandQueue.Dequeue();
            else
               command = "";
         }

         return command;
      }

      public List<string> GetNextCommand()
      {
         string commandKeyword = "";
         List<string> command = new List<string>();

         ReadFromStream();

         if (!isConnected)
         {
            command.Add("DISCONNECT");
            clientStream.Close();
            clientConnection.Close();
         }

         else if (commandQueue.Count > 0)
         {
            commandKeyword = commandQueue.Dequeue();

            switch (commandKeyword)
            {
               case "INITIALIZE":
                  int numUnits = int.Parse(commandQueue.Dequeue());

                  command.Add(commandKeyword);
                  command.Add(numUnits.ToString());
                  for (int i = 0; i < numUnits; i++)
                     command.Add(commandQueue.Dequeue());
                  break;

               case "MOVE":
                  command.Add(commandKeyword);
                  command.Add(commandQueue.Dequeue()); //CurrentX
                  command.Add(commandQueue.Dequeue()); //CurrentY
                  command.Add(commandQueue.Dequeue()); //DesiredX
                  command.Add(commandQueue.Dequeue()); //DesiredY
                  break;

               case "ATTACK":
                  command.Add(commandKeyword);
                  command.Add(commandQueue.Dequeue()); //AttackerX
                  command.Add(commandQueue.Dequeue()); //AttackerY
                  command.Add(commandQueue.Dequeue()); //DefenderX
                  command.Add(commandQueue.Dequeue()); //DefenderY
                  command.Add(commandQueue.Dequeue()); //DefenderHPLoss
                  command.Add(commandQueue.Dequeue()); //AttackerHPLoss
                  break;

               case "LEVEL_UP":
                  command.Add(commandKeyword);
                  command.Add(commandQueue.Dequeue()); //UnitX
                  command.Add(commandQueue.Dequeue()); //UnitY
                  command.Add(commandQueue.Dequeue()); //UnitHealth
                  command.Add(commandQueue.Dequeue()); //UnitStrength
                  command.Add(commandQueue.Dequeue()); //UnitDexterity
                  command.Add(commandQueue.Dequeue()); //UnitPhysicalResistance
                  command.Add(commandQueue.Dequeue()); //UnitMagicResistance
                  break;

               case "TURN_END":
                  command.Add(commandKeyword);
                  break;

               case "OPPONENT_FOUND":
                  command.Add(commandKeyword);
                  command.Add(commandQueue.Dequeue());
                  break;

               case "DISCONNECT":
                  command.Add(commandKeyword);
                  clientStream.Close();
                  clientConnection.Close();
                  break;
            }
         }
         return command;
      }
      public void SendMove(string moveCommand)
      {
         string messageToSend = "MOVE|" + moveCommand + "|";

         WriteToStream(messageToSend);
         
         //clientStream.Write(Encode(messageToSend), 0,
         //   Encoding.ASCII.GetByteCount(messageToSend));
      }

      public void SendAttack(string attackCommand)
      {
         string messageToSend = "ATTACK|" + attackCommand + "|";

         WriteToStream(messageToSend);
         
         //clientStream.Write(Encode(messageToSend), 0,
         //   Encoding.ASCII.GetByteCount(messageToSend));
      }

      public void SendLevelUp(string levelUpCommand)
      {
         string messageToSend = "LEVEL_UP|" + levelUpCommand + "|";

         WriteToStream(messageToSend);

         //clientStream.Write(Encode(messageToSend), 0,
         //   Encoding.ASCII.GetByteCount(messageToSend));
      }

      public void SendTurnEnd()
      {
         string messageToSend = "TURN_END|";

         WriteToStream(messageToSend);
         
         //clientStream.Write(Encode(messageToSend), 0,
         //   Encoding.ASCII.GetByteCount(messageToSend));
      }

      public void Disconnect()
      {
         string messageToSend = "DISCONNECT|";

         WriteToStream(messageToSend);
         clientStream.Close();
         clientConnection.Close();

         isConnected = false;
      }

      public void Abort()
      {
         isConnected = false;
         clientStream.Close();
         clientConnection.Close();
      }

      /// <summary>
      /// Enqueues an array of strings into the client's command queue.
      /// </summary>
      /// <param name="message">An array of strings to be enqueued in
      /// the client's command queue.</param>
      private void EnqueueCommands(string[] message)
      {
         for (int i = 0; i < message.Length; i++)
            commandQueue.Enqueue(message[i]);
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
