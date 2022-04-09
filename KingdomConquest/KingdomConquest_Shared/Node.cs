using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomConquest_Shared
{

   public enum NodeState { Untested, Open, Closed }
   // Used for the pathfinding function
   class Node
   {
      public Tile T { get; set; }
      public int X { get; set; }
      public int Y { get; set; }
      public int G { get; set; } //distance from start
      public int H { get; set; } //distance from end
      public int F { get { return G + H; } } //distance form start and end combined
      public NodeState state { get; set; }
      public Node parentNode { get; set; }

      public Node(int xpos, int ypos, Node pNode)
      {
         X = xpos;
         Y = ypos;
         G = 0;
         H = 0;
         state = NodeState.Untested;
         parentNode = pNode;
         if (pNode != null)
            T = pNode.T;
      }

      public Node(int xpos, int ypos, Node pNode, Tile t)
      {
         X = xpos;
         Y = ypos;
         G = 0;
         H = 0;
         state = NodeState.Untested;
         parentNode = pNode;
         T = t;
      }
   }
}
