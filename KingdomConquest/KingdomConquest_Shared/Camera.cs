using Microsoft.Xna.Framework;
using System;

namespace KingdomConquest_Shared
{
   /// <summary>
   /// Creates a camera that supports the viewing of a map at various zoom levels, positions,
   /// and rotations, as chosen by the user. This camera is for returning the transformation
   /// matirx into the spritebatch object, which is how the camera actually takes effect
   /// </summary>
   class Camera
   {
      public int mapWidth = 30; //The number of tiles across the map from left to right
      public int mapHeight = 30;//The number of tiles across the map from top to bottom
      public const int PIXELS_ACROSS_TILE = 100; //The number of pixels per tile, which assumes tiles are square
      //public const int BORDER_BEYOND_MAP = 50; //The number of pixels around the outside of the map you can see,
                                               //to ensure you can see the entire character sprites
      public Vector2 Position { get; private set; }
      public float Zoom { get; private set; }
      public float Rotation { get; private set; }
      public Vector2 ViewportSize { get; set; } // The size of the viewable window area

      /// <summary>
      /// A constructor to initialize a camera with a specified zoom, which defaults to 1
      /// </summary>
      /// <param name="zoom">A float describing the initial zoom level of the camera</param>
      public Camera(float zoom = 1.0f)
      {
         Zoom = zoom;
      }

      /// <summary>
      /// Returns the position of the center of the viewable window area in camera coordinates
      /// </summary>
      public Vector2 ViewportCenter
      {
         get
         {
            return new Vector2(ViewportSize.X / 2, ViewportSize.Y / 2);
         }
      }

      /// <summary>
      /// Returns the transformation matrix, which is a combination of the translation, scale,
      /// and rotation operations done to the camera, which is used to correctly draw and position
      /// everything in the world, in relation to the camera
      /// </summary>
      public Matrix TransformationMat
      {
         get
         {
            return Matrix.CreateTranslation(
               -(int)Position.X, -(int)Position.Y, 0) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
               Matrix.CreateTranslation(new Vector3(ViewportCenter.X, ViewportCenter.Y, 0));
         }
      }

      /// <summary>
      /// Increases or decreases the amount of zoom additively.
      /// </summary>
      /// <param name="zoom">The adjustment that will be applied to the zoom level</param>
      public void ModifyZoom(float zoom)
      {
         Zoom += zoom;
         if (Zoom < .2f)
            Zoom =  .2f;
         if (Zoom > 2f)
            Zoom = 2f;
         Move(new Vector2(0, 0));
      }

      /// <summary>
      /// Changes the position of the camera additively, so it essentially adjusts the camera
      /// position by the given vector2, as long as it is within the map
      /// It also has necessary logic to keep the camera within the map, and if the player zooms
      /// outside the map to view the whole thing at once, it will center the map
      /// </summary>
      /// <param name="move">The vector used to adjust the current position</param>
      public void Move(Vector2 move)
      {
         Vector2 newPos = Position + move;
         //max represents the maximum the camera should be able to move to the right and down,
         //without seeing anything out of the map
         Vector2 max = new Vector2(mapWidth * PIXELS_ACROSS_TILE - (ViewportSize.X / Zoom / 2),
            mapHeight * PIXELS_ACROSS_TILE - (ViewportSize.Y / Zoom / 2));
         //Keeps the camera inside the last two vectors, which make sure the camera doesn't see outside the map
         Position = Vector2.Clamp(newPos, new Vector2(ViewportSize.X / Zoom / 2,
            ViewportSize.Y / Zoom / 2), max);
         //This check is in case the map is smaller than the screen in both the x and y directions, in
         //which case it is centered in both directions
         if (mapWidth * PIXELS_ACROSS_TILE * Zoom < ViewportSize.X &&
            mapHeight * PIXELS_ACROSS_TILE * Zoom < ViewportSize.Y)
            Position = ScreenToWorld(new Vector2(mapWidth * PIXELS_ACROSS_TILE * Zoom / 2,
               mapHeight * PIXELS_ACROSS_TILE * Zoom / 2));
         //This check is for if the screen is tall enough, but not wide enough for the map, which can happen
         //on many displays these days. It will center with regaurds to x in this case, and allow the user
         //to manuever in the y direction
         else if (mapWidth * PIXELS_ACROSS_TILE * Zoom < ViewportSize.X)
            Position = ScreenToWorld(new Vector2(mapWidth * PIXELS_ACROSS_TILE * Zoom / 2,
               WorldToScreen(Position).Y));
         //This check is for if the screen is wide enough, but not tall enough for the map.
         //It will center with regaurds to y in this case, and allow the user
         //to manuever in the x direction
         else if (mapHeight * PIXELS_ACROSS_TILE * Zoom < ViewportSize.Y)
            Position = ScreenToWorld(new Vector2(WorldToScreen(Position).X,
               mapHeight * PIXELS_ACROSS_TILE * Zoom / 2));
      }

      /// <summary>
      /// Returns the rectangle surrounding the viewable map area
      /// </summary>
      /// <returns>The rectangle surrounding the map area</returns>
      public Rectangle WorldBoundary()
      {
         Vector2 corner = ScreenToWorld(new Vector2(0, 0));
         Vector2 bottomCorner = ScreenToWorld(new Vector2(ViewportSize.X, ViewportSize.Y));
         return new Rectangle((int)corner.X, (int)corner.Y, (int)(bottomCorner.X - corner.X),
            (int)(bottomCorner.Y - corner.Y));
      }

      /// <summary>
      /// Converts screen coordinates to world coordinates, useful for using the mouse with the camera
      /// </summary>
      /// <param name="pos">The vector to translate</param>
      /// <returns>The translated vector</returns>
      public Vector2 ScreenToWorld(Vector2 pos)
      {
         return Vector2.Transform(pos, Matrix.Invert(TransformationMat));
      }

      /// <summary>
      /// Converts world coordinates to screen coordinates, useful for woking with the game using a mouse
      /// </summary>
      /// <param name="pos">The vector to translate</param>
      /// <returns>The translated vector</returns>
      public Vector2 WorldToScreen(Vector2 pos)
      {
         return Vector2.Transform(pos, TransformationMat);
      }
   }
}
