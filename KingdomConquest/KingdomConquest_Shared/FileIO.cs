using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KingdomConquest_Shared
{

   class FileIO
   {
      string filePath = "";
      public FileIO()
      {

      }

      public FileIO(string path)
      {
         filePath = path;
      }

      public void SetFilePath(string path)
      {
         filePath = path;
      }

      public bool IsArmySaved()
      {
         return File.Exists(filePath + "army.txt");
      }

      public bool IsBattleSaved()
      {
         return File.Exists(filePath + "battle.txt");
      }

      public bool DeleteArmySave()
      {
         if (File.Exists(filePath + "army.txt"))
         {
            File.Delete(filePath + "army.txt");
            return true;
         }
         return false;
      }

      public bool DeleteBattleSave()
      {
         if (File.Exists(filePath + "battle.txt"))
         {
            File.Delete(filePath + "battle.txt");
            return true;
         }
         return false;
      }

      public bool SaveBattle(string battleInfo)
      {
         string path = filePath + "battle.txt";

         string save = battleInfo;



         //actual writing
         WriteLine(battleInfo, path);

         return true;
      }

      public bool SaveArmy(string armyInfo)
      {
         string path = filePath + "army.txt";

         string save = armyInfo;

         //actual writing
         WriteLine(armyInfo, path);

         return true;
      }

      public MapManager LoadBattle()
      {
         string path = filePath + "battle.txt";
         string tempString;

         tempString = ReadLine(path);

         MapManager tempManager = ConstructMapManager(tempString);

         return tempManager;
      }

      public Army LoadArmy()
      {
         string path = filePath + "army.txt";
         string tempString;

         tempString = ReadLine(path);

         Army tempArmy = ConstructArmy(tempString);

         return tempArmy;

      }

      public List<Character> LoadEnemyCharList(string mapName = "sampleMap")
      {
         string[] spliter;
         try
         {
            spliter = ReadLine(filePath + mapName + ".txt").Split(',');
         }
         catch { return new List<Character>(); }
         return readCharList(spliter);
      }

      private MapManager ConstructMapManager(string battleInfo)
      {
         //spliting the string of information
         string[] one = battleInfo.Split(';');
         string[][] master = new string[one.Length][];
         for (int i = 0; i < one.Length; i++)
         {
            master[i] = one[i].Split(',');
         }
         List<Character> aList = readCharList(master[1]);
         List<Character> eList = readCharList(master[2]);

         MapManager tempManager = new MapManager(aList, eList, master[0][0].Split(':')[2]);

         string[] battleState = master[0][0].Split(':');

         int bTurn = int.Parse(battleState[0]);
         tempManager.setState(int.Parse(battleState[1]));
         if (bTurn == 1)
            tempManager.IncrimentTurn();
         return tempManager;
      }

      public Map GenerateMapFromFile(string mapName = "sampleMap")
      {
         string path = filePath + mapName + ".tmx";
         string line;
         int width = 0;
         int height = 0;
         StreamReader sw;
         using (sw = new StreamReader(path))
         {
            line = sw.ReadLine();

            while (!sw.EndOfStream && line.Trim() != "<data encoding=\"csv\">")
            {
               line = sw.ReadLine();
               if (line.Substring(0, 13) == " <layer name=")
               {
                  string[] splitter = line.Split(' ');
                  foreach (string s in splitter)
                  {
                     if (s.Length > 4 && s.Substring(0, 5) == "width")
                     {
                        string[] tempArray = s.Split('\"');
                        width = int.Parse(tempArray[1]);
                     }
                     if (s.Length > 4 && s.Substring(0, 5) == "heigh")
                     {
                        string[] tempArray = s.Split('\"');
                        height = int.Parse(tempArray[1]);
                     }
                  }
               }
            }
            Map map = new Map(width, height);
            for (int i = 0; i < height; i++)
            {
               line = sw.ReadLine();
               string[] tiles = line.Split(',');
               for (int j = 0; j < width; j++)
               {
                  if (tiles[j] == "3")
                     map.GetTile(j, i).isPassable = false;
                  else
                     map.GetTile(j, i).isPassable = true;
               }
            }
            string infoPath = filePath + mapName + "Info" + ".txt";
            using (StreamReader Reader = new StreamReader(infoPath))
            {
               string tempString = Reader.ReadLine();
               string[] SplitArray = tempString.Split('|');
               map.maxUnitsPerTeam = int.Parse(SplitArray[0]);
               for (int i = 0; i < int.Parse(SplitArray[1]); i++)
               {
                  map.addStartLocation(int.Parse(SplitArray[2 + i * 2]), int.Parse(SplitArray[3 + i * 2]));
               }

            }
            map.mapName = mapName;
            return map;
         }
      }

      private Army ConstructArmy(string armyInfo)
      {
         string[] splitter = armyInfo.Split(';');
         Army tempArmy = new Army();
         tempArmy.AddFunds(int.Parse(splitter[0]));
         List<Character> charList = readCharList(splitter[1].Split(','));
         foreach (Character c in charList)
         {
            tempArmy.AddChar(c);
         }

         return tempArmy;
      }

      public List<Character> readCharList(string[] characterList)
      {
         Character c;
         List<Character> tempCList = new List<Character>();
         foreach (string cString in characterList)
         {
            if (cString != "")
            {
               c = ReadChar(cString);
               tempCList.Add(c);
            }
         }

         return tempCList;
      }

      private List<Character> readCharList(string[] characterList, Army a)
      {
         Character c;
         List<Character> tempCList = new List<Character>();
         foreach (string cString in characterList)
         {
            if (cString != "")
            {
               c = ReadChar(cString);
               if (a.SwitchUnit(c)) ;
               tempCList.Add(c);
            }
         }

         return tempCList;
      }

      private void WriteLine(string info, string filePath)
      {
         using (StreamWriter sw = new StreamWriter(filePath))
         {
            sw.WriteLine(info);
         }
      }

      private string ReadLine(string filePath)
      {
         string info;
         using (StreamReader sw = new StreamReader(filePath))
         {
            //to do: make this read multiple lines if there are any
            info = sw.ReadLine();
         }
         return info;
      }

      private Character ReadChar(string cString)
      {
         string[] splitter;
         splitter = cString.Split(':');
         Character c;
         if (splitter[14] == "Melee")
            c = new Character(splitter[0], 0);
         else if (splitter[14] == "Ranged")
            c = new Character(splitter[0], 1);
         else if (splitter[14] == "Magic")
            c = new Character(splitter[0], 2);
         else
            c = new Character(splitter[0]);

         c.X = int.Parse(splitter[1]);
         c.Y = int.Parse(splitter[2]);
         c.isAlive = bool.Parse(splitter[3]);
         c.isAbleToMove = bool.Parse(splitter[4]);
         c.isAbleToAttack = bool.Parse(splitter[5]);
         c.health = int.Parse(splitter[6]);
         c.maxHealth = int.Parse(splitter[7]);
         c.strength = int.Parse(splitter[8]);
         c.dexterity = int.Parse(splitter[9]);
         c.physicalResistance = int.Parse(splitter[10]);
         c.magicResistance = int.Parse(splitter[11]);
         c.level = int.Parse(splitter[12]);
         c.experience = int.Parse(splitter[13]);
         return c;
      }
   }
}
