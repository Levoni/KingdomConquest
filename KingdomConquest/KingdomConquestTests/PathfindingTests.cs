using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using KingdomConquest_Shared;

namespace KingdomConquestTests
{
   [TestClass]
   public class PathfindingTests
   {
      [TestMethod]
      public void AdjacentAndAttack_ValidUntestedUnocupiedTile_ReturnsWalkableNodes()
      {
         Tile[,] map = new Tile[32, 32];
         for(int i = 0; i < 32; i++)
         {
            for (int j = 0; j < 32; j++)
            {
               map[i, j] = new Tile();
               map[i, j].isPassable = true;
            }
         }
         Character a1 = new Character("A1",0);
         a1.X = 15;
         a1.Y = 15;
         Character a2 = new Character("A1", 0);
         a2.X = 15;
         a2.Y = 16;
         List<Character> allies = new List<Character>();
         allies.Add(a1);
         allies.Add(a2);

         Pathfinding p = new Pathfinding(map);
         p.AddArmyForChecks(allies);

         p.selectedCharacter = a1;

         List<Node> nList = p.AdjacentAndAttack(new Node(15, 15, null), 1, 1);
         Tile result = null;
         foreach(Node n in nList)
         {
            if (n.T == map[15, 16])
               result = n.T;
         }
         
         Assert.AreEqual(result, map[15,16]);
      }

      [TestMethod]
      public void AdjacentAndAttack_ValidUntestedOcupiedTile_ReturnsNull()
      {
         Tile[,] map = new Tile[32, 32];
         for (int i = 0; i < 32; i++)
         {
            for (int j = 0; j < 32; j++)
            {
               map[i, j] = new Tile();
               map[i, j].isPassable = true;
            }
         }

         Character a1 = new Character("A1", 0);
         a1.X = 15;
         a1.Y = 15;
         Character a2 = new Character("A1", 0);
         a2.X = 15;
         a2.Y = 16;
         List<Character> allies = new List<Character>();
         allies.Add(a1);
         allies.Add(a2);

         Pathfinding p = new Pathfinding(map);
         p.AddArmyForChecks(allies);

         List<Node> nList = p.AdjacentAndAttack(new Node(15, 15, null), 1, 1);
         Tile result = null;
         foreach (Node n in nList)
         {
            if (n.T == map[15, 16])
               result = n.T;
         }

         Assert.AreEqual(result, null);
      }

      [TestMethod]
      public void AdjacentAndAttack_ValidOutOfRangeTile_ReturnsNull()
      {
         Tile[,] map = new Tile[32, 32];
         for (int i = 0; i < 32; i++)
         {
            for (int j = 0; j < 32; j++)
            {
               map[i, j] = new Tile();
               map[i, j].isPassable = true;
            }
         }

         Pathfinding p = new Pathfinding(map);

         List<Node> nList = p.AdjacentAndAttack(new Node(15, 15, null), 1, 1);
         Tile result = null;
         foreach (Node n in nList)
         {
            if (n.T == map[16, 16])
               result = n.T;
         }

         Assert.AreEqual(result, null);
      }

      [TestMethod]
      public void AdjacentAndAttack_ValidtestedUnocupiedTile_ReturnsWalkableNodes()
      {
         Tile[,] map = new Tile[32, 32];
         for (int i = 0; i < 32; i++)
         {
            for (int j = 0; j < 32; j++)
            {
               map[i, j] = new Tile();
               map[i, j].isPassable = true;
            }
         }

         Pathfinding p = new Pathfinding(map);
         p.nMap[15, 16].state = NodeState.Open;
         p.nMap[15, 16].G = 5;
         List<Node> nList = p.AdjacentAndAttack(new Node(15, 15, null), 1, 1);
         Tile result = null;
         foreach (Node n in nList)
         {
            if (n.T == map[15, 16])
               result = n.T;
         }

         Assert.AreEqual(result, map[15, 16]);
      }

      [TestMethod]
      public void AdjacentAndAttack_InvalidTile_ReturnNull()
      {
         Tile[,] map = new Tile[32, 32];
         for (int i = 0; i < 32; i++)
         {
            for (int j = 0; j < 32; j++)
            {
               map[i, j] = new Tile();
               map[i, j].isPassable = true;
            }
         }
         map[15, 16].isPassable = false;
         Pathfinding p = new Pathfinding(map);
         p.nMap[15, 15].G = 1;
         List<Node> nList = p.AdjacentAndAttack(p.nMap[15, 15], 1, 1);
         Tile result = null;
         foreach (Node n in nList)
         {
            if (n.T == map[15, 16])
               result = n.T;
         }

         Assert.AreEqual(result, null);
      }

   }
}
