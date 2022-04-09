using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using KingdomConquest_Shared;

namespace KingdomConquestTests
{
   [TestClass]
   public class MapManagerTest
   {
      [TestMethod]
      public void inGameClick_ValidAttack_ReturnsAttack()
      {
         string result;
         //board setup
         List<Character> ally = new List<Character>(), enemy = new List<Character>();
         Character c = new Character("ally");
         c.SetPosition(0, 0);
         c.SetDexterity(5);
         ally.Add(c);
         c = new Character("enemy");
         c.SetPosition(1, 1);
         enemy.Add(c);
         MapManager mm = new MapManager(ally, enemy, "sampleMap");
         //Gets us down the true path of node 1
         mm.setState(1);
         //Down the false path of node 4
         mm.SelectUnitOnTile(0, 0);
         //Sets the tile to be attackable
         mm.GetTile(1, 1).isAttackable = true;
         //Clicks on the enemy unit
         result = mm.MapClick(1, 1);
         //Test
         Assert.AreEqual(result, "attack");
      }

      [TestMethod]
      public void inGameClick_NoTargetForAttackWithinMoveRange_ReturnsMove()
      {
         string result;
         //board setup
         List<Character> ally = new List<Character>(), enemy = new List<Character>();
         Character c = new Character("ally");
         c.SetPosition(0, 0);
         c.SetDexterity(5);
         ally.Add(c);
         c = new Character("enemy");
         c.SetPosition(1, 1);
         enemy.Add(c);
         MapManager mm = new MapManager(ally);
         //Gets us down the true path of node 1
         mm.setState(1);
         //Down the false path of node 4
         mm.SelectUnitOnTile(0, 0);
         //Clicks on an empty tile that is not moveable
         result = mm.MapClick(1, 0);
         //Test
         Assert.AreEqual(result, "deselect");
      }

      [TestMethod]
      public void InGameClick_ValidMove_ReturnMove()
      { 
         string result;
         //board setup
         List<Character> ally = new List<Character>(), enemy = new List<Character>();
         Character c = new Character("ally");
         c.SetPosition(0, 0);
         c.SetDexterity(5);
         ally.Add(c);
        
         MapManager mm = new MapManager(ally);
         //Gets us down the true path of node 1
         mm.setState(1);
         //set the tile to be movable
         mm.GetTile(1,0).isMovable = true;
         //set the tile to be passable
         mm.GetTile(1,0).isPassable = true;
         //set the character to move
         mm.GetCharOnTile(0,0).isAbleToMove = true;
         //Down the false path of node 4
         mm.SelectUnitOnTile(0,0);      
         //Clicks on an empty tile that is movable
         result = mm.MapClick(1,0);
       
         //Test
         Assert.AreEqual(result, "move");
      }

      [TestMethod]
      public void InGameClick_InValidMoveOrAttack_ReturnSelect()
      { 
         string result;
         //board setup
         List<Character> ally = new List<Character>(), enemy = new List<Character>();
         Character c = new Character("ally");
         c.SetPosition(0, 0);
         c.SetDexterity(5);
         ally.Add(c);
         c = new Character("ally");
         c.SetPosition(1, 1);
         ally.Add(c);
         MapManager mm = new MapManager(ally);
         //Gets us down the true path of node 1
         mm.setState(1);
         //Down the false path of node 4
         mm.SelectUnitOnTile(0,0);
         //Clicks on an tile that ally is located
         result = mm.MapClick(1,1);
        
         //Test
         Assert.AreEqual(result, "select");
      }

      
   }
  
   
}
