using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{
    public class NameGenerator
    {

      Random rand;
      string[] nameList = {"Merek","Carac","Ulric","Tybalt","Borin","Sadon","Rowan","Fendrel", "Brom","Hadrian","Oliver",
"Joseph","Geoffrey","William","Simon","John","Edmund","Charles","Benedict","Gregory","Peter","Henry","Walter","Thomas","Arthur",
"Bryce","Donald","Lief","Barda","Rulf","Robin","Gavin","Terryn","Ronald","Jarin","Cassius","Leo","Cedric","Gavin",
"Peyton","Josef","Janshai","Doran","Asher","Quinn","Zane","Favian","Dain","Berinon","Tristan","Gorvena"};

      public NameGenerator()
      {
         rand = new Random();
      }

      public string GenerateName()
      {
         int index = rand.Next() % nameList.Length;
         string name = nameList[index];
         return name;
      }

    }
}
