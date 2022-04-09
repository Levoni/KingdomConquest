using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
   public class Tile
   {
      int TerrainType;
      public bool isPassable { get; set; }
      public bool isAttackable { get; set; }
      public bool isMovable { get; set; }
      public bool isHighlighted { get; set; }

      /// <summary>
      /// Constructor
      /// </summary>
      public Tile()
      {
         isPassable = false;
         isMovable = false;
         isAttackable = false;
         isHighlighted = false;
         TerrainType = 0;
      }

      public void ResetTempInformation()
      {
         isMovable = false;
         isAttackable = false;
         isHighlighted = false;
      }

   }
}
