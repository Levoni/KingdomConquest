using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace KingdomConquest_Desktop
{
   /// <summary>
   /// A flexible button class that handles mouse input to detect button click, entrance, and exit
   /// events. It also supports the changing of images, displaying of text, and centers the text in
   /// the button.
   /// </summary>
   public class ListBox
   {
      public Texture2D Texture { get; set; }
      public float x { get; set; }
      public float y { get; set; }
      private List<ListBoxItem> items;
      public Vector2 Size { get; set; }
      private bool isMouseOnList;
      public int SelectedItem { get; set; }
      public Texture2D SelectedTexture { get; set; }
      public object SelectedObj
      {
         get
         {
            if (SelectedItem != -1)
               return items[SelectedItem].obj;
            return null;
         }
      }

      /// <summary>
      /// The constructor which takes in parameters to construct the button, defaulting some optional ones
      /// </summary>
      /// <param name="x">The x position of the upper left corner of the button</param>
      /// <param name="y">The y position of the upper left corner of the button</param>
      /// <param name="texture">The texture to be displayed</param>
      /// <param name="spriteFont">The spritefont to be used for text(optional)</param>
      /// <param name="btnText">The text to be displayed (optional)</param>
      /// <param name="txtColor">The color to show the text in(optional)</param>
      public ListBox(float x, float y, Texture2D texture, List<ListBoxItem> list = null)
      {
         this.x = x;
         this.y = y;
         this.Texture = texture;
         if (list == null)
            items = new List<ListBoxItem>();
         else
            items = list;
         Size = new Vector2(Texture.Width, Texture.Height);
         SelectedItem = -1;
      }

      /// <summary>
      /// Handles the drawing of the button and the text. The text will be centered in the button
      /// </summary>
      /// <param name="spriteBatch">the spritebatch object to which it should draw</param>
      public void Draw(SpriteBatch spriteBatch)
      {
         spriteBatch.Draw(Texture, new Rectangle(new Point((int)x, (int)y), Size.ToPoint()), Color.White);
         float pos = y + 5;
         foreach (ListBoxItem i in items)
         {
            i.x = x + 5;
            i.y = pos;
            Texture2D original = i.Texture;
            if (SelectedItem >= 0 && SelectedItem < items.Count)
               if (i == items[SelectedItem])
                  i.Texture = SelectedTexture;
            i.Draw(spriteBatch);
            pos += i.GetHeight() + 5;
            i.Texture = original;
         }
      }

      public void Click()
      {
         foreach (ListBoxItem i in items)
         {
            if (i.CanClick())
            {
               SelectedItem = items.IndexOf(i);
               return;
            }
         }
         SelectedItem = -1;
      }

      /// <summary>
      /// Determines if the button can currently be clicked
      /// </summary>
      /// <returns>True if the mouse is inside the button, false otherwise</returns>
      public bool CanClick()
      {
         return isMouseOnList;
      }

      /// <summary>
      /// Determines if the mouse has entered the button since the last update
      /// Also sets to true the boolean determining if the button is clickable
      /// </summary>
      /// <returns>True if the mouse entered the button, false otherwise</returns>
      public bool HasMouseEntered()
      {
         MouseState state = Mouse.GetState();
         if (!isMouseOnList && state.X > x && state.Y > y && state.X < x + Size.X &&
            state.Y < y + Size.Y)
         {
            isMouseOnList = true;
            return true;
         }
         return false;
      }

      /// <summary>
      /// Determines if the mouse has left the button since the last update
      /// Also sets to false the boolean determining if the button is clickable
      /// </summary>
      /// <returns>True if the mouse left the button, false otherwise</returns>
      public bool HasMouseLeft()
      {
         MouseState state = Mouse.GetState();
         if (isMouseOnList && (state.X <= x || state.Y <= y || state.X >= x + Size.X ||
            state.Y >= y + Size.Y))
         {
            isMouseOnList = false;
            return true;
         }
         return false;
      }

      private float SumPos(int index)
      {
         float sum = 0f;
         if (index < items.Count)
            for (int i = 0; i <= index; i++)
            {
               sum += items[i].GetHeight() + 5f;
            }
         return sum;
      }

      public void ScrollDown()
      {
         if (items.Count > 0)
         {
            if (SumPos(items.Count - 1) > GetHeight())
            {
               items.Add(items[0]);
               items.RemoveAt(0);
               if (SelectedItem != -1)
               {
                  SelectedItem = (SelectedItem - 1) % items.Count;
                  if (SelectedItem == -1)
                     SelectedItem = items.Count - 1;
               }
            }
         }
      }

      public void ScrollUp()
      {
         if (items.Count > 0)
         {
            if (SumPos(items.Count - 1) > GetHeight())
            {
               items.Insert(0, items[items.Count - 1]);
               items.RemoveAt(items.Count - 1);
               if (SelectedItem != -1)
                  SelectedItem = (SelectedItem + 1) % items.Count;
            }
         }
      }

      public void SetWidth(float width)
      {
         Size = new Vector2(width, Size.Y);
      }

      public void SetHeight(float height)
      {
         Size = new Vector2(Size.X, height);
      }

      public void Add(ListBoxItem item)
      {
         items.Add(item);
      }

      public ListBoxItem Remove(int index)
      {
         if (index >= 0 && index < items.Count)
         {
            ListBoxItem item = items[index];
            items.RemoveAt(index);
            SelectedItem = -1;
            return item;
         }
         else
            return null;
      }

      /// <summary>
      /// Returns the width of the button, determined by the texture used
      /// </summary>
      /// <returns>The width of the button</returns>
      public float GetWidth()
      {
         return Size.X;
      }

      /// <summary>
      /// Returns the height of the button, determined by the texture used
      /// </summary>
      /// <returns>The height of the button</returns>
      public float GetHeight()
      {
         return Size.Y;
      }
   }
}
