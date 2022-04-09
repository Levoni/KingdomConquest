using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace KingdomConquestServer
{
   public class Server
   {
      //-----------------------------------------------------------------------
      //START Variable Declaration
      //-----------------------------------------------------------------------
      private const int PORT_NUM = 5756;
      private const int SIMULTANEOUS_GAMES = 50;
      private const int PLAYERS_PER_GAME = 2;
      private const int BUF_SIZE = 2048;
      private const int CONNECTION_TIMEOUT = 100;

      string input;

      private TcpListener listener;

      private Thread listenerThread;
      private Thread inputThread;
      private Thread closeThread;

      bool serverRunning;
      bool acceptingConnections;

      int numConnections;

      private List<Player> unassignedPlayers;
      private List<Player> randomOpponentWaiters;
      private List<Player> friendOpponentWaiters;
      private List<GameConnection> currentGameConnections;

      private List<string> usernames;

      private bool connectionClosing;
      private Queue<Socket> socketsToClose;
      private Queue<NetworkStream> streamsToClose;

      //bool connectionDisconnect;
      //-----------------------------------------------------------------------
      //END Variable Declaration
      //-----------------------------------------------------------------------

      /// <summary>
      /// Constructor.
      /// </summary>
      public Server()
      {
         input = "";

         listener = new TcpListener(IPAddress.Any, PORT_NUM);

         listenerThread = new Thread(new ThreadStart(AcceptConnections));
         inputThread = new Thread(new ThreadStart(GetInput));
         closeThread = new Thread(new ThreadStart(DelayedConnectionClose));

         serverRunning = true;
         acceptingConnections = true;

         numConnections = 0;
         unassignedPlayers = new List<Player>();
         randomOpponentWaiters = new List<Player>();
         friendOpponentWaiters = new List<Player>();
         currentGameConnections = new List<GameConnection>();

         usernames = new List<string>();

         connectionClosing = false;
         socketsToClose = new Queue<Socket>();
         streamsToClose = new Queue<NetworkStream>();

         //connectionDisconnect = false;
      }
      
      public void Run()
      {
         Player newPlayer;

         Console.WriteLine("Kingdom Conquest server started.");
         Console.WriteLine();

         listenerThread.Start();
         inputThread.Start();

         Console.WriteLine("Now accepting connection requests...");
         Console.WriteLine();

         while (serverRunning)
         {
            for (int i = 0; i < unassignedPlayers.Count; i++)
            {
               newPlayer = unassignedPlayers[i];
               newPlayer.ReadFromStream();

               if (!newPlayer.IsConnected)
               {
                  if (!unassignedPlayers[i].Username.Equals(""))
                     usernames.Remove(unassignedPlayers[i].Username);
                  unassignedPlayers.RemoveAt(i);
                  --i;
               }
               else if (newPlayer.CommandsAvailable())
               {
                  if (PerformSetupCommand(newPlayer))
                  {
                     unassignedPlayers.RemoveAt(i);
                     --i;
                  }
               }
            }

            for (int i = 0; i < randomOpponentWaiters.Count; i++)
            {
               randomOpponentWaiters[i].ReadFromStream();

               if (!randomOpponentWaiters[i].IsConnected)
               {
                  EndSingleConnection(randomOpponentWaiters[i]);
                  randomOpponentWaiters.RemoveAt(i);
                  --i;
               }

               else if (randomOpponentWaiters[i].CommandsAvailable())
               {
                  PerformWaitingCommand(randomOpponentWaiters[i]);
                  --i; //For now, command received at this time is guaranteed to be a "DISCONNECT" command
               }
            }

            for (int i = 0; i < friendOpponentWaiters.Count; i++)
            {
               friendOpponentWaiters[i].ReadFromStream();

               if (!friendOpponentWaiters[i].IsConnected)
               {
                  EndSingleConnection(friendOpponentWaiters[i]);
                  friendOpponentWaiters.RemoveAt(i);
                  --i;
               }

               else if (friendOpponentWaiters[i].CommandsAvailable())
               {
                  PerformWaitingCommand(friendOpponentWaiters[i]);
                  --i; //For now, command received at this time is guaranteed to be a "DISCONNECT" command
               }
            }

            if (randomOpponentWaiters.Count >= PLAYERS_PER_GAME)
            {
               PairRandomOpponents();
            }

            for (int i = 0; i < currentGameConnections.Count; i++)
            {
               bool connectionClosed = false;

               currentGameConnections[i].P1ReadFromStream();
               currentGameConnections[i].P2ReadFromStream();

               if (!currentGameConnections[i].P1IsConnected)
               {
                  //DisconnectCommand(currentGameConnections[i].Player1,
                     //currentGameConnections[i].Player2);
                  EndGameConnection(i);
                  connectionClosed = true;
               }
               else if (!currentGameConnections[i].P2IsConnected)
               {
                  //DisconnectCommand(currentGameConnections[i].Player1,
                     //currentGameConnections[i].Player2);
                  EndGameConnection(i);
                  connectionClosed = true;
               }
               else
               {
                  if (currentGameConnections[i].P1CommandsAvailable)
                     PerformInGameCommand(currentGameConnections[i].Player1,
                        currentGameConnections[i].Player2);
                  if (currentGameConnections[i].P2CommandsAvailable)
                     PerformInGameCommand(currentGameConnections[i].Player2,
                        currentGameConnections[i].Player1);
               }

               if (connectionClosed)
               {
                  //Console.WriteLine("Closed.");
                  --i;
               }
            }
         }

         Console.WriteLine();
         Console.WriteLine("Kingdom Conquest server stopped.");
      }

      private void InitializeCommand(Player sendingPlayer,
         Player receivingPlayer)
      {
         int numUnits = int.Parse(sendingPlayer.GetCommand());

         string messageToSend = "INITIALIZE|" + numUnits + "|";
         for (int i = 0; i < numUnits; i++)
            messageToSend += (sendingPlayer.GetCommand() + "|");

         receivingPlayer.WriteToStream(messageToSend);

         Console.WriteLine("An INITIALIZE command was received from \"" +
            sendingPlayer.Username + "\" and sent to \"" +
            receivingPlayer.Username + "\". It contained " + numUnits +
            " unit(s).");
      }

      private void MoveCommand(Player sendingPlayer, Player receivingPlayer)
      {
         string currentX = sendingPlayer.GetCommand();
         string currentY = sendingPlayer.GetCommand();
         string desiredX = sendingPlayer.GetCommand();
         string desiredY = sendingPlayer.GetCommand();

         string messageToSend = "MOVE|" + currentX + "|" + currentY + "|" +
            desiredX + "|" + desiredY + "|";

         receivingPlayer.WriteToStream(messageToSend);

         Console.WriteLine("A MOVE command was received from \"" +
            sendingPlayer.Username + "\" and sent to \"" +
            receivingPlayer.Username + "\": [(" + currentX + ", " + currentY +
            ") -> (" + desiredX + ", " + desiredY + ")]");
      }

      private void AttackCommand(Player sendingPlayer, Player receivingPlayer)
      {
         string attackerX = sendingPlayer.GetCommand();
         string attackerY = sendingPlayer.GetCommand();
         string defenderX = sendingPlayer.GetCommand();
         string defenderY = sendingPlayer.GetCommand();
         string defenderHPLoss = sendingPlayer.GetCommand();
         string attackerHPLoss = sendingPlayer.GetCommand();

         string messageToSend = "ATTACK|" + attackerX + "|" + attackerY + "|" +
            defenderX + "|" + defenderY + "|" + defenderHPLoss + "|" +
            attackerHPLoss + "|";

         receivingPlayer.WriteToStream(messageToSend);

         Console.WriteLine("An ATTACK command was received from \"" +
            sendingPlayer.Username + "\" and sent to \"" +
            receivingPlayer.Username + "\": [A: (" + attackerX + ", " +
            attackerY + ") - " + attackerHPLoss + ", D: (" + defenderX + ", " +
            defenderY + ") - " + defenderHPLoss + "]");
      }

      private void LevelUpCommand(Player sendingPlayer, Player receivingPlayer)
      {
         string unitX = sendingPlayer.GetCommand();
         string unitY = sendingPlayer.GetCommand();
         string unitHealth = sendingPlayer.GetCommand();
         string unitStrength = sendingPlayer.GetCommand();
         string unitDexterity = sendingPlayer.GetCommand();
         string unitPhysicalResistance = sendingPlayer.GetCommand();
         string unitMagicResistance = sendingPlayer.GetCommand();

         string messageToSend = "LEVEL_UP|" + unitX + "|" + unitY + "|" +
            unitHealth + "|" + unitStrength + "|" + unitDexterity + "|" +
            unitPhysicalResistance + "|" + unitMagicResistance + "|";

         receivingPlayer.WriteToStream(messageToSend);

         Console.WriteLine("A LEVEL_UP command was received from \"" +
            sendingPlayer.Username + "\" and sent to \"" +
            receivingPlayer.Username + "\": (" + unitX + ", " + unitY +
            ") = (" + unitHealth + ", " + unitStrength + ", " + unitDexterity +
            ", " + unitPhysicalResistance + ", " + unitMagicResistance + ").");
      }

      private void EndTurnCommand(Player sendingPlayer, Player receivingPlayer)
      {
         string messageToSend = "TURN_END|";

         receivingPlayer.WriteToStream(messageToSend);

         Console.WriteLine("A TURN_END command was received from \"" +
            sendingPlayer.Username + "\" and sent to \"" +
            receivingPlayer.Username + "\".");
      }

      private void DisconnectCommand(Player sendingPlayer, Player receivingPlayer)
      {
         string relayMessage = "DISCONNECT|";

         receivingPlayer.WriteToStream(relayMessage);

         Console.WriteLine("A DISCONNECT command was received from \"" + 
            sendingPlayer.Username + "\" and sent to \"" +
            receivingPlayer.Username + "\".");
      }

      private void EndSingleConnection(Player player)
      {
         string playerIdentifier = "";
         Thread close = new Thread(new ThreadStart(DelayedConnectionClose));

         streamsToClose.Enqueue(player.Stream);
         socketsToClose.Enqueue(player.PlayerSocket);
         close.Start();

         if (player.Username.Equals(""))
            playerIdentifier =
               ((IPEndPoint)player.PlayerSocket.RemoteEndPoint).
               Address.ToString();
         else
         {
            playerIdentifier = player.Username;
            usernames.Remove(playerIdentifier);
         }

         Console.WriteLine("The connection to \"" + playerIdentifier +
            "\" was closed.");
      }

      private void EndGameConnection(int index)
      {
         Thread close1 = new Thread(new ThreadStart(DelayedConnectionClose));
         Thread close2 = new Thread(new ThreadStart(DelayedConnectionClose));

         streamsToClose.Enqueue(currentGameConnections[index].P1Stream);
         socketsToClose.Enqueue(currentGameConnections[index].P1Socket);
         close1.Start();

         streamsToClose.Enqueue(currentGameConnections[index].P2Stream);
         socketsToClose.Enqueue(currentGameConnections[index].P2Socket);
         close2.Start();

         Console.WriteLine("The connection between \"" +
                           currentGameConnections[index].P1Username +
                           "\" and \"" +
                           currentGameConnections[index].P2Username +
                           "\" was closed.");

         usernames.Remove(currentGameConnections[index].P1Username);
         usernames.Remove(currentGameConnections[index].P2Username);

         currentGameConnections.RemoveAt(index);
      }
      
      public void Abort()
      {
         serverRunning = false;
         listener.Stop();
         listenerThread.Abort();

         //Thread.Sleep(50);

         /*
         while (pendingUsername.Count > 0)
         {
            Player player = pendingUsername.Dequeue();
            player.Stream.Close();
            player.PlayerSocket.Close();
         }

         while(pendingMatchType.Count > 0)
         {
            Player player = pendingMatchType.Dequeue();
            player.Stream.Close();
            player.PlayerSocket.Close();
         }
         */

         for (int i = 0; i < unassignedPlayers.Count; i++)
         {
            Player player = unassignedPlayers[i];
            player.WriteToStream("DISCONNECT|");
            player.Stream.Close();
            player.PlayerSocket.Close();
         }

         for (int i = 0; i < randomOpponentWaiters.Count; i++)
         {
            Player player = randomOpponentWaiters[i];
            player.WriteToStream("DISCONNECT|");
            player.Stream.Close();
            player.PlayerSocket.Close();
         }

         /*
         while (randomOpponentWaiters.Count > 0)
         {
            Player player = randomOpponentWaiters.Dequeue();
            player.Stream.Close();
            player.PlayerSocket.Close();
         }
         */

         for (int i = 0; i < friendOpponentWaiters.Count; i++)
         {
            Player player = friendOpponentWaiters[i];
            player.WriteToStream("DISCONNECT|");
            player.Stream.Close();
            player.PlayerSocket.Close();
         }

         for (int i = 0; i < currentGameConnections.Count; i++)
         {
            GameConnection gameConnection = currentGameConnections[i];
            gameConnection.P1WriteToStream("DISCONNECT|");
            gameConnection.P1Stream.Close();
            gameConnection.P1Socket.Close();
            gameConnection.P2WriteToStream("DISCONNECT|");
            gameConnection.P2Stream.Close();
            gameConnection.P2Socket.Close();
         }
      }
      
      private void AcceptConnections()
      {
         Socket newSocket;
         NetworkStream newStream;

         while (true)
         {
            try
            {
               listener.Start();

               newSocket = listener.AcceptSocket();
               newStream = new NetworkStream(newSocket);

               Player newPlayer = new Player(newSocket, newStream);

               if (acceptingConnections)
               {
                  ++numConnections;

                  newPlayer.WriteToStream("REQUEST_USERNAME|");

                  Console.WriteLine("A connection request from a client at " +
                     ((IPEndPoint)newSocket.RemoteEndPoint).Address.ToString() +
                     " was accepted, and a username request was sent.");

                  unassignedPlayers.Add(newPlayer);

                  if (numConnections >= SIMULTANEOUS_GAMES * PLAYERS_PER_GAME)
                  {
                     acceptingConnections = false;

                     Console.WriteLine("The server has reached its connection " +
                                       "capacity and has stopped accepting " +
                                       "connection requests.");
                  }
               }
               else
               {
                  string ipAddress =
                     ((IPEndPoint)newSocket.RemoteEndPoint).Address.ToString();

                  newPlayer.WriteToStream("SERVER_FULL|");

                  socketsToClose.Enqueue(newSocket);
                  streamsToClose.Enqueue(newStream);
                  closeThread.Start();

                  Console.WriteLine("A connection request from " + ipAddress +
                                    " was denied because the server was full.");
               }
            }
            catch (SocketException)
            {

            }
         }
      }

      private void GetInput()
      {
         while (serverRunning)
         {
            input = Console.ReadLine();
            if (input.Equals("STOP"))
               Abort();
         }
      }

      /// <summary>
      /// Closes a stream/socket pair with a specified delay, allowing final
      /// messages sent to be read on the receiving end of the connection.
      /// </summary>
      private void DelayedConnectionClose()
      {
         while(connectionClosing)
         {
            ;
         }

         connectionClosing = true;

         NetworkStream terminatedStream = streamsToClose.Dequeue();
         Socket terminatedSocket = socketsToClose.Dequeue();

         try
         {
            terminatedStream.Close(CONNECTION_TIMEOUT);
            terminatedSocket.Close(CONNECTION_TIMEOUT);
            //Console.WriteLine("Closed.");
         }
         catch (SocketException)
         {
            ;
         }

         --numConnections;

         connectionClosing = false;
      }

      /// <summary>
      /// Encodes a string into a byte array that can be sent through a network
      /// stream.
      /// </summary>
      /// <param name="message">Message to encode, as a string</param>
      /// <returns>A byte array representing a string.</returns>
      private byte[] Encode(string message)
      {
         byte[] byteBuffer = new byte[BUF_SIZE];
         byteBuffer = Encoding.ASCII.GetBytes(message);
         return byteBuffer;
      }

      /// <summary>
      /// Decodes a byte array into a string.
      /// </summary>
      /// <param name="byteMessage">The byte array to decode</param>
      /// <param name="size">The number of elements in the byte array before
      /// the end of the message</param>
      /// <returns>An array of strings representing each command or data member
      /// sent through the network stream</returns>
      private string[] Decode(byte[] byteMessage, int size)
      {
         string stringMessage = Encoding.ASCII.GetString(byteMessage, 0, size);

         return stringMessage.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
      }

      private bool PerformSetupCommand(Player player)
      {
         bool finishedSetup = false;
         string commandKeyword = player.GetCommand();

         switch(commandKeyword)
         {
            case "USERNAME":
               string newUsername = player.GetCommand();
               if (!usernames.Contains(newUsername))
               {
                  player.Username = newUsername;
                  usernames.Add(newUsername);

                  Console.WriteLine("The client at " +
                     ((IPEndPoint)player.PlayerSocket.RemoteEndPoint).
                     Address.ToString() + " has set its username to \"" +
                     player.Username + "\".");

                  player.WriteToStream("REQUEST_MATCH_TYPE|");

                  Console.WriteLine("A match type request was sent to \"" +
                     player.Username + "\".");
               }
               else
               {
                  player.WriteToStream("USERNAME_TAKEN|");

                  Console.WriteLine("The username \"" + newUsername +
                     "\" from the client at " +
                     ((IPEndPoint)player.PlayerSocket.RemoteEndPoint).
                     Address.ToString() + " was rejected because it's " +
                     "already being used. A notification was sent.");
               }
               break;

            case "MATCH_TYPE":
               string matchType = player.GetCommand();
               if (matchType.Equals("RANDOM"))
               {
                  player.MatchType = Player.MATCH_TYPE.RANDOM;
                  randomOpponentWaiters.Add(player);
                  player.WriteToStream("WAITING_RANDOM|");

                  Console.WriteLine("\"" + player.Username +
                     "\" is now waiting for a random opponent.");
               }
               else
               {
                  player.MatchType = Player.MATCH_TYPE.FRIEND;
                  player.DesiredTeammate = player.GetCommand();
                  player.WriteToStream("WAITING_FRIEND|");

                  Console.WriteLine("\"" + player.Username +
                     "\" is now waiting for a friend with username \"" +
                     player.DesiredTeammate + "\".");

                  AddFriendOpponentWaiter(player);
               }

               finishedSetup = true;
               break;

            case "DISCONNECT":
               unassignedPlayers.Remove(player);
               EndSingleConnection(player);
               break;
         }
         return finishedSetup;
      }

      /// <summary>
      /// If the desired opponent is already waiting, pairs the specified
      /// player with the desired opponent in a new game connection;
      /// if the desired opponent has not yet arrived, adds the specified
      /// player to the friendly opponent waiting list.
      /// </summary>
      /// <param name="playerToAdd">The player to add to the friendly opponent
      /// waiting list.</param>
      private void AddFriendOpponentWaiter(Player playerToAdd)
      {
         int searchIndex = 0;
         while (searchIndex < friendOpponentWaiters.Count &&
            !friendOpponentWaiters[searchIndex].DesiredTeammate.Equals(
            playerToAdd.Username))
         {
            ++searchIndex;
         }

         if (searchIndex != friendOpponentWaiters.Count)
         {
            GameConnection newGameConnection = 
               new GameConnection(friendOpponentWaiters[searchIndex],
               playerToAdd);
            currentGameConnections.Add(newGameConnection);

            newGameConnection.P1WriteToStream("OPPONENT_FOUND|0|");
            newGameConnection.P2WriteToStream("OPPONENT_FOUND|1|");

            Console.WriteLine("\"" + 
               friendOpponentWaiters[searchIndex].Username + "\" and \"" + 
               playerToAdd.Username + 
               "\" have been paired in a friend match.");

            friendOpponentWaiters.RemoveAt(searchIndex);
         }
         else
            friendOpponentWaiters.Add(playerToAdd);
      }

      /// <summary>
      /// Pairs two players waiting for random opponents and adds them to a new
      /// game connection.
      /// </summary>
      private void PairRandomOpponents()
      {
         Player player1 = randomOpponentWaiters[0];
         Player player2 = randomOpponentWaiters[1];

         randomOpponentWaiters.RemoveAt(0);
         randomOpponentWaiters.RemoveAt(0);

         currentGameConnections.Add(new GameConnection(player1, player2));

         player1.WriteToStream("OPPONENT_FOUND|0|");
         player2.WriteToStream("OPPONENT_FOUND|1|");

         Console.WriteLine("\"" + player1.Username + "\" and \"" +
            player2.Username + "\" have been paired in a random match.");
      }

      private void PerformWaitingCommand(Player player)
      {
         string commandKeyword = player.GetCommand();

         if (commandKeyword.Equals("DISCONNECT"))
         {
            if (player.MatchType == Player.MATCH_TYPE.RANDOM)
               randomOpponentWaiters.Remove(player);
            else
               friendOpponentWaiters.Remove(player);
            EndSingleConnection(player);
         }
      }

      private void PerformInGameCommand(Player sendingPlayer, Player receivingPlayer)
      {
         string commandKeyword = sendingPlayer.GetCommand();

         switch(commandKeyword)
         {
            case "INITIALIZE":
               InitializeCommand(sendingPlayer, receivingPlayer);
               break;

            case "MOVE":
               MoveCommand(sendingPlayer, receivingPlayer);
               break;

            case "ATTACK":
               AttackCommand(sendingPlayer, receivingPlayer);
               break;

            case "LEVEL_UP":
               LevelUpCommand(sendingPlayer, receivingPlayer);
               break;

            case "TURN_END":
               EndTurnCommand(sendingPlayer, receivingPlayer);
               break;

            case "DISCONNECT":
               DisconnectCommand(sendingPlayer, receivingPlayer);
               sendingPlayer.IsConnected = false;
               break;
         }
      }
   }
}