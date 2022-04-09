using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace KingdomConquest_Android
{
   /// <summary>
   /// A flexible button class that handles mouse input to detect button click, entrance, and exit
   /// events. It also supports the changing of images, displaying of text, and centers the text in
   /// the button.
   /// </summary>
   public class ListBoxItem
   {
      public Texture2D Texture { get; set; }
      public float x { get; set; }
      public float y { get; set; }
      private SpriteFont font;
      public string text { get; set; }
      private Color color;
      public object obj { get; set; }

      /// <summary>
      /// The constructor which takes in parameters to construct the button, defaulting some optional ones
      /// </summary>
      /// <param name="x">The x position of the upper left corner of the button</param>
      /// <param name="y">The y position of the upper left corner of the button</param>
      /// <param name="texture">The texture to be displayed</param>
      /// <param name="spriteFont">The spritefont to be used for text(optional)</param>
      /// <param name="btnText">The text to be displayed (optional)</param>
      /// <param name="txtColor">The color to show the text in(optional)</param>
      public ListBoxItem(float x, float y, Texture2D texture, SpriteFont spriteFont = null, string btnText = null, Color? txtColor = null)
      {
         this.x = x;
         this.y = y;
         this.Texture = texture;
         font = spriteFont;
         text = btnText;
         color = txtColor ?? Color.White;
         obj = null;
      }

      /// <summary>
      /// Handles the drawing of the button and the text. The text will be centered in the button
      /// </summary>
      /// <param name="spriteBatch">the spritebatch object to which it should draw</param>
      public void Draw(SpriteBatch spriteBatch)
      {
         spriteBatch.Draw(Texture, new Vector2(x, y), Color.White);
         if (font != null)
         {
            Vector2 textPos = new Vector2(x + (Texture.Width / 2) - (font.MeasureString(text).X / 2),
               y + (Texture.Height / 2) - (font.MeasureString(text).Y / 2));
            spriteBatch.DrawString(font, text, textPos, color);
         }
      }

      /// <summary>
      /// Determines if the button can currently be clicked
      /// </summary>
      /// <returns>True if the mouse is inside the button, false otherwise</returns>
      public bool CanClick()
      {
         Vector2 state = TouchPanel.GetState()[0].Position;
         return (state.X > x && state.Y > y && state.X < x + Texture.Width && state.Y < y + Texture.Height);
      }

      /// <summary>
      /// Returns the width of the button, determined by the texture used
      /// </summary>
      /// <returns>The width of the button</returns>
      public float GetWidth()
      {
         return Texture.Width;
      }

      /// <summary>
      /// Returns the height of the button, determined by the texture used
      /// </summary>
      /// <returns>The height of the button</returns>
      public float GetHeight()
      {
         return Texture.Height;
      }
   }
}
