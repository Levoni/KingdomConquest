using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
   // Class stors and x and y position
   public class Location : IEquatable<Location>
   {
      public int X { get; set; }
      public int Y { get; set; }

      public Location()
      {
         X = 0;
         Y = 0;
      }
      public Location(int xpos, int ypos)
      {
         X = xpos;
         Y = ypos;
      }

      public bool Equals(Location other)
      {
         if (other == null)
            return false;
         return X == other.X && Y == other.Y;
      }
   }

   //Class for A* pathfinding and daj's algorithem for highlighting tiles
   class Pathfinding
   {
      public Node[,] nMap { get; set; }
      List<List<Character>> charList;
      List<Node> opened;
      List<Node> closed;
      public Character selectedCharacter { get; set; }


      /// <summary>
      /// Constructor creates a Node map based on tile map
      /// </summary>
      /// <param name="tmap">tilemap to base nodes on</param>
      public Pathfinding(Tile[,] tmap)
      {
         nMap = new Node[tmap.GetLength(0), tmap.GetLength(1)];
         for (int i = 0; i < nMap.GetLength(0); i++)
            for (int j = 0; j < nMap.GetLength(1); j++)
            {
               nMap[i, j] = new Node(i, j, null, tmap[i, j]);
            }
         charList = new List<List<Character>>();
         opened = new List<Node>();
         closed = new List<Node>();
      }

      /// <summary>
      /// Adds characters to the comprehensive character list
      /// </summary>
      /// <param name="cList">list of characters</param>
      public void AddArmyForChecks(List<Character> cList)
      {
         charList.Add(cList);
      }

      public void ClearArmys()
      {
         charList.Clear();
      }

      /// <summary>
      /// Calculates the shortes path from one point to another
      /// </summary>
      /// <param name="startX">starting position X</param>
      /// <param name="startY">starting position Y</param>
      /// <param name="endX">Ending postition X</param>
      /// <param name="endY">Ending postition Y</param>
      /// <returns>List of Locations from point a to point b</returns>
      public List<Location> FindPathAStar(int startX, int startY, int endX, int endY, int attackRange, bool attack = false)
      {
         opened.Clear();
         closed.Clear();
         for (int i = 0; i < nMap.GetLength(0); i++)
            for (int j = 0; j < nMap.GetLength(1); j++)
               nMap[i, j].state = NodeState.Untested;
         bool Found = false;
         CalculateHValues(endX, endY);
         List<Location> path = new List<Location>();
         Node startNode = new Node(startX, startY, null);
         Node endNode = new Node(endX, endY, null);
         opened.Add(startNode);
         while (opened.Count > 0)
         {
            Node curNode = GetSmalletF();
            List<Node> walkableNodes = GetAdjacentWakableNodes(curNode, endX, endY);
            foreach (Node n in walkableNodes)
            {
               if (n.state == NodeState.Open)
               {
                  opened.Remove(n);
                  opened.Add(n);
               }
               if (n.X == endX && n.Y == endY)
               {
                  // shouldn't check for open tile to attack when only moving
                  if (attack)
                     if (IsOpenTileToAttack(attackRange, n))
                        Found = true;
                     else
                     {
                        opened.Remove(n);
                        n.state = NodeState.Untested;
                     }
                  else
                     Found = true;
               }
               if (Found)
               {
                  if (attack)
                     path = DetermineAttackPath(attackRange, n);
                  else
                     path = GerneratePathFromEndnode(n);
                  opened.Clear();
                  return path;
               }
            }
         }
         return path;
      }

      public List<Tile> GetTilesInMovementRange(int startX, int startY, int moveRange, int attackRange)
      {
         opened.Clear();
         closed.Clear();
         for (int i = 0; i < nMap.GetLength(0); i++)
            for (int j = 0; j < nMap.GetLength(1); j++)
               nMap[i, j].state = NodeState.Untested;
         List<Tile> Tiles = new List<Tile>();
         selectedCharacter = GetPlayerOnTile(startX, startY);
         Node startNode = new Node(startX, startY, null);
         opened.Add(startNode);
         while (opened.Count >= 1)
         {
            Node CurrentNode = GetSmalletF();
            List<Node> walkableNodes = AdjacentAndAttack(CurrentNode, moveRange, attackRange);
            foreach (Node n in walkableNodes)
            {
               if (n.G == moveRange)
               {
                  if (IsPlayerInPosition(n.X, n.Y) && IsSameTeam(GetPlayerOnTile(n.X, n.Y)))
                  {
                     n.G += SumOccupiedTiles(n);
                  }
               }

               if (n.G <= moveRange)
               {
                  if (!IsSameTeam(GetPlayerOnTile(n.X, n.Y)))
                     n.T.isMovable = true;
                  opened.Remove(n);
                  opened.Add(n);
               }
               else if (n.G > moveRange && n.G <= moveRange + attackRange)
               {
                  if (!IsSameTeam(GetPlayerOnTile(n.X, n.Y)))
                     n.T.isAttackable = true;
                  opened.Remove(n);
                  opened.Add(n);
               }
               else
               {
                  opened.Remove(n);
                  closed.Add(n);
               }
            }

         }

         return null;
      }

      /// <summary>
      /// Finds walkable nodes around current node and 
      /// calcuations their values (G,H) and sets their statuses and then
      /// returns them
      /// </summary>
      /// <param name="CurrentNode">Node method is currently on</param>
      /// <returns>List of nodes that are around the current node</returns>
      public List<Node> AdjacentAndAttack(Node CurrentNode, int moveRange, int attackRange)
      {
         // to do: put in check for characters as obsticales
         List<Node> walkableNodes = new List<Node>();
         CurrentNode.state = NodeState.Closed;
         opened.Remove(CurrentNode);
         List<Location> nextLocation = GetAdjacentTiles(CurrentNode);
         foreach (Location l in nextLocation)
         {
            if (l.X >= 0 && l.X < nMap.GetLength(0) && l.Y >= 0 && l.Y < nMap.GetLength(1))
            {
               Node nextNode = nMap[l.X, l.Y];
               if (nextNode.T.isPassable && (!IsPlayerInPosition(l.X, l.Y) || IsSameTeam(GetPlayerOnTile(l.X, l.Y))))
               {
                  if (nextNode.state != NodeState.Closed)
                  {
                     if (nextNode.state == NodeState.Open)
                     {
                        int traversalCost = 1;
                        int gTemp = CurrentNode.G + traversalCost;
                        if (gTemp < nextNode.G)
                        {
                           nextNode.parentNode = CurrentNode;
                           nextNode.G = gTemp;
                           walkableNodes.Add(nextNode);
                        }
                     }
                     else
                     {
                        nextNode.parentNode = CurrentNode;
                        nextNode.G = CurrentNode.G + 1;
                        nextNode.state = NodeState.Open;
                        walkableNodes.Add(nextNode);
                     }
                  }
               }
               else
               {
                  if (!closed.Remove(nextNode) && nextNode.state != NodeState.Closed)
                  {
                     nextNode.parentNode = CurrentNode;
                     nextNode.G = CurrentNode.G + 1;
                     if (IsPlayerInPosition(nextNode.X, nextNode.Y))
                     {
                        if (nextNode.G <= moveRange)
                           nextNode.G += SumOccupiedTiles(nextNode);
                     }
                     if (nextNode.G <= moveRange)
                     {
                        SetAttackTiles(l.X, l.Y, attackRange - 1);
                        nextNode.state = NodeState.Closed;
                        closed.Add(nextNode);
                     }
                     else if (nextNode.G <= moveRange + attackRange)
                     {
                        SetAttackTiles(l.X, l.Y, (attackRange + moveRange) - (nextNode.G));
                        opened.Remove(nextNode);
                        nextNode.state = NodeState.Closed;
                        closed.Add(nextNode);
                     }
                  }


               }
            }

         }
         return walkableNodes;
      }

      public void SetAttackTiles(int startx, int starty, int attackRange)
      {
         for (int i = startx - attackRange; i <= startx + attackRange; i++)
            for (int j = starty - attackRange; j <= starty + attackRange; j++)
            {
               if (i >= 0 && j >= 0 && i < nMap.GetLength(0) && j < nMap.GetLength(1)
                  && (Math.Abs(i - startx) + Math.Abs(j - starty) <= attackRange)
                  && !IsSameTeam(GetPlayerOnTile(i, j)))
                  nMap[i, j].T.isAttackable = true;
            }
      }

      /// <summary>
      /// Finds walkable nodes around current node and 
      /// calcuations their values (G,H) and sets their statuses and then
      /// returns them
      /// </summary>
      /// <param name="CurrentNode">Node method is currently on</param>
      /// <returns>List of nodes that are around the current node</returns>
      private List<Node> GetAdjacentWakableNodes(Node CurrentNode, int endx, int endy)
      {
         // to do: put in check for characters as obsticales
         List<Node> walkableNodes = new List<Node>();
         CurrentNode.state = NodeState.Closed;
         opened.Remove(CurrentNode);
         List<Location> nextLocation = GetAdjacentTiles(CurrentNode);
         foreach (Location l in nextLocation)
         {
            if (l.X >= 0 && l.X < nMap.GetLength(0) && l.Y >= 0 && l.Y < nMap.GetLength(1))
            {
               Node nextNode = nMap[l.X, l.Y];
               if (nextNode.T.isPassable)
               {
                  if (!IsPlayerInPosition(l.X, l.Y) || IsSameTeam(GetPlayerOnTile(l.X, l.Y)) || (endx == l.X && endy == l.Y))
                  {
                     if (nextNode.state != NodeState.Closed)
                     {
                        if (nextNode.state == NodeState.Open)
                        {
                           int traversalCost = 1;
                           int gTemp = CurrentNode.G + traversalCost;
                           if (gTemp < nextNode.G)
                           {
                              nextNode.parentNode = CurrentNode;
                              walkableNodes.Add(nextNode);
                           }
                        }
                        else
                        {
                           nextNode.parentNode = CurrentNode;
                           nextNode.G = CurrentNode.G + 1;
                           nextNode.state = NodeState.Open;
                           walkableNodes.Add(nextNode);
                        }
                     }
                  }
               }
            }

         }


         return walkableNodes;
      }

      /// <summary>
      /// Gets the locations around a node
      /// </summary>
      /// <param name="fromNode">node to check for tiles around</param>
      /// <returns>list of locations around the node</returns>
      private List<Location> GetAdjacentTiles(Node fromNode)
      {
         List<Location> adjacent = new List<Location>();
         adjacent.Add(new Location(fromNode.X + 1, fromNode.Y));
         adjacent.Add(new Location(fromNode.X - 1, fromNode.Y));
         adjacent.Add(new Location(fromNode.X, fromNode.Y + 1));
         adjacent.Add(new Location(fromNode.X, fromNode.Y - 1));
         return adjacent;
      }

      /// <summary>
      /// creates a Location list by working back from finished 
      /// position
      /// </summary>
      /// <param name="endNode">destination node</param>
      /// <returns>list of locations in order from start to finish</returns>
      private List<Location> GerneratePathFromEndnode(Node endNode)
      {
         List<Location> correctPath = new List<Location>();
         Node n = endNode;
         while (n != null)
         {
            correctPath.Add(new Location(n.X, n.Y));
            n = n.parentNode;
         }
         correctPath.Reverse();
         return correctPath;
      }

      /// <summary>
      /// Finds the node with the smallest F in the opened list
      /// </summary>
      /// <returns>Node with smallest F</returns>
      private Node GetSmalletF()
      {
         Node n = opened[0];
         foreach (Node tn in opened)
         {
            if (tn.F < n.F)
               n = tn;
         }
         return n;
      }

      /// <summary>
      /// Caculates H value for every node 
      /// </summary>
      /// <param name="Xend">X destination</param>
      /// <param name="Yend"> y destination</param>
      private void CalculateHValues(int Xend, int Yend)
      {
         foreach (Node n in nMap)
         {
            n.H = (int)Math.Sqrt(Math.Pow(Xend - n.X, 2) + Math.Pow(Yend - n.Y, 2));
         }
      }

      /// <summary>
      /// Looks through character lists to see if any 
      /// player is in a specified location
      /// </summary>
      /// <param name="x">x position to check</param>
      /// <param name="y">Y position to check</param>
      /// <returns>true if there is or false if not</returns>
      private bool IsPlayerInPosition(int x, int y)
      {
         foreach (List<Character> cList in charList)
            foreach (Character c in cList)
               if (c.X == x && c.Y == y)
               {
                  return true;
               }
         return false;
      }

      private Character GetPlayerOnTile(int x, int y)
      {
         foreach (List<Character> cList in charList)
            foreach (Character c in cList)
               if (c.X == x && c.Y == y)
               {
                  return c;
               }
         return null;
      }

      private bool IsSameTeam(Character c)
      {
         int teamOne = -1;
         int teamTwo = -2;
         for (int i = 0; i < charList.Count; i++)
            foreach (Character tempChar in charList[i])
               if (selectedCharacter == tempChar)
                  teamOne = i;
         for (int i = 0; i < charList.Count; i++)
            foreach (Character tempChar in charList[i])
               if (c == tempChar)
                  teamTwo = i;

         if (teamOne == teamTwo)
            return true;
         return false;
      }

      public int SumOccupiedTiles(Node n)
      {
         int count = 1;
         bool Person = true;
         Node tempNode = n;
         while (Person)
         {
            tempNode = tempNode.parentNode;
            if (tempNode != null && IsPlayerInPosition(tempNode.X, tempNode.Y) && selectedCharacter != GetPlayerOnTile(tempNode.X, tempNode.Y))
            {
               count++;
            }
            else
            {
               Person = false;
            }
         }
         return count;
      }

      public bool IsOpenTileToAttack(int attackRange, Node endNode)
      {
         Node tempNode = endNode.parentNode;
         while (tempNode != null && attackRange > 0)
         {
            if (!IsPlayerInPosition(tempNode.X, tempNode.Y) || selectedCharacter == GetPlayerOnTile(tempNode.X, tempNode.Y))
            {
               return true;
            }
            attackRange--;
            tempNode = tempNode.parentNode;
         }

         return false;
      }

      public List<Location> DetermineAttackPath(int attackRange, Node endNode)
      {
         Stack<Node> tempStack = new Stack<Node>();
         Node tempNode = endNode.parentNode;
         for (int i = 0; i < attackRange; i++)
         {
            if (tempNode != null)
            {
               tempStack.Push(tempNode);
               tempNode = tempNode.parentNode;
            }
         }
         while (tempStack.Count > 0)
         {
            tempNode = tempStack.Pop();
            if (!IsPlayerInPosition(tempNode.X, tempNode.Y) || selectedCharacter == GetPlayerOnTile(tempNode.X,tempNode.Y))
            {
               return GerneratePathFromEndnode(tempNode);
            }
         }
         return null;
      }
   }
}
