using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
   public class Map
   {
      Tile[,] tileMap;
      public string mapName { get; set; }
      int width;
      int height;
      public int maxUnitsPerTeam { get; set; }

      List<Location> startLocations;
      //int teamTurn;
      //Character selectedChar;
      //List<Character> Allies;
      //List<Character> Enemies;
      Pathfinding pathfinding;
      //MapState state;

      /*

      public enum MapState
      {
         initializing,
         standby,
         movingChar,
         attackintChar
      }
      */

      //Will allow for more than 2 armys


      /// <summary>
      /// Default Constructor
      /// </summary>
      /// <param name="x">number of tiles in x direction</param>
      /// <param name="y">number of tiles in y direction</param>
      public Map(int x, int y)
      {
         width = x;
         height = y;
         tileMap = new Tile[width, height];
         for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
               tileMap[i, j] = new Tile();
               tileMap[i, j].isPassable = true;
            }
         pathfinding = new Pathfinding(tileMap);
         startLocations = new List<Location>();

         //pathfinding.AddArmyForChecks(Allies);
         //pathfinding.AddArmyForChecks(Enemies);
         //teamTurn = 0;
         //state = MapState.initializing;
         //testData();
      }

      public void addStartLocation(int x,int y)
      {
         Location l = new Location(x, y);
         startLocations.Add(l);
      }

      private void testData()
      {
         //Put Stub inforation here
         Location L = new Location(15, 15);
         startLocations.Add(L);
         L = new Location(15, 20);
         startLocations.Add(L);
      }


      //to do: Turn things and AI


      /*
      /// <summary>
      /// Runs the logic for when a spot on the map is clicked
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public bool MapCLick(int x, int y)
      {
         /*switch (state)
         {
            case MapState.initializing:
               SetupClick(x, y);
               break;
            case MapState.standby:*/
      //return inGameClick(x, y);
      /*break;*/

      //}
      //}
      //*/

      /*
      private void SetupClick(int x, int y)
        {
            
        }

      */


      /*
      private bool inGameClick(int x, int y)
      {
         if (selectedChar != null)
         {
            return SelectUnitOnTile(x, y);
         }
         else
         {
            if (!GetTile(x, y).isMovable
                || GetCharOnTile(x, y) == selectedChar)
            {
               DiselectUnit();
               return false;
            }
            else
            {
               //check for it being your turn
               if (Allies.Contains(selectedChar))
               {
                  if (selectedChar.isAbleToAttack
                      && GetCharOnTile(x, y) != null
                      && !IsSameTeam(selectedChar, GetCharOnTile(x, y))
                      && (GetTile(x, y).isMovable || GetTile(x, y).isAttackable))
                  {
                     //to do: move next to enemy here
                     selectedChar.attack(GetCharOnTile(x, y));
                  }
                  else if (selectedChar.isAbleToMove
                          && GetTile(x, y).isMovable
                          && GetCharOnTile(x, y) == null)
                  {
                     selectedChar.SetPosition(x, y);
                  }
               }
               return true;
            }
         }
      }
        */

      /*
      /// <summary>
      /// Sets the GameManagers selected unit as the 
      /// unit currently on a specified tile
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public bool SelectUnitOnTile(int x, int y)
      {
         selectedChar = GetCharOnTile(x, y);
         if (selectedChar != null)
         {
            SetMovementHelper(selectedChar.X, selectedChar.Y, selectedChar.movementRange);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Diselects unit and resets the map info for 
      /// the selected unit
      /// </summary>
      public void DiselectUnit()
      {
         selectedChar = null;
         ResetTempMapInformation();
      }

      /// <summary>
      /// selected unit attacks a character on a 
      /// specific tile if there is a character on it
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public bool AttackUnit(int x, int y)
      {
         Character reciver = GetCharOnTile(x, y);
         int range = selectedChar.attackRange;
         if (reciver != null && !IsSameTeam(selectedChar, reciver))
         {
            List<Location> path = pathfinding.FindPathAStar(selectedChar.X, selectedChar.Y, x, y);
            for (int i = 0; i < path.Count; i++)
            {
               selectedChar.SetPosition(path[i].X, path[i].Y);
               if ((Math.Abs((y - path[i].Y)) + Math.Abs((x - path[i].X))) < range)
               {
                  selectedChar.attack(reciver);
                  return true;
               }
            }
         }
         return false;
      }
        */

      /*
      /// <summary>
      /// Resets all of the temporary variables in each tile
      /// </summary>
      public void ResetTempMapInformation()
      {
         for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
               tileMap[i, j].ResetTempInformation();
            }
      }
      */

      /// <summary>
      /// Gets a tile in at a specified position
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      /// <returns></returns>
      public Tile GetTile(int x, int y)
      {
         if (x >= 0 && y >= 0 && x < width && y < width)
            return tileMap[x, y];
         return null;
      }

      /*
      /// <summary>
      /// Gets a character on a tile and returns null
      /// if no characters are on it
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      /// <returns>CHaracter on tile or null if no character</returns>
      public Character GetCharOnTile(int x, int y)
      {
         foreach (Character c in Allies)
         {
            if (c.X == x && c.Y == y)
               return c;
         }
         return null;
      }
      */

      /*
      /// <summary>
      /// Sets a character's position
      /// </summary>
      /// <param name="c">Character to move</param>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public void SetCharPosition(Character c, int x, int y)
      {
         if (GetTile(x, y).isMovable)
         {
            List<Location> path = pathfinding.FindPathAStar(c.X, c.Y, x, y);
            for (int i = 1; i < path.Count; i++)
               c.SetPosition(path[i].X, path[i].Y);
         }
      }
      */

      /*
      /// <summary>
      /// Checks if two characters are on the same team
      /// </summary>
      /// <param name="one">First character</param>
      /// <param name="two">Second character</param>
      /// <returns>True if they are on the same team, false if not</returns>
      public bool IsSameTeam(Character one, Character two)
      {
         // teams: 1=allies, 2=enemies, etc
         int CharacterOneTeam = -1;
         int CharacterTwoTeam = -1;
         for (int i = 0; i < Armys.Count; i++)
         {
            foreach (Character c in Allies)
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
      */
      /*
      /// <summary>
      /// recursivly moves out from player position,
      /// if all the conditions are met it sets the tile
      /// isMovable to true
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      /// <param name="distanceToMove">distance left to move</param>
      private void SetMovement(int x, int y, int distanceToMove)
      {
         if (x > 0 && y > 0 && x < width && y < height)
         {
            if (distanceToMove > 0)
            {
               if (tileMap[x, y].isPassable && GetCharOnTile(x, y) == null)
               {
                  if (!tileMap[x, y].isMovable)
                  {
                     tileMap[x, y].isMovable = true;
                     SetMovement(x - 1, y, distanceToMove - 1);
                     SetMovement(x + 1, y, distanceToMove - 1);
                     SetMovement(x, y - 1, distanceToMove - 1);
                     SetMovement(x, y + 1, distanceToMove - 1);
                  }
               }
               else
               {
                  SetAttackHelper(x, y, selectedChar.attackRange - 1);
               }
            }

            return CharacterOneTeam == CharacterTwoTeam;
        }
        */

      /*
      /// <summary>
      /// Helper fuction for SetMovement
      /// </summary>
      /// <param name="c">Character to check movement for</param>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      public void SetMovementHelper(int x, int y, int distanceToMove)
      {
          SetMovement(x, y, distanceToMove);
      }
      */

      /*
      /// <summary>
      /// Helper fuction for SetAttack
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      /// <param name="attackRange">the attack range to move out </param>
      public void SetAttackHelper(int x, int y, int attackRange)
      {
          GetTile(x, y).isAttackable = true;
          SetAttack(x + 1, y, attackRange);
          SetAttack(x - 1, y, attackRange);
          SetAttack(x, y + 1, attackRange);
          SetAttack(x, y - 1, attackRange);
      }

      */

      /*
      /// <summary>
      /// Recursivly moves out from a position,
      /// if all the conditions are met it sets the tile
      /// isAttackable to true.
      /// </summary>
      /// <param name="x">X position of tile</param>
      /// <param name="y">Y position of tile</param>
      /// <param name="distanceToMove">distance left to move</param>
      private void SetAttack(int x, int y, int distanceToMove)
      {
          if (distanceToMove > 0)
          {
          if (x > 0 && x < width && y > 0 && y < height)
          {
             if (!GetTile(x, y).isAttackable)
             {
                GetTile(x, y).isAttackable = true;
                SetAttack(x + 1, y, distanceToMove - 1);
                SetAttack(x - 1, y, distanceToMove - 1);
                SetAttack(x, y + 1, distanceToMove - 1);
                SetAttack(x, y - 1, distanceToMove - 1);
             }
          }
       }
      }
      */

      public int GetWidth()
      {
         return width;
      }

      public int GetHeight()
      {
         return height;
      }

      public bool TileIsMovable(int x, int y)
      {
         return tileMap[x, y].isMovable;
      }

      public bool TileIsAttackable(int x, int y)
      {
         return tileMap[x, y].isAttackable;
      }

      public void ResetTileTempInformation()
      {
         for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
               tileMap[i, j].ResetTempInformation();
            }
      }

      public void SetTileIsMovable(int x, int y, bool value)
      {
         tileMap[x, y].isMovable = value;
      }

      public bool GetTileIsPassable(int x, int y)
      {
         return tileMap[x, y].isPassable;
      }

      public void setPathfindingArmyList(List<List<Character>> masterList)
      {
         pathfinding.ClearArmys();
         foreach (List<Character> cList in masterList)
         {
            pathfinding.AddArmyForChecks(cList);
         }
      }

      public List<Location> FindPath(int currentX, int currentY,
                                     int desiredX, int desiredY,
                                     int attackRange, bool attack = false)
      {
         return pathfinding.FindPathAStar(currentX, currentY, desiredX, desiredY, attackRange, attack);
      }

      public void SetMoveAttackTiles(int xStart, int yStart, int moveRange, int attackRange)
      {
         pathfinding.GetTilesInMovementRange(xStart, yStart, moveRange, attackRange);
      }

      public void SetAttackTiles(int xStart, int yStart, int attackRange, Character baseCharater)
      {
         pathfinding.selectedCharacter = baseCharater;
         pathfinding.SetAttackTiles(xStart, yStart, attackRange);
      }

      public Location GetStartLocation(int team)
      {
         return startLocations[team];
      }
   }
}
