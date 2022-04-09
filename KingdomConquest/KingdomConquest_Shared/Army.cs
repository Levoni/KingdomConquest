using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
   public class Army
   {
      public List<Character> CharList { get; private set; }
      List<Equipment> EquipList;
      public int gold { get; private set; }

      /// <summary>
      /// Default Constructor
      /// </summary>
      public Army()
      {
         CharList = new List<Character>();
         EquipList = new List<Equipment>();
         gold = 0;
         //StubArmy();
      }

      private void StubArmy()
      {
         Character C = new Character("one");
         CharList.Add(C);
         C = new Character("two");
         CharList.Add(C);
         C = new Character("three");
         CharList.Add(C);
         C = new Character("four");
         CharList.Add(C);
         C = new Character("five");
         CharList.Add(C);
         C = new Character("six");
         CharList.Add(C);
      }

      /// <summary>
      /// creates a character then adds it to the character list
      /// </summary>
      /// <param name="name">name of the character</param>
      /// <param name="type">0=melee,1=ranged,2=magic</param>
      public void AddChar(string name, int type)
      {
         Character c = new Character(name, type);
         if (!CheckForDuplicate(c))
            CharList.Add(c);
      }

      /// <summary>
      /// Adds a precreated character to the army
      /// </summary>
      /// <param name="c">Character to add</param>
      public bool AddChar(Character c)
      {
         if (!CheckForDuplicate(c))
         {
            CharList.Add(c);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Removes a Character from the army
      /// </summary>
      /// <param name="c">Character to remove</param>
      /// <returns>true if removed false if not</returns>
      public bool RemoveChar(Character c)
      {
         return CharList.Remove(c);
      }

      /// <summary>
      /// Removes a Character based on name
      /// </summary>
      /// <param name="cName">name of character</param>
      /// <returns>true if removed false if not</returns>
      public bool RemoveChar(string cName)
      {
         for (int i = 0; i < CharList.Count; i++)
         {
            if (CharList[i].name.ToLower() == cName.ToLower())
            {
               CharList.RemoveAt(i);
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Equips a character with a piece of equipment from the 
      /// army
      /// </summary>
      /// <param name="c">Character to equip</param>
      /// <param name="equip">name of equipment in army to equip</param>
      public void EquipChar(Character c, string equip)
      {
         Equipment e = GetAvailableEquipment(equip);
         c.EquipGear(e.gearType, e);
      }

      /// <summary>
      /// Adds a precreated equipment to the army
      /// </summary>
      /// <param name="e">Equipment to add</param>
      public bool AddEquip(Equipment e)
      {
         if (e.valid)
         {
            EquipList.Add(e);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Creates a equipment and then adds it to the 
      /// army
      /// </summary>
      /// <param name="eName">name of equipment</param>
      /// <returns>true if added false if not</returns>
      public bool AddEquip(string eName)
      {
         Equipment e = new Equipment(eName);
         if (e.valid)
         {
            EquipList.Add(e);
            return true;
         }
         return false;
      }

      /// <summary>
      /// removes a equipment from army
      /// </summary>
      /// <param name="e">equipment to remove</param>
      /// <returns>true if removed false if not</returns>
      public bool RemoveEquip(Equipment e)
      {
         if (!e.isEquiped)
         {
            return EquipList.Remove(e);
         }
         return false;
      }

      /// <summary>
      /// remove a equipment form army
      /// </summary>
      /// <param name="eName"></param>
      /// <returns></returns>
      public bool RemoveEquip(string eName)
      {
         for (int i = 0; i < EquipList.Count; i++)
         {
            if (!EquipList[i].isEquiped && EquipList[i].name == eName)
            {
               EquipList.RemoveAt(i);
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Checks if character is already in the charList
      /// </summary>
      /// <param name="c">Character to check</param>
      /// <returns>true if duplicate false if not</returns>
      private bool CheckForDuplicate(Character c)
      {
         foreach (Character character in CharList)
         {
            if (c.Equals(character))
               return true;
         }
         return false;
      }

      /// <summary>
      /// Adds a amount to the money the army has
      /// </summary>
      /// <param name="amount">amount to add</param>
      public void AddFunds(int amount)
      {
         gold += amount;
      }

      /// <summary>
      /// Finds Character in army
      /// </summary>
      /// <param name="name">name of character</param>
      /// <returns>character if found null if not</returns>
      public Character FindChar(string name)
      {
         foreach (Character c in CharList)
         {
            if (c.name == name)
               return c;
         }
         return null;
      }

      /// <summary>
      /// Finds first available equipment
      /// </summary>
      /// <param name="name">name of equipment</param>
      /// <returns>equipmetn if found null if not</returns>
      public Equipment GetAvailableEquipment(string name)
      {
         foreach (Equipment e in EquipList)
         {
            if (e.name == name && !e.isEquiped)
               return e;
         }
         return null;
      }

      /// <summary>
      /// combines all the armys information into a string to save
      /// </summary>
      /// <returns></returns>
      public string GetSaveDataString()
      {
         string info = string.Empty;
         info += gold.ToString() + ";";
         for (int i = 0; i < CharList.Count; i++)
         {
            Character c = CharList[i];
            info += c.ToString();
            if (i != CharList.Count - 1)
               info += ",";
         }


         return info;
      }

      public bool SwitchUnit(Character c)
      {
         for (int i = 0; i < CharList.Count; i++)
         {
            if (CharList[i].Equals(c))
            {
               CharList[i] = c;
               return true;
            }
         }
         return false;
      }
   }
}
