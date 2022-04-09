using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
   public enum MapState
   {
      initializing,
      standby,
      movingCharacter,
      attackingCharacter,
      over
   }
   class MapManager
   {
      //-----------------------------------------------------------------------
      //START Variable Declaration
      //-----------------------------------------------------------------------
      Map map;

      //temp solution
      public List<Character> allies { get; private set; }
      public List<Character> enemies { get; private set; }

      List<List<Character>> Armys;
      Character selectedCharacter;
      Character targetCharacter;
      public bool TargetHit { get; set; }
      Random rand = new Random();

      Turn turn;
      MapState state;

      public List<Location> path { get; private set; }
      public enum Turn { allyTurn, enemyTurn }
      public int team { get; set; }
      //-----------------------------------------------------------------------
      //END Variable Declaration
      //-----------------------------------------------------------------------

      //-----------------------------------------------------------------------
      //Constructor
      //-----------------------------------------------------------------------
      public MapManager(List<Character> allyImport, List<Character> enemyImport, string mapName = "sampleMap")
      {
         map = new FileIO("./map_files/").GenerateMapFromFile(mapName);
         enemies = enemyImport;
         allies = allyImport;
         ScaleEnemies();
         Armys = new List<List<Character>>();
         Armys.Add(allies);
         Armys.Add(enemies);
         map.setPathfindingArmyList(Armys);
         selectedCharacter = null;
         turn = Turn.allyTurn;  //needs to be fixed for multiplayer
         state = MapState.initializing;
      }

      public MapManager(List<Character> allyImport, string mapName = "sampleMap")
      {
         map = new FileIO("./map_files/").GenerateMapFromFile(mapName);
         enemies = new FileIO("./enemy_armys/").LoadEnemyCharList(mapName);
         allies = allyImport;
         ScaleEnemies();
         Armys = new List<List<Character>>();
         Armys.Add(allies);
         Armys.Add(enemies);
         map.setPathfindingArmyList(Armys);
         selectedCharacter = null;
         turn = Turn.allyTurn;  //needs to be fixed for multiplayer
         state = MapState.initializing;
      }

      private void ScaleEnemies()
      {
         int levelAverage = 0;
         foreach (Character tempCharacter in allies)
         {
            levelAverage += tempCharacter.level;
         }
         if (levelAverage != 0)
            levelAverage /= allies.Count;

         foreach (Character tempCharacter in enemies)
         {
            for (int i = 0; i < levelAverage; i++)
            {
               tempCharacter.LevelUp();
               tempCharacter.ResetHealth();
            }
         }
      }

      public void HighlightStartTiles()
      {
         for (int i = map.GetStartLocation(team).X - 3; i <= map.GetStartLocation(team).X + 3; i++)
            for (int j = map.GetStartLocation(team).Y - 3; j <= map.GetStartLocation(team).Y + 3; j++)
            {
               if (i >= 0 && j >= 0 && i < map.GetWidth() && j < map.GetHeight()
                  && map.GetTile(i, j).isPassable)
                  map.GetTile(i, j).isMovable = true;
            }
      }

      public void GetSelectedCharLocation(out int x, out int y)
      {
         x = selectedCharacter.X;
         y = selectedCharacter.Y;
      }

      //-----------------------------------------------------------------------
      //Allows each enemy unit to have a turn
      //
      //POTENTIAL PROBLEM: Tile highlights may appear visible as enemies are
      //                   taking their turns
      //-----------------------------------------------------------------------
      public string EnemyAction()
      {
         if (state == MapState.standby)
         {
            for (int i = 0; i < enemies.Count; i++)
            {
               SelectUnitOnTile(enemies[i].X, enemies[i].Y);
               if (selectedCharacter.isAbleToMove)
               {
                  string gameEvent = DetermineMove(enemies[i]);
                  if (gameEvent == "standby")
                  {
                     state = MapState.standby;
                     selectedCharacter.isAbleToAttack = false;
                     selectedCharacter.isAbleToMove = false;
                  }
                  return gameEvent;
               }
            }
            IncrimentTurn();
            return "turn change";
         }
         return "invalid";
      }

      //-----------------------------------------------------------------------
      //Determines if an enmy unit should attack an ally unit, and if so,
      //carries out the required movement and attack.  Otherwise, the enemy
      //unit's turn is skipped.
      //-----------------------------------------------------------------------
      private string DetermineMove(Character computerUnit)
      {
         //If unit is in range
         //If favorable outcome
         //Move and attack

         List<Character> alliesInRange = FindAllies(computerUnit);
         int bestDamageGiven = 0;
         int bestDamageReceived = int.MaxValue;
         int bestIndex = -1;

         for (int i = 0; i < alliesInRange.Count; i++)
         {
            int[] potentialDamage = DetermineDamage(computerUnit,
                                                    alliesInRange[i]);
            if (i == -1 || (potentialDamage[0] >= bestDamageGiven &&
                            potentialDamage[1] < bestDamageReceived))
            {
               bestDamageGiven = potentialDamage[0];
               bestDamageReceived = potentialDamage[1];
               bestIndex = i;
            }
         }

         if (bestIndex != -1) //Move enemy to tile adjacent to chosen ally and attack
         {
            AttackUnit(alliesInRange[bestIndex].X, alliesInRange[bestIndex].Y);
            return "attack";
         }
         //could put Code for moving to nearest enemy when they can't attack here

         return "standby";
      }

      //-----------------------------------------------------------------------
      //Finds ally units within attacking range of an enemy unit.
      //
      //PROBLEM: All units within range are included in the returned list,
      //         including other enemy units.
      //-----------------------------------------------------------------------
      private List<Character> FindAllies(Character computerUnit)
      {
         List<Character> alliesInRange = new List<Character>();
         map.ResetTileTempInformation();

         map.SetMoveAttackTiles(computerUnit.X, computerUnit.Y,
            computerUnit.movementRange, computerUnit.attackRange);
         for (int i = 0; i < Armys.Count; i++)
         {
            if (i != (int)turn)
            {
               foreach (Character c in Armys[i])
               {
                  if (map.GetTile(c.X, c.Y).isMovable || map.GetTile(c.X, c.Y).isAttackable)
                     alliesInRange.Add(c);
               }
            }
         }
         return alliesInRange;
      }


      //-----------------------------------------------------------------------
      //Calculates the outcome of an attack for both the attacking and
      //defending units.
      //-----------------------------------------------------------------------
      private int[] DetermineDamage(Character attackingUnit,
                                    Character defendingUnit)
      {
         int[] damage = new int[2];
         damage[0] = 0;
         damage[1] = 0;

         if (attackingUnit.charClass != charClassType.Magic)
            damage[0] = attackingUnit.GetStrength() - defendingUnit.physicalResistance;
         else
            damage[0] = attackingUnit.GetStrength() - defendingUnit.magicResistance;

         if (attackingUnit.charClass != charClassType.Magic)
            damage[1] = defendingUnit.GetStrength() - attackingUnit.physicalResistance;
         else
            damage[1] = defendingUnit.GetStrength() - attackingUnit.magicResistance;

         return damage;
      }

      //-----------------------------------------------------------------------
      //START Import from Map class
      //-----------------------------------------------------------------------

      /// <summary>
      /// Runs the logic for when a spot on the map is clicked
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public string MapClick(int x, int y)
      {
         if (turn == Turn.allyTurn)
         {
            switch (state)
            {
               case MapState.initializing:
                  return SetupClick(x, y);
               case MapState.standby:
                  return inGameClick(x, y);
            }
         }
         else
         {
            return "not turn";
         }
         //temp return
         return "invalid";
      }

      /// <summary>
      /// The function that is ran when map is in setup 
      /// and the map was clicked
      /// </summary>
      /// <param name="x">x position of click</param>
      /// <param name="y">y position of click</param>
      /// <returns></returns>
      private string SetupClick(int x, int y)
      {
         Character tempChar = GetCharNotOnMap(allies);
         if (tempChar == null)
         {
            //to do: Run the enemy setup stuff
            state = MapState.standby;
            ResetTempMapInformation();
            return "initialized";
         }
         else
         {
            if (x >= 0 && y >= 0 && x < map.GetWidth() && y < map.GetHeight()
            && x <= map.GetStartLocation(team).X + 3 && x >= map.GetStartLocation(team).X - 3
            && y <= map.GetStartLocation(team).Y + 3 && y >= map.GetStartLocation(team).Y - 3)
            {
               if (GetCharOnTile(x, y) == null)
               {
                  tempChar.X = x;
                  tempChar.Y = y; ;
                  if (tempChar != null)
                  {
                     return "Set";
                  }
               }
               else
               {
                  Character oldChar = GetCharOnTile(x, y);
                  oldChar.X = -1;
                  oldChar.Y = -1;
                  tempChar.X = x;
                  tempChar.Y = y;
                  return "switch";
               }
            }
         }
         return "invalid";
      }

      /// <summary>
      /// Gets the first character that is not on the map
      /// </summary>
      /// <param name="cList">list of characters to check</param>
      /// <returns>first character that isn't of map</returns>
      private Character GetCharNotOnMap(List<Character> cList)
      {
         for (int i = 0; i < cList.Count; i++)
         {
            if (cList[i].X <= 0 && cList[i].Y <= 0)
            {
               return cList[i];
            }
         }
         return null;
      }

      /// <summary>
      /// Checks if amy units in a army can still do something
      /// </summary>
      /// <returns>true if actions are available false if not </returns>
      private bool isTurnDone()
      {
         foreach (Character C in Armys[(int)turn])
         {
            if (C.isAbleToAttack || C.isAbleToMove)
            {
               return false;
            }
         }
         return true;
      }

      private void SetAllyTurn()
      {
         turn = Turn.allyTurn;
         foreach (Character a in allies)
         {
            a.isAbleToMove = true;
            a.isAbleToAttack = true;
         }
      }

      private void SetEnemyTurn()
      {
         turn = Turn.enemyTurn;
         foreach (Character e in enemies)
         {
            e.isAbleToMove = true;
            e.isAbleToAttack = true;
         }
         EnemyAction();
      }
      /// <summary>
      /// Click function that is run when game is in progresses
      /// and the map was clicked
      /// </summary>
      /// <param name="x">x position of click</param>
      /// <param name="y">y position of click</param>
      /// <returns></returns>
      private string inGameClick(int x, int y)
      {
         if (state == MapState.standby)
         {
            if (selectedCharacter == null)
            {
               SelectUnitOnTile(x, y);
               if (selectedCharacter != null)
                  return "select";
            }
            else
            {
               if (!map.GetTile(x, y).isMovable && !map.GetTile(x, y).isAttackable)
               {
                  if (selectedCharacter.X != x || selectedCharacter.Y != y)
                  {
                     DeselectUnit();
                     SelectUnitOnTile(x, y);
                     if (selectedCharacter != null)
                        return "select";
                  }
                  else
                  {
                     selectedCharacter.isAbleToAttack = false;
                     selectedCharacter.isAbleToMove = false;
                  }
                  DeselectUnit();
                  if (isTurnDone())
                  {
                     IncrimentTurn();
                     return "turn_over";
                  }
                  return "deselect";
               }
               else
               {
                  //check for it being your turn
                  if (allies.Contains(selectedCharacter))
                  {
                     if (selectedCharacter.isAbleToAttack
                         && GetCharOnTile(x, y) != null
                         && !CheckForSameTeam(selectedCharacter, GetCharOnTile(x, y))
                         && (map.GetTile(x, y).isMovable || map.GetTile(x, y).isAttackable))
                     {
                        AttackUnit(x, y);

                        return "attack";
                     }
                     else if (selectedCharacter.isAbleToMove
                             && map.GetTile(x, y).isMovable
                             && GetCharOnTile(x, y) == null)
                     {
                        SetCharPosition(selectedCharacter, x, y);
                        DeselectUnit();
                        return "move";
                     }
                     else
                     {
                        DeselectUnit();
                        return "deselect";
                     }
                  }
               }
            }
         }
         //temp holder
         return "invalid";
      }

      /// <summary>
      /// Sets the GameManagers selected unit as the 
      /// unit currently on a specified tile
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public void SelectUnitOnTile(int x, int y)
      {
         selectedCharacter = GetCharOnTile(x, y);
         if (selectedCharacter != null)
         {
            if (selectedCharacter.isAbleToMove)
            {
               map.SetMoveAttackTiles(x, y, selectedCharacter.movementRange, selectedCharacter.attackRange);
               //SetMovementHelper(selectedCharacter.X + 1, selectedCharacter.Y, selectedCharacter.movementRange);
               //SetMovementHelper(selectedCharacter.X - 1, selectedCharacter.Y, selectedCharacter.movementRange);
               //SetMovementHelper(selectedCharacter.X, selectedCharacter.Y + 1, selectedCharacter.movementRange);
               //SetMovementHelper(selectedCharacter.X, selectedCharacter.Y - 1, selectedCharacter.movementRange);
            }
            if (selectedCharacter.isAbleToAttack)
            {
               map.SetAttackTiles(x, y, selectedCharacter.attackRange, selectedCharacter);
               //SetAttackHelper(selectedCharacter.X, selectedCharacter.Y, selectedCharacter.attackRange);
            }
         }
      }

      /// <summary>
      /// Deselects unit and resets the map info for 
      /// the selected unit
      /// </summary>
      public void DeselectUnit()
      {
         selectedCharacter = null;
         ResetTempMapInformation();
      }

      /// <summary>
      /// Resets all of the temporary variables in each tile
      /// </summary>
      public void ResetTempMapInformation()
      {
         map.ResetTileTempInformation();
      }

      /// <summary>
      /// selected unit to attack and finds the path to them 
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public bool AttackUnit(int x, int y)
      {
         targetCharacter = GetCharOnTile(x, y);
         List<Location> tempPath = new List<Location>();
         int range = selectedCharacter.attackRange;
         if (targetCharacter != null && !CheckForSameTeam(selectedCharacter, targetCharacter))
         {
            TargetHit = selectedCharacter.GetHitOrMiss(targetCharacter.GetDexterity(), rand.Next());
            path = map.FindPath(selectedCharacter.X, selectedCharacter.Y, x, y, selectedCharacter.attackRange, true);
            if (path.Count > 1)
            {
               selectedCharacter.X = path[0].X;
               selectedCharacter.Y = path[0].Y;
               state = MapState.movingCharacter;
               return true;
            }
            //if the character doesn't move tiles to attack
            if (selectedCharacter.isAbleToAttack && (Math.Abs((y - selectedCharacter.Y)) + Math.Abs((x - selectedCharacter.X))) <= range)
            {
               state = MapState.attackingCharacter;
            }
         }
         return false;
      }

      public string executeAttack()
      {
         if (selectedCharacter != null && targetCharacter != null)
         {
            selectedCharacter.attack(targetCharacter, TargetHit);
            if (!targetCharacter.isAlive)
            {
               selectedCharacter.GainExperience(selectedCharacter.GenerateExperienceGain(targetCharacter.level) + 1);
               allies.Remove(targetCharacter);
               enemies.Remove(targetCharacter);
            }
            targetCharacter = null;
            DeselectUnit();
            state = MapState.standby;
            //to do: make this a function
            if (IsBattleOver())
            {
               state = MapState.over;
               return "over";
            }
            if (isTurnDone())
            {
               IncrimentTurn();
               return "turn_over";
            }
            return "hit";
         }
         return "invalid";
      }

      public string MultiplayerAttack(int xAttacker, int yAttacker, int xReciver, int yReciver, int damage)
      {
         Character reciver = GetCharOnTile(xReciver, yReciver);
         // to do: make this function in character class
         reciver.health -= damage;
         state = MapState.standby;
         if (reciver.health <= 0)
         {
            GetCharOnTile(xAttacker, yAttacker).GainExperience(GetCharOnTile(xAttacker, yAttacker).GenerateExperienceGain(reciver.level) + 1);
            reciver.isAlive = false;
            reciver.health = 0;
            allies.Remove(reciver);
            enemies.Remove(reciver);
         }
         if (IsBattleOver())
         {
            state = MapState.over;
            return "over";
         }
         return "";
      }

      /// <summary>
      /// Gets a character on a tile and returns null
      /// if no characters are on it
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      /// <returns>CHaracter on tile or null if no character</returns>
      public Character GetCharOnTile(int x, int y)
      {
         foreach (List<Character> charList in Armys)
         {
            foreach (Character c in charList)
            {
               if (c.X == x && c.Y == y)
                  return c;
            }
         }
         return null;
      }

      /// <summary>
      /// Sets a character's position
      /// </summary>
      /// <param name="c">Character to move</param>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public void SetCharPosition(Character c, int x, int y)
      {
         if (map.GetTile(x, y).isMovable)
         {
            path = map.FindPath(c.X, c.Y, x, y, c.attackRange);
            c.SetPosition(path[0].X, path[0].Y);
            state = MapState.movingCharacter;
         }
      }

      public void IncrementPathLocation(Character c, List<Location> path)
      {
         path.RemoveAt(0);
         if (path.Count > 0)
         {
            Location[] locations = path.ToArray();
            c.X = locations[0].X;
            c.Y = locations[0].Y;
         }
      }

      /// <summary>
      /// Checks if two characters are on the same team
      /// </summary>
      /// <param name="one">First character</param>
      /// <param name="two">Second character</param>
      /// <returns>True if they are on the same team, false if not</returns>
      public bool CheckForSameTeam(Character one, Character two)
      {
         // teams: 1=allies, 2=enemies, etc
         int CharacterOneTeam = -1;
         int CharacterTwoTeam = -1;
         for (int i = 0; i < Armys.Count; i++)
         {
            foreach (Character c in Armys[i])
            {
               if (one.Equals(c))
               {
                  CharacterOneTeam = i;
               }
               if (two.Equals(c))
               {
                  CharacterOneTeam = i;
               }
            }
         }

         return CharacterOneTeam == CharacterTwoTeam;
      }

      /// <summary>
      /// Gets a tile at a specific location
      /// </summary>
      /// <param name="x">x position of tile</param>
      /// <param name="y">y position of tile</param>
      /// <returns></returns>
      public Tile GetTile(int x, int y)
      {
         return map.GetTile(x, y);
      }

      public string GetBattleSaveData()
      {
         string info = string.Empty;
         info += ((int)turn).ToString() + ":" + ((int)state).ToString() + ':' +map.mapName + ";";
         for (int i = 0; i < Armys.Count; i++)
         {
            for (int j = 0; j < Armys[i].Count; j++)
            {
               Character c = Armys[i][j];
               info += c.ToString();
               if (j != Armys[i].Count - 1)
                  info += ",";
            }
            if (i != Armys.Count - 1)
               info += ";";
         }
         return info;
      }

      public void IncrimentState()
      {
         if (state != MapState.attackingCharacter)
            state = state + 1;
         else
            state = MapState.initializing;
      }

      public void setState(int tempState)
      {
         if (state >= 0 && state <= MapState.attackingCharacter)
            state = (MapState)tempState;
      }

      public int GetTurn()
      {
         return (int)turn;
      }

      public void IncrimentTurn()
      {
         if (turn == Turn.enemyTurn)
            turn = Turn.allyTurn;
         else
            turn = Turn.enemyTurn;
         foreach (List<Character> cList in Armys)
            foreach (Character c in cList)
            {
               c.isAbleToAttack = true;
               c.isAbleToMove = true;
            }
         selectedCharacter = null;
         map.ResetTileTempInformation();
         state = MapState.standby;
      }

      public Location GetAttackerLocation()
      {
         if (selectedCharacter != null)
            return new Location(selectedCharacter.X, selectedCharacter.Y);
         return null;
      }

      public Location GetTargetsLocation()
      {
         if (targetCharacter != null)
            return new Location(targetCharacter.X, targetCharacter.Y);
         return null;
      }

      private bool IsBattleOver()
      {
         foreach (List<Character> cList in Armys)
         {
            if (cList.Count == 0)
               return true;
         }
         return false;
      }

      public void SetEnemyArmy(List<Character> cList)
      {
         Armys.Remove(enemies);
         enemies = cList;
         Armys.Add(enemies);
         map.setPathfindingArmyList(Armys);
      }

      public MapState GetMapState()
      {
         return state;
      }

      public int GetMapWidth()
      {
         return map.GetWidth();
      }

      public int GetMapHeight()
      {
         return map.GetHeight();
      }

      public string GetMapName()
      {
         return map.mapName;
      }


      //-----------------------------------------------------------------------
      //END Import from Map class
      //-----------------------------------------------------------------------
   }
}
