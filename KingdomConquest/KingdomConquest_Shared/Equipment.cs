using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomConquest_Shared
{
   public class Equipment
   {
      //Stats for equipment
      public int strength { get; set; }
      public int dexterity { get; set; }
      public int physicalResistance { get; set; }
      public int magicalResistance { get; set; }

      //Information for catogorizing
      public string name { get; set; }
      public string equipmentClass { get; set; }
      public int gearType { get; set; }

      //Status indicators
      public bool isEquiped { get; set; }
      public bool valid { get; set; }

      /// <summary>
      /// Constructor
      /// </summary>
      public Equipment()
      {
         //setting stats
         strength = 0;
         dexterity = 0;
         physicalResistance = 0;
         magicalResistance = 0;

         //catogory information
         name = "";

         //setting indicators
         isEquiped = false;
         valid = true;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="n">name of equipment</param>
      public Equipment(string n)
      {
         //setting stats
         strength = 0;
         dexterity = 0;
         physicalResistance = 0;
         magicalResistance = 0;

         //catogory information
         name = n;

         //setting indicators
         isEquiped = false;
         valid = true;
      }

      /// <summary>
      /// Sets stats for the weapon
      /// </summary>
      /// <param name="stats">array of stats</param>
      protected void SetStats(int[] stats)
      {
         strength = stats[0];
         dexterity = stats[1];
         physicalResistance = stats[3];
         magicalResistance = stats[4];
      }

      /// <summary>
      /// Returns The equipment type
      /// </summary>
      /// <returns>equipment type as a string</returns>
      public virtual string GetEquipmentType()
      {
         return "none";
      }
   }
}
