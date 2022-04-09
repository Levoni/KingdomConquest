using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace KingdomConquest_Android
{
   public class Button
   {
      public Texture2D Texture { get; set; }
      public float x { get; set; }
      public float y { get; set; }
      private SpriteFont font;
      public string text { get; set; }
      private Color color;
      private bool isMouseOnButton;

      public Button(float x, float y, Texture2D texture, SpriteFont spriteFont = null, string btnText = null, Color? txtColor = null)
      {
         this.x = x;
         this.y = y;
         this.Texture = texture;
         font = spriteFont;
         text = btnText;
         color = txtColor ?? Color.White;
      }

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

      public bool HasMouseEntered()
      {
         Vector2 state = TouchPanel.GetState()[0].Position;
         if (!isMouseOnButton && state.X > x && state.Y > y && state.X < x + Texture.Width && state.Y < y + Texture.Height)
         {
            isMouseOnButton = true;
            return true;
         }
         return false;
      }

      public bool CanClick()
      {
         return isMouseOnButton;
      }

      public bool HasMouseLeft()
      {
         Vector2 state = TouchPanel.GetState()[0].Position;
         if (isMouseOnButton && (state.X <= x || state.Y <= y || state.X >= x + Texture.Width || state.Y >= y + Texture.Height))
         {
            isMouseOnButton = false;
            return true;
         }
         return false;
      }

      public float GetWidth()
      {
         return Texture.Width;
      }

      public float GetHeight()
      {
         return Texture.Height;
      }
   }
}
