using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace KingdomConquestServer
{
   class GameConnection
   {
      Player player1;
      Player player2;

      Queue<string> player1Commands;
      Queue<string> player2Commands;

      public GameConnection(Player player1, Player player2)
      {
         this.player1 = player1;
         this.player2 = player2;

         player1Commands = new Queue<string>();
         player2Commands = new Queue<string>();
      }

      /*
      public byte[] p1Read()
      {
         return player1.Read();
      }

      public byte[] p2Read()
      {
         return player2.Read();
      }
      */

      /*
      public void p1Write(byte[] byteBuffer, int size)
      {
         player1.Write(byteBuffer, size);
      }

      public void p2Write(byte[] byteBuffer, int size)
      {
         player2.Write(byteBuffer, size);
      }
      */

      public void StoreP1Command(string command)
      {
         player1Commands.Enqueue(command);
      }

      public void StoreP2Command(string command)
      {
         player2Commands.Enqueue(command);
      }

      public string GetP1Command()
      {
         if (player1Commands.Count >= 1)
            return player1Commands.Dequeue();
         return "";
      }

      public string GetP2Command()
      {
         if (player2Commands.Count >= 1)
            return player2Commands.Dequeue();
         return "";
      }

      public void P1ReadFromStream()
      {
         player1.ReadFromStream();
      }

      public void P2ReadFromStream()
      {
         player2.ReadFromStream();
      }

      public bool P1IsConnected
      {
         get { return player1.IsConnected; }
      }

      public bool P2IsConnected
      {
         get { return player2.IsConnected; }
      }

      public Player Player1
      {
         get { return player1; }
      }

      public Player Player2
      {
         get { return player2; }
      }

      public bool P1CommandsAvailable
      {
         get { return player1.CommandsAvailable(); }
      }

      public bool P2CommandsAvailable
      {
         get { return player2.CommandsAvailable(); }
      }

      public void P1WriteToStream(string messageToSend)
      {
         player1.WriteToStream(messageToSend);
      }

      public void P2WriteToStream(string messageToSend)
      {
         player2.WriteToStream(messageToSend);
      }
      public Socket P1Socket
      {
         get { return player1.PlayerSocket; }
         set { player1.PlayerSocket = value; }
      }

      public Socket P2Socket
      {
         get { return player2.PlayerSocket; }
         set { player2.PlayerSocket = value; }
      }

      public NetworkStream P1Stream
      {
         get { return player1.Stream; }
         set { player1.Stream = value; }
      }

      public NetworkStream P2Stream
      {
         get { return player2.Stream; }
         set { player2.Stream = value; }
      }

      public string P1Username
      {
         get { return player1.Username; }
      }

      public string P2Username
      {
         get { return player2.Username; }
      }
   }
}
