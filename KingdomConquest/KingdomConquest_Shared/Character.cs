using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomConquest_Shared
{
   public enum charClassType
   {
      Melee,
      Ranged,
      Magic
   }


   public class Character
   {
      //Class Specific data arrays
      //Level up Format: Health, Strength,dexterity,physicalResistance,magicalResistance
      public int[] levelUp { get; set; }
      public int[] statRequiremts { get; set; }
      public string[] startingEquipment { get; set; }
      public string[] equipmentClasses { get; set; }

      //Base stats
      public int maxHealth { get; set; }
      public int strength { get; set; }
      public int dexterity { get; set; }
      public int physicalResistance { get; set; }
      public int magicResistance { get; set; }
      public int luck { get; set; }
      public int movementRange { get; set; }
      public int attackRange { get; set; }

      //Calculated stats
      public int health { get; set; }
      protected int initiative;

      //Charater info
      public int level { get; set; }
      public string name { get; set; }
      public int experience { get; set; }
      public charClassType charClass { get; set; }
      public int X { get; set; }
      public int Y { get; set; }

      //statuses
      public bool isAlive { get; set; }
      public bool isLeader { get; set; }
      public bool isAbleToMove { get; set; }
      public bool isAbleToAttack { get; set; }

      //Equipment
      //Format: 1-Weapon, 2-headGear, 3-chestGear, 4-boots, 5-charm
      protected Equipment[] Gear = new Equipment[5];

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="n">name of character in form of string</param>
      /// <param name="characaterType">0 = melee, 1 = Ranged, 2 = Magic</param>
      public Character(string n, int characaterType = 0)
      {
         maxHealth = 100;
         health = maxHealth;
         strength = 50;
         dexterity = 50;
         physicalResistance = 0;
         magicResistance = 0;
         luck = 50;
         movementRange = 3;
         attackRange = 1;
         initiative = 5;
         experience = 0;
         isAlive = true;
         isLeader = false;
         isAbleToMove = true;
         isAbleToAttack = true;
         levelUp = new int[] { 4, 3, 3, 3, 3 };
         name = n;
         if (characaterType == 0)
         {
            charClass = charClassType.Melee;
            movementRange = 3;
            attackRange = 1;
            physicalResistance = 25;
            levelUp = new int[] { 5, 4, 3, 5, 0 };
         }
         if (characaterType == 1)
         {
            charClass = charClassType.Ranged;
            movementRange = 3;
            attackRange = 3;
            physicalResistance = 10;
            levelUp = new int[] { 5, 4, 4, 3, 0 };
         }
         if (characaterType == 2)
         {
            charClass = charClassType.Magic;
            movementRange = 3;
            attackRange = 3;
            magicResistance = 25;
            levelUp = new int[] { 4, 5, 4, 0, 5 };
         }
         X = -1;
         Y = -1;
      }

      /// <summary>
      /// Sets a position for char, currently it
      /// is a tile x and y position
      /// </summary>
      /// <param name="x">X-coordinate</param>
      /// <param name="y">Y-coordinate</param>
      public void SetPosition(int x, int y)
      {
         X = x;
         Y = y;
         isAbleToMove = false;
      }

      /// <summary>
      /// takes an opponents level and determines xp gain for this
      /// character.
      /// </summary>
      /// <param name="level">level of opponent</param>
      /// <returns>xp gain</returns>
      public int GenerateExperienceGain(int olevel)
      {
         int gain = 0;
         if (olevel >= level + 3)
            gain = 100;
         else if (olevel == level + 2)
            gain = 50;
         else if (olevel == level + 1)
            gain = 25;
         else if (olevel == level)
            gain = 15;
         else if (olevel == level - 1)
            gain = 10;
         else if (olevel <= level - 2)
            gain = 5;
         return gain;
      }

      /// <summary>
      /// Increases experience and levels up the character if it has 
      /// enough xp.
      /// </summary>
      /// <param name="xp">amount to increase</param>
      public void GainExperience(int xp)
      {
         experience += xp;
         if (experience > 100)
         {
            LevelUp();
            experience = 0;
         }
      }

      /// <summary>
      /// Copy constructor
      /// </summary>
      /// <param name="c"></param>
      public Character(Character c)
      {
         level = c.level;
         health = c.health;
         strength = c.strength;
         dexterity = c.dexterity;
         experience = c.experience;
         name = c.name;
         isAlive = c.isAlive;
         isLeader = c.isLeader;
         charClass = c.charClass;
      }
      /// <summary>
      /// Removes all the equipment on the character
      /// </summary>
      public void RemoveAllEquipment()
      {
         for (int i = 0; i < Gear.Length; i++)
            EquipGear(i, null);
      }

      /// <summary>
      /// Equip a new weapon and returns the previously equiped weapon
      /// </summary>
      /// <param name="gearType">Equipment Type in array</param>
      /// <param name="e">Equipment (new weapon)</param>
      public void EquipGear(int gearType, Equipment e)
      {
         if (Gear[gearType] != null)
         {
            Gear[gearType].isEquiped = false;
         }
         Gear[gearType] = e;
         if (Gear[gearType] != null)
            Gear[gearType].isEquiped = true;
      }

      /// <summary>
      /// Caculates strength bonus from all equipment
      /// </summary>
      /// <returns>sum of Equipment strength</returns>
      private int CalculatEquipmentStrength()
      {
         int tempInt = 0;
         for (int i = 0; i < Gear.Length; i++)
            if (Gear[i] != null)
               tempInt += Gear[i].strength;
         return tempInt;
      }

      /// <summary>
      /// Caculates dexterity bonus from all equipment
      /// </summary>
      /// <returns>sum of equipment dexterity</returns>
      private int CalculatEquipmentDexterity()
      {
         int tempInt = 0;
         for (int i = 0; i < Gear.Length; i++)
            if (Gear[i] != null)
               tempInt += Gear[i].dexterity;
         return tempInt;
      }

      /// <summary>
      /// Caculates physical resistance bonus from all equipment
      /// </summary>
      private int CalculatEquipmentPysicalResistance()
      {
         int tempInt = 0;
         for (int i = 0; i < Gear.Length; i++)
            if (Gear[i] != null)
               tempInt += Gear[i].physicalResistance;
         return tempInt;
      }

      /// <summary>
      /// Caculates magical resistance bonus from all equipment
      /// </summary>
      private int CalculatEquipmentMagicalResistance()
      {
         int tempInt = 0;
         for (int i = 0; i < Gear.Length; i++)
            if (Gear[i] != null)
               tempInt += Gear[i].magicalResistance;
         return tempInt;
      }

      /// <summary>
      /// Determins if this character hits or misses another character
      /// based on both characters dexterity and random chance.
      /// </summary>
      /// <param name="opponentDexterity">opponents dexterity</param>
      /// <param name="rand">a random number (random chance)</param>
      /// <returns></returns>
      public virtual bool GetHitOrMiss(int opponentDexterity, int rand)
      {
         float chance = 80;
         float difference = dexterity / opponentDexterity;
         chance = chance * difference;
         if (chance < 40)
            chance = 40;
         return (chance > (rand % 100));
      }

      /// <summary>
      /// Takes Damage based on an imcoming attack, physicalResistance, and active 
      /// (de)buffs
      /// </summary>
      /// <param name="attack">the damage of incoming attack</param>
      /// <returns>the final calculated damage</returns>
      public virtual int RecivePhysicalAttack(int attack)
      {
         int tempDam = attack - physicalResistance;
         if (tempDam > 0)
         {
            health -= tempDam;
         }
         if (health <= 0)
         {
            isAlive = false;
            health = 0;
         }
         return tempDam;
      }

      /// <summary>
      /// Takes Damage based on an imcoming attack, physicalResistance, and active 
      /// (de)buffs
      /// </summary>
      /// <param name="attack">the damage of incoming attack</param>
      /// <returns>the final calculated damage</returns>
      public virtual int ReciveMagicalAttack(int attack)
      {
         int tempDam = attack - magicResistance;
         if (tempDam > 0)
         {
            health -= tempDam;
         }
         if (health <= 0)
         {
            isAlive = false;
            health = 0;
         }
         return tempDam;
      }

      /// <summary>
      /// attacks another character
      /// </summary>
      /// <param name="reciever">Character being attacked</param>
      public virtual void attack(Character reciever, bool hit = true)
      {
         if (hit)
         {
            if (charClass == charClassType.Magic)
               reciever.ReciveMagicalAttack(GetAttack());
            else
               reciever.RecivePhysicalAttack(GetAttack());
         }
         isAbleToAttack = false;
         isAbleToMove = false;
      }

      /// <summary>
      /// Sets health equal to characters maximum health
      /// </summary>
      public void ResetHealth()
      {
         health = maxHealth;
      }

      /// <summary>
      /// Increases character's stats based on class.
      /// </summary>
      public void LevelUp()
      {
         level++;
         maxHealth += levelUp[0];
         strength += levelUp[1];
         dexterity += levelUp[2];
         physicalResistance += levelUp[3];
         magicResistance += levelUp[4];
      }

      /// <summary>
      /// Increases a specific stat by a specific amount
      /// </summary>
      /// <param name="stat">0=maxhealth,1=strenght,2=dexterity,3=PResistance,4=MResistance</param>
      /// <param name="amount">amount to increase the stat by</param>
      public void IncreaseStat(int stat, int amount)
      {
         if (stat == 0)
         {
            maxHealth += amount;
         }
         if (stat == 1)
         {
            strength += amount;
         }
         if (stat == 2)
         {
            dexterity += amount;
         }
         if (stat == 3)
         {
            physicalResistance += amount;
         }
         if (stat == 4)
         {
            magicResistance += amount;
         }
      }

      /// <summary>
      /// Returns base initiative
      /// </summary>
      /// <returns>the characters base initiative</returns>
      public int GetBaseInitiative()
      {
         return initiative;
      }

      /// <summary>
      /// Returns name of Class's Special
      /// </summary>
      /// <returns>Specials name (string)</returns>
      public virtual string GetSpecialOneName()
      {
         return "None";
      }

      /// <summary>
      /// Activates class's special
      /// </summary>
      public virtual void SpecialOne()
      {

      }

      /// <summary>
      /// Ticks (de)buff
      /// </summary>
      public virtual void tick()
      {

      }

      /// <summary>
      /// Checks if character is the same as another, based on name
      /// </summary>
      /// <param name="c">Character to chack</param>
      /// <returns>if the characters are same</returns>
      public bool Equals(Character c)
      {
         return name == c.name;
      }

      /// <summary>
      /// Heals character by set amount
      /// </summary>
      /// <param name="h">Amount to heal character</param>
      public void Heal(int h)
      {
         health += h;
         if (health > maxHealth)
         {
            health = maxHealth;
         }
      }

      /// <summary>
      /// Returns calculated attack
      /// </summary>
      /// <returns>the attack</returns>
      public int GetAttack()
      {
         return strength + CalculatEquipmentStrength(); ;
      }

      /// <summary>
      /// Returns calculated dexterity
      /// </summary>
      public virtual int GetDexterity()
      {
         return dexterity + CalculatEquipmentDexterity();
      }

      /// <summary>
      /// Returns base dexterity
      /// </summary>
      public int GetBaseDexterity()
      {
         return dexterity;
      }

      /// <summary>
      /// Returns calculated strength
      /// </summary>
      public int GetStrength()
      {
         return strength + CalculatEquipmentStrength();
      }

      /// <summary>
      /// Returns Base strength
      /// </summary>
      public int GetBaseStrength()
      {
         return strength;
      }

      /// <summary>
      /// Returns equipment's name
      /// </summary>
      /// <param name="gearType">1=weapon,2=headgear,3=chestgear,4=boots,5=charm</param>
      public string GetEquipmentName(int gearType)
      {
         return Gear[gearType].name;
      }

      /// <summary>
      /// Returns weapon
      /// </summary>
      /// <param name="gearType">1=weapon,2=headgear,3=chestgear,4=boots,5=charm</param>
      public Equipment GetGear(int gearType)
      {
         return Gear[gearType];
      }

      /// <summary>
      /// sets base Strength
      /// </summary>
      /// <param name="amount">amount to increase</param>
      public void SetStrength(int amount)
      {
         strength = amount;
      }

      /// <summary>
      /// Sets base dexterity
      /// </summary>
      /// <param name="amount">amount to increase</param>
      public void SetDexterity(int amount)
      {
         dexterity = amount;
      }

      /// <summary>
      /// Sets base physical resistance
      /// </summary>
      /// <param name="amout"> amount to increase</param>
      public void SetPhysicalResistance(int amount)
      {
         physicalResistance = amount;
      }

      /// <summary>
      /// Sets base magical resistance
      /// </summary>
      /// <param name="amount">amount to increase</param>
      public void SetMagicalResistance(int amount)
      {
         magicResistance = amount;
      }

      /// <summary>
      /// Sets level
      /// </summary>
      /// <param name="l">level to set to</param>
      public void SetLevel(int l)
      {
         level = l;
      }

      /// <summary>
      /// Sets base initiative
      /// </summary>
      /// <param name="i">initiative to set to</param>
      public void SetInitiative(int i)
      {
         initiative = i;
      }

      /// <summary>
      /// Returns the Charaters information as a string
      /// seperated by :
      /// </summary>
      /// <returns>character information as string</returns>
      public override string ToString()
      {
         string info = name + ":" + X.ToString() + ":" + Y.ToString() + ":" + isAlive + ":" + isAbleToMove + ":" + isAbleToAttack + ":" + health + ":"
            + maxHealth.ToString() + ":" + strength.ToString() + ":" + dexterity.ToString() + ":" + physicalResistance.ToString() + ":" + magicResistance.ToString() + ":"
            + level.ToString() + ":" + experience.ToString() + ":" + charClass.ToString();
         return info;
      }
   }
}
