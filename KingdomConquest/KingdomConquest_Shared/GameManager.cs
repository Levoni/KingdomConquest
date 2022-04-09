using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
   public class GameManager
   {
      private MapManager MM;
      public Army MainArmy { get; private set; }
      private Client client;
      private NameGenerator ng;
      public GameState GS { get; set; }
      public List<Character> BattleList { get; private set; }
      public List<Character> stubEnemyList { get; private set; }
      FileIO File = new FileIO();

      private string[] attackInformation;

      /// <summary>
      /// Defalut constructor
      /// </summary>
      public GameManager(string savePath = null)
      {
         GS = GameState.MainMenu;
         client = null;
         BattleList = new List<Character>();
         stubEnemyList = new List<Character>();
         ng = new NameGenerator();
         if(savePath != null)
         {
            File.SetFilePath(savePath);
         }
         if (File.IsArmySaved())
         {
            MainArmy = File.LoadArmy();
         }
         else
         {
            MainArmy = new Army();

            MainArmy.AddChar(ng.GenerateName(), 0);
            MainArmy.AddChar(ng.GenerateName(), 1);
            MainArmy.AddChar(ng.GenerateName(), 2);
            File.SaveArmy(MainArmy.GetSaveDataString());
         }
      }

      public Character GetAttackedChar()
      {
         Location l = MM.GetTargetsLocation();
         return MM.GetCharOnTile(l.X, l.Y);
      }

      public bool GetHitOrMiss()
      {
         return MM.TargetHit;
      }


      /// <summary>
      /// Determines what click function to use when map
      /// is clicked
      /// </summary>
      /// <param name="x">x number tile</param>
      /// <param name="y">y number tile</param>
      public string TileClick(int x, int y)
      {
         switch (GS)
         {
            case GameState.MainMenu:
               return "";
            case GameState.Multiplayer:
               return MultiplyaerClick(x, y);
            case GameState.MultiplayerMenu:
               return "";
            case GameState.Singleplayer:
               return SinglePlayerClick(x, y);
            default:
               return "invalid";
         }
      }

      public string EnemyAction()
      {
         switch (GS)
         {
            case GameState.MainMenu:
               return "";
            case GameState.Multiplayer:
               return MultiplayerEnemyAction();
            case GameState.MultiplayerMenu:
               return "";
            case GameState.Singleplayer:
               return SinglePlayerEnemyAction();
            default:
               return "invalid";
         }
      }

      public string ExecuteAttack()
      {
         switch (GS)
         {
            case GameState.MainMenu:
               return "";
            case GameState.MultiplayerMenu:
               return "";
            case GameState.Multiplayer:
               return ExecuteMultiplayerAttack();
            case GameState.Singleplayer:
               return ExecuteSinglePlayerAttack();
            default:
               return "invalid";
         }
      }
      /// <summary>
      /// Runs the click event on map for the 
      /// single player case
      /// </summary>
      /// <param name="x">x number tile</param>
      /// <param name="y">y number tile</param>
      public string SinglePlayerClick(int x, int y)
      {
         string gameEvent = MM.MapClick(x, y);
         if (gameEvent == "initialized")
            File.SaveBattle(MM.GetBattleSaveData());
         return gameEvent;
      }

      /// <summary>
      /// Runs the click event on map for the 
      /// multiplayer case
      /// </summary>
      /// <param name="x">x number tile</param>
      /// <param name="y">y number tile</param>
      public string MultiplyaerClick(int x, int y)
      {
         string gameEvent = MM.MapClick(x, y);
         if (gameEvent == "attack" || gameEvent == "move")
         {
            //send move message to the server
            List<Location> path = MM.path;
            if (path.Count > 1)
            {
               string msg = path[0].X.ToString() + "|" + path[0].Y.ToString()
                  + "|" + path[path.Count - 1].X.ToString() + "|" + path[path.Count - 1].Y.ToString();
               client.SendMove(msg);
               //send movement message
            }
         }
         else if (gameEvent == "turn_over")
         {
            client.SendTurnEnd();
            gameEvent = "deselect";
         }
         else if (gameEvent == "initialized")
         {
            string[] msg = new string[BattleList.Count];
            int index = 0;
            foreach (Character c in BattleList)
            {
               msg[index] += c.ToString();
               ++index;
            }
            //send info to networking class
            client.SendInitializationData(msg);
            MM.IncrimentTurn();
            MM.setState((int)MapState.standby); //to do: find corect state or make one that is for waiting for a command
         }
         return gameEvent;
      }

      /// <summary>
      /// Creates map based on the arguments sent in
      /// </summary>
      public void StartSinglePlayerFight(string mapName = "sampleMap")
      {
         stubEnemyList = new List<Character>();
         /*
         AddToBattleList("one");
         AddToBattleList("two");
         AddToBattleList("three");
         */
         foreach (Character tempChar in BattleList)
         {
            tempChar.SetPosition(-1, -1);
            tempChar.isAbleToAttack = true;
            tempChar.isAbleToMove = true;
            tempChar.isAlive = true;
            tempChar.ResetHealth();
         }

         MM = new MapManager(BattleList, mapName);
         MM.HighlightStartTiles();
      }

      /// <summary>
      /// Creates map based on the arguments sent in
      /// </summary>
      public void StartMultiplayerFight(int team)
      {

         foreach (Character tempChar in BattleList)
         {
            tempChar.SetPosition(-1, -1);
            tempChar.isAbleToAttack = true;
            tempChar.isAbleToMove = true;
            tempChar.isAlive = true;
            tempChar.ResetHealth();
         }
         List<Character> temp = new List<Character>();
         MM = new MapManager(BattleList, temp, "sampleMap");
         MM.team = team;
         MM.HighlightStartTiles();
      }

      public void GetSelectedCharLocation(out int x, out int y)
      {
         MM.GetSelectedCharLocation(out x, out y);
      }

      public bool LoadFight()
      {
         if (File.IsBattleSaved())
         {
            MM = File.LoadBattle();
            return true;
         }
         return false;
      }

      /// <summary>
      /// Adds a character from the army to the active
      /// battle list
      /// </summary>
      /// <param name="charToAdd">Name of character to add</param>
      /// <returns>true = character added, false = character not added</returns>
      public bool AddToBattleList(string charToAdd)
      {
         Character c = MainArmy.FindChar(charToAdd);
         if (BattleList.Find(test => test.Equals(c)) == null)
         {
            BattleList.Add(c);
            return true;
         }
         return false;
      }

      /// <summary>
      /// removes a character from the active battle list
      /// </summary>
      /// <param name="charToRemove">name of character to remove</param>
      /// <returns>true = character removed, false = character not removed</returns
      public bool removeFromBattleList(string charToRemove)
      {
         Character c = MainArmy.FindChar(charToRemove);
         if (BattleList.Find(test => test.Equals(c)) != null)
         {
            BattleList.Remove(c);
            return true;
         }
         return false;
      }

      public Character GetCharOnTile(int x, int y)
      {
         return MM.GetCharOnTile(x, y);
      }

      public Tile GetTile(int x, int y)
      {
         return MM.GetTile(x, y);
      }

      //temp solution
      public List<Character> GetMMAllyArmy()
      {
         return MM.allies;
      }

      //temp solution
      public List<Character> GetMMenemyArmy()
      {
         return MM.enemies;
      }

      public bool IsBattleSave()
      {
         return File.IsBattleSaved();
      }

      //temp solution
      public List<Location> GetPath()
      {
         return MM.path;
      }

      public void MoveCharToNextPathLocation(Character c, List<Location> path)
      {
         MM.IncrementPathLocation(c, path);
      }

      public void SetMapManagerToStanby()
      {
         MM.setState(1);
         File.SaveBattle(MM.GetBattleSaveData());
      }

      public string SetUpMultiplayerGame(string IP, string username, string opponentName)
      {
         List<string> tempArray = new List<string>();
         string serverEvent = string.Empty;
         if (GS == GameState.MultiplayerMenu)
         {
            //create connection to server using networking class
            if (client == null)
               client = new Client();
            if (!client.IsConnected && !client.Connect(IP))
               serverEvent = "INVALID_IP";
            else if (opponentName == "")
               serverEvent = client.SetConnectionType(username);
            else
               serverEvent = client.SetConnectionType(username, opponentName);
         }
         else
         {
            tempArray = client.GetNextCommand();
            if (tempArray.Count > 0)
               serverEvent = tempArray[0]; //get command from server
         }

         switch (serverEvent)
         {
            case "INVALID_IP":
               ;
               break;
            case "DISCONNECT":
               GS = GameState.MainMenu;
               break;
            case "server_full":
               ;
               break;
            case "request_username":
               ;//send the username to server
               break;
            case "username_taken":
               ;
               break;
            case "request_match_type":
               ;//send the match type to server
               break;
            case "WAITING_RANDOM":
               GS = GameState.serverLobby;
               break;
            case "WAITING_FRIEND":
               GS = GameState.serverLobby;
               break;
            case "OPPONENT_FOUND":
               GS = GameState.Multiplayer;
               StartMultiplayerFight(int.Parse(tempArray[1]));
               break;
            case "game_start":
               GS = GameState.Multiplayer;
               MM.setState((int)MapState.initializing);
               break;
            default:
               ;
               break;
         }



         return serverEvent;
      }

      private string SinglePlayerEnemyAction()
      {
         string gameEvent = MM.EnemyAction();
         if (gameEvent == "turn change")
            File.SaveBattle(MM.GetBattleSaveData());
         return gameEvent;
      }

      private string MultiplayerEnemyAction()
      {
         if (MM.GetMapState() != MapState.movingCharacter && MM.GetMapState() != MapState.attackingCharacter)
         {
            List<string> command = client.GetNextCommand();
            string gameEvent = string.Empty;
            //get next command from Networking class
            if (command.Count > 0)
            {
               gameEvent = command[0];
               switch (command[0])
               {
                  case "MOVE":
                     MM.SelectUnitOnTile(int.Parse(command[1]), int.Parse(command[2]));
                     MM.SetCharPosition(MM.GetCharOnTile(int.Parse(command[1]), int.Parse(command[2])),
                                                         int.Parse(command[3]), int.Parse(command[4]));
                     break;
                  case "ATTACK":
                     MM.SelectUnitOnTile(int.Parse(command[1]), int.Parse(command[2]));
                     MM.AttackUnit(int.Parse(command[3]), int.Parse(command[4]));//execute attack action in map manager
                     attackInformation = command.ToArray();
                     break;
                  case "TURN_END":
                     MM.IncrimentTurn();
                     break;
                  case "INITIALIZE":
                     command.RemoveAt(0);
                     command.RemoveAt(0);
                     string[] tempArray = new string[command.Count];
                     for (int i = 0; i < command.Count; i++)
                     {
                        tempArray[i] = command[i];
                     }
                     MM.SetEnemyArmy(File.readCharList(tempArray));
                     if (MM.team == 0)
                        MM.IncrimentTurn();
                     break;
                  case "DISCONNECT":
                     GS = GameState.MainMenu;
                     break;

               }
               //check for game events and perform necessary functions
            }
            return gameEvent.ToLower();
         }
         return "";

      }

      public int GetBattleTurn()
      {
         return MM.GetTurn();
      }

      public int GetMapWidth()
      {
         return MM.GetMapWidth();
      }

      public int GetMapHeight()
      {
         return MM.GetMapHeight();
      }

      public string GetMapName()
      {
         return MM.GetMapName();
      }

      public int GetArmyFunds()
      {
         return MainArmy.gold;
      }

      public bool PurchaseUnits(int melee, int ranged, int magic)
      {
         int cost = (melee + ranged + magic) * 100;
         NameGenerator ng = new NameGenerator();
         if (cost <= MainArmy.gold)
         {
            Character c = null;
            for (int i = 0; i < melee; i++)
            {
               bool added = false;
                  while (!added)
               {
                  c = new Character(ng.GenerateName(), 0);
                  added = MainArmy.AddChar(c);
               }
            }
            for (int i = 0; i < ranged; i++)
            {
               bool added = false;
               while (!added)
               {
                  c = new Character(ng.GenerateName(), 1);
                  added = MainArmy.AddChar(c);
               }
            }
            for (int i = 0; i < magic; i++)
            {
               bool added = false;
               while (!added)
               {
                  c = new Character(ng.GenerateName(), 2);
                  added = MainArmy.AddChar(c);
               }
            }
            MainArmy.AddFunds(-cost);
            File.SaveArmy(MainArmy.GetSaveDataString());
            return true;
         }
         return false;
      }

      public string ExecuteSinglePlayerAttack()
      {
         string gameEvent = string.Empty;
         gameEvent = MM.executeAttack();
         File.SaveBattle(MM.GetBattleSaveData());
         if (gameEvent == "over")
         {
            MainArmy.AddFunds(50);
            foreach (Character c in MM.allies)
            {
               MainArmy.SwitchUnit(c);
            }
            File.SaveArmy(MainArmy.GetSaveDataString());
            File.DeleteBattleSave();
         }



         return gameEvent;
      }

      public string ExecuteMultiplayerAttack()
      {

         string gameEvent = string.Empty;
         if (MM.GetTurn() == 0)
         {
            string msg = MM.GetAttackerLocation().X.ToString()
               + "|" + MM.GetAttackerLocation().Y.ToString()
               + "|" + MM.GetTargetsLocation().X.ToString()
               + "|" + MM.GetTargetsLocation().Y.ToString();
            //temp fix
            Character attacker = GetCharOnTile(MM.GetAttackerLocation().X, MM.GetAttackerLocation().Y);
            Character target = GetCharOnTile(MM.GetTargetsLocation().X, MM.GetTargetsLocation().Y);
            int attackerInitialHealth = attacker.health;
            int targetInitialHealth = target.health;
            gameEvent = ExecuteSinglePlayerAttack();
            if (gameEvent != "invalid")
            {
               //Generate attack
               msg += "|" + (targetInitialHealth - target.health).ToString()
                + "|" + (attackerInitialHealth - attacker.health).ToString();
               client.SendAttack(msg);
               //send attack message to networking class
               if (gameEvent == "over")
               {
                  //send game over message if protocol needed

                  client.Disconnect();
                  client = null;
               }
               if (gameEvent == "turn_over")
               {
                  client.SendTurnEnd();
               }
            }
         }
         else
         {
            gameEvent = MM.MultiplayerAttack(int.Parse(attackInformation[1]), int.Parse(attackInformation[2]), int.Parse(attackInformation[3]), int.Parse(attackInformation[4]), int.Parse(attackInformation[5]));
            if (gameEvent == "over")
            {
               MainArmy.AddFunds(50);
               foreach (Character c in MM.allies)
               {
                  MainArmy.SwitchUnit(c);
               }
               File.SaveArmy(MainArmy.GetSaveDataString());

               client.Disconnect();
               client = null;
            }
            if (attackInformation[5] != "0")
            {
               MM.TargetHit = true;
            }
            else
            {
               MM.TargetHit = false;
            }
         }

         return gameEvent;
      }
   }
}
