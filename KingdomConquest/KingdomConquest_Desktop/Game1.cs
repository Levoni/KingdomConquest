using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KingdomConquest_Shared;
using System.Collections;
using System.Collections.Generic;
using System;
using forms = System.Windows.Forms;
using System.IO;

namespace KingdomConquest_Desktop
{
   /// <summary>
   /// This is the main type for your game.
   /// </summary>
   public class Game1 : Game
   {
      private static char[] allowedUsernameChars = new char[]
         { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C',
            'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
            'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '9', '8', '7', '6', '5', '4',
            '3', '2', '1' };
      private static char[] allowedKeyChars = allowedUsernameChars;
      private static char[] allowedIPChars = new char[]
         { '0', '9', '8', '7', '6', '5', '4','3', '2', '1', '.', ':' };
      private static int UNIT_COST = 100;
      private GraphicsDeviceManager graphics;
      private SpriteBatch spriteBatch;
      private float screenHeight, screenWidth;
      private Texture2D background, map_texture, tile_highlight, tile_outline, stats_hud;
      private Button btnSingleplayer, btnMultiplayer, btnExit, btnLoad, btnShop;
      private Button btnMainMenu;
      private Button lArrowSoldier, rArrowSoldier, lArrowMage,
         rArrowMage, lArrowArcher, rArrowArcher, btnStart;
      private Button btnSelect, btnRemove;
      private int numSoldiers, numArchers, numMages, attackStage;
      private SpriteFont btnFont, gameOverFont, statsFont;
      private Hashtable btnTextures, characterTextures;
      private static Camera camera = new Camera();
      private int scrollVal;
      GameManager gm;
      private bool drawGrid, highlightMovement, gameOver;
      private Character selectedChar, movingChar, attackingChar, hoveredChar;
      MouseState prevMouse;
      KeyboardState prevKeyboard;
      private Vector2 movingPlayerLoc;
      private Vector2 attackLoc;
      private Character attackedChar;
      private Vector2 statsPos;
      private TextBox txtIP, txtKey, txtUsername, selectedTxt;
      private ListBox lstArmy, lstSelected;
      private Texture2D lstItemTexture;
      private string mapName = "";
      private int mapWidth = 0;
      private int mapHeight = 0;
      private bool singlePlayer = true;

      /// <summary>
      /// A constructor that initializes the game and some initial parameters
      /// </summary>
      public Game1()
      {
         graphics = new GraphicsDeviceManager(this);
         //We want to find a better way to do this, so that the window is monitor size or something
         graphics.PreferredBackBufferWidth = 1100;
         graphics.PreferredBackBufferHeight = 700;
         Window.AllowUserResizing = true;
         Window.ClientSizeChanged += Window_ClientSizeChanged;
         IsMouseVisible = true;
         //Don't use this for development, because it is really slow, and prevents single monitor debugging
         //graphics.IsFullScreen = true;
         Content.RootDirectory = "Content";
      }

      /// <summary>
      /// Allows the game to perform any initialization it needs to before starting to run.
      /// This is where it can query for any required services and load any non-graphic
      /// related content.  Calling base.Initialize will enumerate through any components
      /// and initialize them as well.
      /// </summary>
      protected override void Initialize()
      {
         // TODO: Add your initialization logic here
         screenHeight = graphics.GraphicsDevice.Viewport.Height;
         screenWidth = graphics.GraphicsDevice.Viewport.Width;
         camera.ViewportSize = new Vector2(screenWidth, screenHeight);
         btnTextures = new Hashtable();
         characterTextures = new Hashtable();
         scrollVal = 0;
         gm = new GameManager(null);
         drawGrid = true;
         highlightMovement = false;
         prevMouse = new MouseState();
         prevKeyboard = new KeyboardState();
         movingChar = hoveredChar = attackedChar = null;
         gameOver = false;
         movingPlayerLoc = attackLoc = new Vector2(-1, -1);
         numArchers = numMages = numSoldiers = attackStage = 0;
         base.Initialize();
      }

      /// <summary>
      /// LoadContent will be called once per game and is the place to load
      /// all of your content.
      /// </summary>
      protected override void LoadContent()
      {
         // Create a new SpriteBatch, which can be used to draw textures.
         spriteBatch = new SpriteBatch(GraphicsDevice);
         UnloadContent();
         switch (gm.GS)
         {
            case GameState.MainMenu:
               LoadMainMenu();
               break;
            case GameState.Multiplayer:
               LoadMultiplayer();
               break;
            case GameState.MultiplayerMenu:
               LoadMultiplayerMenu();
               break;
            case GameState.Singleplayer:
               LoadSingleplayer();
               break;
            case GameState.CharacterSelection:
               LoadCharSelection();
               break;
            case GameState.Shop:
               LoadShop();
               break;
            case GameState.GameOver:
               LoadGameOver();
               break;
            case GameState.MapSelection:
               LoadMapSelection();
               break;
         }
      }

      /// <summary>
      /// Loads the resources used on the game over screen
      /// </summary>
      private void LoadGameOver()
      {
         string initialBtnImageName = "buttonStock1";
         background = Content.Load<Texture2D>("map-with-castle");
         btnFont = Content.Load<SpriteFont>("btnFont");
         gameOverFont = Content.Load<SpriteFont>("gameOverFont");
         btnTextures.Add(initialBtnImageName, Content.Load<Texture2D>(initialBtnImageName));
         btnTextures.Add(initialBtnImageName + "h", Content.Load<Texture2D>(initialBtnImageName + "h"));
         btnTextures.Add(initialBtnImageName + "d", Content.Load<Texture2D>(initialBtnImageName + "d"));
         btnMainMenu = new Button(0, 75 + gameOverFont.MeasureString("Game   Over").Y,
            (Texture2D)btnTextures[initialBtnImageName], btnFont, "Main Menu");
         btnExit = new Button(0, 75 + gameOverFont.MeasureString("Game   Over").Y,
            (Texture2D)btnTextures[initialBtnImageName], btnFont, "Exit");
      }

      /// <summary>
      /// Loads the resources used on the multiplayer screen
      /// </summary>
      private void LoadMultiplayer()
      {
         background = Content.Load<Texture2D>("map-edited");
         map_texture = Content.Load<Texture2D>("sampleMap");
         //tile_outline = Content.Load<Texture2D>("TileOutline");
         tile_outline = new Texture2D(graphics.GraphicsDevice, 1, 1);
         tile_outline.SetData(new Color[] { Color.White });
         tile_highlight = Content.Load<Texture2D>("tileHighlight");
         characterTextures.Add("0b", Content.Load<Texture2D>("0b"));
         characterTextures.Add("0r", Content.Load<Texture2D>("0r"));
         characterTextures.Add("0bmage", Content.Load<Texture2D>("0bmage"));
         characterTextures.Add("0rmage", Content.Load<Texture2D>("0rmage"));
         characterTextures.Add("0barcher", Content.Load<Texture2D>("0barcher"));
         characterTextures.Add("0rarcher", Content.Load<Texture2D>("0rarcher"));
         stats_hud = Content.Load<Texture2D>("stats_hud");
         statsPos = Vector2.Zero;
         statsFont = Content.Load<SpriteFont>("statsFont");
         camera.Move(new Vector2(background.Width / 2, background.Height / 2));
      }

      /// <summary>
      /// Loads the resources used on the multiplayer menu screen
      /// </summary>
      private void LoadMultiplayerMenu()
      {
         string initialBtnImageName = "buttonStock1";
         background = Content.Load<Texture2D>("map-with-castle");
         Texture2D texture = Content.Load<Texture2D>("textBoxWood");
         btnFont = Content.Load<SpriteFont>("btnFont");
         txtIP = new TextBox(0, 0, texture, btnFont, "", Color.White, allowedIPChars);
         txtKey = new TextBox(0, 0, texture, btnFont, "", Color.White, allowedKeyChars);
         txtUsername = new TextBox(0, 0, texture, btnFont, "", Color.White, allowedUsernameChars);
         btnTextures.Add(initialBtnImageName, Content.Load<Texture2D>(initialBtnImageName));
         btnTextures.Add(initialBtnImageName + "h", Content.Load<Texture2D>(initialBtnImageName + "h"));
         btnTextures.Add(initialBtnImageName + "d", Content.Load<Texture2D>(initialBtnImageName + "d"));
         btnStart = new Button(0, 0, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Connect", Color.White);
         btnMainMenu = new Button(0, 0, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Main Menu");
         selectedTxt = txtUsername;
      }

      private void LoadCharSelection()
      {
         statsFont = Content.Load<SpriteFont>("charSelectFont");
         btnFont = Content.Load<SpriteFont>("btnFont");
         stats_hud = Content.Load<Texture2D>("listBackground");
         background = Content.Load<Texture2D>("woodBackground");
         Texture2D paper = Content.Load<Texture2D>("listBackground");
         lstArmy = new ListBox(0, 0, paper);
         lstSelected = new ListBox(0, 0, paper);
         lstItemTexture = Content.Load<Texture2D>("textBoxWood");
         lstArmy.SetWidth(lstItemTexture.Width + 10);
         lstSelected.SetWidth(lstItemTexture.Width + 10);
         ListBoxItem i;
         foreach (Character c in gm.MainArmy.CharList)
         {
            i = new ListBoxItem(0, 0, lstItemTexture, statsFont, "Name: " + c.name +
               " Level: " + c.level + " Class: " + c.charClass, Color.White);
            i.obj = c;
            lstArmy.Add(i);
         }
         string initialBtnImageName = "buttonStock1";
         btnTextures.Add(initialBtnImageName, Content.Load<Texture2D>(initialBtnImageName));
         btnTextures.Add(initialBtnImageName + "h", Content.Load<Texture2D>(initialBtnImageName + "h"));
         btnTextures.Add(initialBtnImageName + "d", Content.Load<Texture2D>(initialBtnImageName + "d"));
         btnTextures.Add("textBoxWood", lstItemTexture);
         btnTextures.Add("textBoxWoodh", Content.Load<Texture2D>("textBoxWoodh"));
         Texture2D tex = (Texture2D)btnTextures[initialBtnImageName];
         btnSelect = new Button(0, 0, tex, btnFont, "Select", Color.White);
         btnRemove = new Button(0, 0, tex, btnFont, "Remove", Color.White);
         btnStart = new Button(screenWidth - tex.Width - 5, 5, tex, btnFont, "Start", Color.White);
         btnMainMenu = new Button(5, 5, tex, btnFont, "Main Menu", Color.White);
      }

      private void LoadMapSelection()
      {
         btnFont = Content.Load<SpriteFont>("btnFont");
         background = Content.Load<Texture2D>("woodBackground");
         Texture2D paper = Content.Load<Texture2D>("listBackground");
         lstSelected = new ListBox(0, 0, paper);
         lstItemTexture = Content.Load<Texture2D>("textBoxWood");
         lstSelected.SetWidth(lstItemTexture.Width + 10);
         ListBoxItem i;
         string[] files = Directory.GetFiles("./map_files", "*.tmx", SearchOption.TopDirectoryOnly);
         foreach (string s in files)
         {
            string name = s.Split('\\')[1].Split('.')[0];
            i = new ListBoxItem(0, 0, lstItemTexture, btnFont, name, Color.White);
            i.obj = name;
            lstSelected.Add(i);
         }
         string initialBtnImageName = "buttonStock1";
         btnTextures.Add(initialBtnImageName, Content.Load<Texture2D>(initialBtnImageName));
         btnTextures.Add(initialBtnImageName + "h", Content.Load<Texture2D>(initialBtnImageName + "h"));
         btnTextures.Add(initialBtnImageName + "d", Content.Load<Texture2D>(initialBtnImageName + "d"));
         btnTextures.Add("textBoxWood", lstItemTexture);
         btnTextures.Add("textBoxWoodh", Content.Load<Texture2D>("textBoxWoodh"));
         Texture2D tex = (Texture2D)btnTextures[initialBtnImageName];
         btnStart = new Button(screenWidth - tex.Width - 5, 5, tex, btnFont, "Next", Color.White);
         btnMainMenu = new Button(5, 5, tex, btnFont, "Main Menu", Color.White);
      }

      /// <summary>
      /// Loads the resources used on the character selection menu screen
      /// </summary>
      private void LoadShop()
      {
         string initialBtnImageName = "_arrow";
         background = Content.Load<Texture2D>("map-edited");
         btnFont = Content.Load<SpriteFont>("btnFont");
         btnTextures.Add("l" + initialBtnImageName, Content.Load<Texture2D>("l" + initialBtnImageName));
         btnTextures.Add("r" + initialBtnImageName, Content.Load<Texture2D>("r" + initialBtnImageName));
         btnTextures.Add("l" + initialBtnImageName + "h", Content.Load<Texture2D>("l" + initialBtnImageName + "h"));
         btnTextures.Add("r" + initialBtnImageName + "h", Content.Load<Texture2D>("r" + initialBtnImageName + "h"));
         btnTextures.Add("l" + initialBtnImageName + "d", Content.Load<Texture2D>("l" + initialBtnImageName + "d"));
         btnTextures.Add("r" + initialBtnImageName + "d", Content.Load<Texture2D>("r" + initialBtnImageName + "d"));
         btnTextures.Add("l" + initialBtnImageName + "x", Content.Load<Texture2D>("l" + initialBtnImageName + "x"));
         btnTextures.Add("r" + initialBtnImageName + "x", Content.Load<Texture2D>("r" + initialBtnImageName + "x"));
         characterTextures.Add("0b", Content.Load<Texture2D>("0btall"));
         characterTextures.Add("0bmage", Content.Load<Texture2D>("0btallmage"));
         characterTextures.Add("0barcher", Content.Load<Texture2D>("0btallarcher"));
         lArrowArcher = new Button(0, 0, (Texture2D)btnTextures["l" + initialBtnImageName]);
         rArrowArcher = new Button(0, 0, (Texture2D)btnTextures["r" + initialBtnImageName]);
         lArrowMage = new Button(0, 0, (Texture2D)btnTextures["l" + initialBtnImageName]);
         rArrowMage = new Button(0, 0, (Texture2D)btnTextures["r" + initialBtnImageName]);
         lArrowSoldier = new Button(0, 0, (Texture2D)btnTextures["l" + initialBtnImageName]);
         rArrowSoldier = new Button(0, 0, (Texture2D)btnTextures["r" + initialBtnImageName]);
         initialBtnImageName = "buttonStock1";
         btnTextures.Add(initialBtnImageName, Content.Load<Texture2D>(initialBtnImageName));
         btnTextures.Add(initialBtnImageName + "h", Content.Load<Texture2D>(initialBtnImageName + "h"));
         btnTextures.Add(initialBtnImageName + "d", Content.Load<Texture2D>(initialBtnImageName + "d"));
         btnStart = new Button(0, 0, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Purchase");
         btnMainMenu = new Button(0, 0, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Main Menu");
      }

      /// <summary>
      /// Loads the resources used on the main menu screen
      /// </summary>
      private void LoadMainMenu()
      {
         string initialBtnImageName = "buttonStock1";
         background = Content.Load<Texture2D>("map-with-castle");
         btnFont = Content.Load<SpriteFont>("btnFont");
         btnTextures.Add(initialBtnImageName, Content.Load<Texture2D>(initialBtnImageName));
         btnTextures.Add(initialBtnImageName + "h", Content.Load<Texture2D>(initialBtnImageName + "h"));
         btnTextures.Add(initialBtnImageName + "d", Content.Load<Texture2D>(initialBtnImageName + "d"));
         btnSingleplayer = new Button(50, 50, (Texture2D)btnTextures[initialBtnImageName], btnFont, "New Game");
         btnLoad = new Button(50, 150, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Load Game");
         btnExit = new Button(50, 450, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Exit");
         btnMultiplayer = new Button(50, 250, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Multiplayer");
         btnShop = new Button(50, 350, (Texture2D)btnTextures[initialBtnImageName], btnFont, "Shop");
      }

      /// <summary>
      /// Loads the resources used on the singleplayer screen
      /// </summary>
      private void LoadSingleplayer()
      {
         background = Content.Load<Texture2D>("map-edited");
         map_texture = Content.Load<Texture2D>(mapName);
         //tile_outline = Content.Load<Texture2D>("TileOutline");
         tile_outline = new Texture2D(graphics.GraphicsDevice, 1, 1);
         tile_outline.SetData(new Color[] { Color.White });
         tile_highlight = Content.Load<Texture2D>("tileHighlight");
         characterTextures.Add("0b", Content.Load<Texture2D>("0b"));
         characterTextures.Add("0r", Content.Load<Texture2D>("0r"));
         characterTextures.Add("0bmage", Content.Load<Texture2D>("0bmage"));
         characterTextures.Add("0rmage", Content.Load<Texture2D>("0rmage"));
         characterTextures.Add("0barcher", Content.Load<Texture2D>("0barcher"));
         characterTextures.Add("0rarcher", Content.Load<Texture2D>("0rarcher"));
         stats_hud = Content.Load<Texture2D>("stats_hud");
         statsPos = Vector2.Zero;
         statsFont = Content.Load<SpriteFont>("statsFont");
         camera.Move(new Vector2(background.Width / 2, background.Height / 2));
      }

      /// <summary>
      /// UnloadContent will be called once per game and is the place to unload
      /// game-specific content.
      /// </summary>
      protected override void UnloadContent()
      {
         // TODO: Unload any non ContentManager content here
         switch (gm.GS)
         {
            case GameState.MainMenu:
               UnloadMainMenu();
               break;
            case GameState.Multiplayer:
               UnloadMultiplayer();
               break;
            case GameState.MultiplayerMenu:
               UnloadMultiplayerMenu();
               break;
            case GameState.Singleplayer:
               UnloadSingleplayer();
               break;
            case GameState.CharacterSelection:
               UnloadCharSelection();
               break;
            case GameState.Shop:
               UnloadShop();
               break;
            case GameState.GameOver:
               UnloadGameOver();
               break;
            case GameState.MapSelection:
               UnloadMapSelection();
               break;
         }
      }

      /// <summary>
      /// Unloads resources used in the main menu screen
      /// </summary>
      private void UnloadMainMenu()
      {
         btnTextures.Clear();
         Content.Unload();
      }

      /// <summary>
      /// Unloads resources used in the multiplayer screen
      /// </summary>
      private void UnloadMultiplayer()
      {
         characterTextures.Clear();
         Content.Unload();
      }

      /// <summary>
      /// Unloads resources used in the multiplayer menu screen
      /// </summary>
      private void UnloadMultiplayerMenu()
      {
         btnTextures.Clear();
         Content.Unload();
      }

      /// <summary>
      /// Unloads resources used in the singleplayer screen
      /// </summary>
      private void UnloadSingleplayer()
      {
         characterTextures.Clear();
         Content.Unload();
      }

      private void UnloadGameOver()
      {
         btnTextures.Clear();
         Content.Unload();
      }

      private void UnloadCharSelection()
      {
         btnTextures.Clear();
         Content.Unload();
      }

      private void UnloadMapSelection()
      {
         btnTextures.Clear();
         Content.Unload();
      }

      private void UnloadShop()
      {
         characterTextures.Clear();
         btnTextures.Clear();
         Content.Unload();
      }

      /// <summary>
      /// Allows the game to run logic such as updating the world,
      /// checking for collisions, gathering input, and playing audio.
      /// </summary>
      /// <param name="gameTime">Provides a snapshot of timing values.</param>
      protected override void Update(GameTime gameTime)
      {

         // TODO: Add your update logic here
         switch (gm.GS)
         {
            case GameState.MainMenu:
               UpdateMainMenu(gameTime);
               break;
            case GameState.Multiplayer:
               UpdateMultiplayer(gameTime);
               break;
            case GameState.MultiplayerMenu:
               UpdateMultiplayerMenu(gameTime);
               break;
            case GameState.serverLobby:
               string serverEvent = gm.SetUpMultiplayerGame("", "", "");
               if (serverEvent == "OPPONENT_FOUND")
               {
                  camera.mapWidth = mapWidth = gm.GetMapWidth();
                  camera.mapHeight = mapHeight = gm.GetMapHeight();
               }
               LoadContent();
               break;
            case GameState.Singleplayer:
               UpdateSingleplayer(gameTime);
               break;
            case GameState.GameOver:
               UpdateGameOver(gameTime);
               break;
            case GameState.CharacterSelection:
               UpdateCharSelection(gameTime);
               break;
            case GameState.Shop:
               UpdateShop(gameTime);
               break;
            case GameState.MapSelection:
               UpdateMapSelection(gameTime);
               break;
         }

         base.Update(gameTime);
      }

      /// <summary>
      /// All the checks done each update of the game, while on the
      /// main menu screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void UpdateMainMenu(GameTime gameTime)
      {
         MouseState state = Mouse.GetState();
         if (btnExit.HasMouseEntered())
            btnExit.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnShop.HasMouseEntered())
            btnShop.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnSingleplayer.HasMouseEntered())
            btnSingleplayer.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnMultiplayer.HasMouseEntered())
            btnMultiplayer.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnLoad.HasMouseEntered())
            btnLoad.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnExit.HasMouseLeft())
            btnExit.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnShop.HasMouseLeft())
            btnShop.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnSingleplayer.HasMouseLeft())
            btnSingleplayer.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnMultiplayer.HasMouseLeft())
            btnMultiplayer.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnLoad.HasMouseLeft())
            btnLoad.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (state.LeftButton == ButtonState.Pressed)
         {
            if (btnExit.CanClick())
            {
               btnExit.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
            else if (btnShop.CanClick())
            {
               btnShop.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
            else if (btnMultiplayer.CanClick())
            {
               btnMultiplayer.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
            else if (btnSingleplayer.CanClick())
            {
               btnSingleplayer.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
            else if (btnLoad.CanClick())
            {
               btnLoad.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
         }
         else if (prevMouse.LeftButton == ButtonState.Pressed && state.LeftButton == ButtonState.Released)
         {
            if (btnExit.CanClick())
            {
               Exit();
            }
            else if (btnShop.CanClick())
            {
               gm.GS = GameState.Shop;
               LoadContent();
            }
            else if (btnMultiplayer.CanClick())
            {
               singlePlayer = false;
               gm.GS = GameState.CharacterSelection;
               LoadContent();
            }
            else if (btnSingleplayer.CanClick())
            {
               singlePlayer = true;
               gm.GS = GameState.MapSelection;
               LoadContent();
            }
            else if (btnLoad.CanClick())
            {
               if (gm.IsBattleSave())
                  StartLoadedMap();
            }
         }
         prevMouse = state;
      }

      /// <summary>
      /// All the checks done each update of the game, while on the
      /// multiplayer screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void UpdateMultiplayer(GameTime gameTime)
      {
         KeyboardState state = Keyboard.GetState();
         Vector2 movement = Vector2.Zero;
         if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
            movement.X += -1;
         if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
            movement.X += 1;
         if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
            movement.Y += 1;
         if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
            movement.Y += -1;
         if (movement != Vector2.Zero)
            camera.Move(movement * 25f);
         if (state.IsKeyUp(Keys.Space) && prevKeyboard.IsKeyDown(Keys.Space))
         {
            if (statsPos == Vector2.Zero)
               statsPos = new Vector2(screenWidth, 0);
            else if (statsPos == new Vector2(screenWidth, 0))
               statsPos = new Vector2(screenWidth, screenHeight);
            else if (statsPos == new Vector2(screenWidth, screenHeight))
               statsPos = new Vector2(0, screenHeight);
            else
               statsPos = Vector2.Zero;
         }
         MouseState mouse = Mouse.GetState();
         if (mouse.ScrollWheelValue < scrollVal)
         {
            camera.ModifyZoom(-.1f);
         }
         else if (mouse.ScrollWheelValue > scrollVal)
         {
            camera.ModifyZoom(.1f);
         }
         scrollVal = mouse.ScrollWheelValue;

         Vector2 worldPos = camera.ScreenToWorld(mouse.Position.ToVector2());
         int x = (int)worldPos.X / Camera.PIXELS_ACROSS_TILE;
         int y = (int)worldPos.Y / Camera.PIXELS_ACROSS_TILE;
         hoveredChar = gm.GetCharOnTile(x, y);

         if (prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
         {
            string gameEvent = "";
            if (x > 0 && x < mapWidth && y > 0 && y < mapHeight)
            {
               gameEvent = gm.TileClick(x, y);
            }
            switch (gameEvent)
            {
               case "Set":
                  break;
               case "initialized":
                  drawGrid = false;
                  break;
               case "select":
                  ResetAllTileHighlights();
                  drawGrid = true;
                  selectedChar = gm.GetCharOnTile(x, y);
                  if (selectedChar.isAbleToMove || selectedChar.isAbleToAttack)
                  {
                     //HighlightMovementRange(selectedChar.GetBaseInitiative(), selectedChar.X, selectedChar.Y);
                     highlightMovement = true;
                  }
                  else
                  {
                     highlightMovement = false;
                  }
                  break;
               case "deselect":
                  drawGrid = false;
                  highlightMovement = false;
                  break;
               case "move":
                  drawGrid = false;
                  highlightMovement = false;
                  movingChar = selectedChar;
                  drawGrid = false;
                  break;
               case "attack":
                  drawGrid = false;
                  highlightMovement = false;
                  attackingChar = selectedChar;
                  if (gm.GetPath().Count > 1)
                     movingChar = selectedChar;
                  break;
            }
         }
         if (gm.GetBattleTurn() != 0)
         {
            string gameEvent = gm.EnemyAction();
            switch (gameEvent)
            {
               case "Set":
                  break;
               case "initialized":
                  drawGrid = false;
                  break;
               case "select":
                  ResetAllTileHighlights();
                  drawGrid = true;
                  //selectedChar = gm.GetCharOnTile(x, y);
                  if (selectedChar.isAbleToMove || selectedChar.isAbleToAttack)
                  {
                     //HighlightMovementRange(selectedChar.GetBaseInitiative(), selectedChar.X, selectedChar.Y);
                     highlightMovement = true;
                  }
                  else
                  {
                     highlightMovement = false;
                  }
                  break;
               case "deselect":
                  drawGrid = false;
                  highlightMovement = false;
                  break;
               case "move":
                  drawGrid = false;
                  highlightMovement = false;
                  selectedChar = gm.GetCharOnTile(gm.GetPath()[0].X, gm.GetPath()[0].Y);
                  movingChar = selectedChar;
                  drawGrid = false;
                  break;
               case "attack":
                  drawGrid = false;
                  highlightMovement = false;
                  selectedChar = gm.GetCharOnTile(gm.GetPath()[0].X, gm.GetPath()[0].Y);
                  attackingChar = selectedChar;
                  if (gm.GetPath().Count > 1)
                     movingChar = selectedChar;
                  break;
               case "disconnect":
                  LoadContent();
                  break;
            }
         }
         prevKeyboard = state;
         prevMouse = mouse;
      }

      private void HandleTextInput(Keys[] keys, HashSet<Keys> prevPressedKeys)
      {
         foreach (Keys key in keys)
         {
            if (!prevPressedKeys.Contains(key))
            {
               if (key == Keys.Back)
               {
                  if (selectedTxt.text.Length > 1)
                     selectedTxt.text = selectedTxt.text.Remove(selectedTxt.text.Length - 2, 1);
               }
               else
               {
                  string text = "";
                  if (forms.Control.ModifierKeys == forms.Keys.Shift)
                  {
                     if (key == Keys.A)
                        text = "A";
                     else if (key == Keys.B)
                        text = "B";
                     else if (key == Keys.C)
                        text = "C";
                     else if (key == Keys.D)
                        text = "D";
                     else if (key == Keys.E)
                        text = "E";
                     else if (key == Keys.F)
                        text = "F";
                     else if (key == Keys.G)
                        text = "G";
                     else if (key == Keys.H)
                        text = "H";
                     else if (key == Keys.I)
                        text = "I";
                     else if (key == Keys.J)
                        text = "J";
                     else if (key == Keys.K)
                        text = "K";
                     else if (key == Keys.L)
                        text = "L";
                     else if (key == Keys.M)
                        text = "M";
                     else if (key == Keys.N)
                        text = "N";
                     else if (key == Keys.O)
                        text = "O";
                     else if (key == Keys.P)
                        text = "P";
                     else if (key == Keys.Q)
                        text = "Q";
                     else if (key == Keys.R)
                        text = "R";
                     else if (key == Keys.S)
                        text = "S";
                     else if (key == Keys.T)
                        text = "T";
                     else if (key == Keys.U)
                        text = "U";
                     else if (key == Keys.V)
                        text = "V";
                     else if (key == Keys.W)
                        text = "W";
                     else if (key == Keys.X)
                        text = "X";
                     else if (key == Keys.Y)
                        text = "Y";
                     else if (key == Keys.Z)
                        text = "Z";
                     else if (key == Keys.OemSemicolon)
                        text = ":";
                  }
                  else
                  {
                     if (key == Keys.OemPeriod
                     || key == Keys.Decimal)
                        text = ".";
                     else if (key == Keys.D0 ||
                        key == Keys.NumPad0)
                        text = "0";
                     else if (key == Keys.D1 ||
                        key == Keys.NumPad1)
                        text = "1";
                     else if (key == Keys.D2 ||
                        key == Keys.NumPad2)
                        text = "2";
                     else if (key == Keys.D3 ||
                        key == Keys.NumPad3)
                        text = "3";
                     else if (key == Keys.D4 ||
                        key == Keys.NumPad4)
                        text = "4";
                     else if (key == Keys.D5 ||
                        key == Keys.NumPad5)
                        text = "5";
                     else if (key == Keys.D6 ||
                        key == Keys.NumPad6)
                        text = "6";
                     else if (key == Keys.D7 ||
                        key == Keys.NumPad7)
                        text = "7";
                     else if (key == Keys.D8 ||
                        key == Keys.NumPad8)
                        text = "8";
                     else if (key == Keys.D9 ||
                        key == Keys.NumPad9)
                        text = "9";
                     else if (key == Keys.A)
                        text = "a";
                     else if (key == Keys.B)
                        text = "b";
                     else if (key == Keys.C)
                        text = "c";
                     else if (key == Keys.D)
                        text = "d";
                     else if (key == Keys.E)
                        text = "e";
                     else if (key == Keys.F)
                        text = "f";
                     else if (key == Keys.G)
                        text = "g";
                     else if (key == Keys.H)
                        text = "h";
                     else if (key == Keys.I)
                        text = "i";
                     else if (key == Keys.J)
                        text = "j";
                     else if (key == Keys.K)
                        text = "k";
                     else if (key == Keys.L)
                        text = "l";
                     else if (key == Keys.M)
                        text = "m";
                     else if (key == Keys.N)
                        text = "n";
                     else if (key == Keys.O)
                        text = "o";
                     else if (key == Keys.P)
                        text = "p";
                     else if (key == Keys.Q)
                        text = "q";
                     else if (key == Keys.R)
                        text = "r";
                     else if (key == Keys.S)
                        text = "s";
                     else if (key == Keys.T)
                        text = "t";
                     else if (key == Keys.U)
                        text = "u";
                     else if (key == Keys.V)
                        text = "v";
                     else if (key == Keys.W)
                        text = "w";
                     else if (key == Keys.X)
                        text = "x";
                     else if (key == Keys.Y)
                        text = "y";
                     else if (key == Keys.Z)
                        text = "z";
                  }
                  if (text != "")
                     selectedTxt.HandleTextInput(text.ToCharArray()[0]);
               }
            }
         }
      }

      /// <summary>
      /// All the checks done each update of the game, while on the
      /// multiplayer menu screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void UpdateMultiplayerMenu(GameTime gameTime)
      {
         KeyboardState keyState = Keyboard.GetState();
         Keys[] keys = keyState.GetPressedKeys();
         HashSet<Keys> prevPressedKeys = new HashSet<Keys>();
         foreach (Keys key in prevKeyboard.GetPressedKeys())
            prevPressedKeys.Add(key);
         HandleTextInput(keys, prevPressedKeys);
         MouseState state = Mouse.GetState();
         if (btnMainMenu.HasMouseEntered())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnStart.HasMouseEntered())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnMainMenu.HasMouseLeft())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnStart.HasMouseLeft())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (state.LeftButton == ButtonState.Pressed)
         {
            if (btnMainMenu.CanClick())
               btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (btnStart.CanClick())
               btnStart.Texture = (Texture2D)btnTextures["buttonStock1d"];
         }
         else if (prevMouse.LeftButton == ButtonState.Pressed && state.LeftButton == ButtonState.Released)
         {
            if (btnMainMenu.CanClick())
            {
               gm.GS = GameState.MainMenu;
               LoadContent();
            }
            else if (btnStart.CanClick())
            {
               StartMultiplayer();
            }
            else if (txtIP.CanClick())
            {
               SetActiveTextbox(txtIP);
            }
            else if (txtKey.CanClick())
            {
               SetActiveTextbox(txtKey);
            }
            else if (txtUsername.CanClick())
            {
               SetActiveTextbox(txtUsername);
            }
         }
         prevMouse = state;
         prevKeyboard = keyState;
      }

      /// <summary>
      /// All the checks done each update of the game, while on the
      /// singleplayer screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void UpdateSingleplayer(GameTime gameTime)
      {
         KeyboardState state = Keyboard.GetState();
         Vector2 movement = Vector2.Zero;
         if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
            movement.X += -1;
         if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
            movement.X += 1;
         if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
            movement.Y += 1;
         if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
            movement.Y += -1;
         if (movement != Vector2.Zero)
            camera.Move(movement * 25f);
         if (state.IsKeyUp(Keys.Space) && prevKeyboard.IsKeyDown(Keys.Space))
         {
            if (statsPos == Vector2.Zero)
               statsPos = new Vector2(screenWidth, 0);
            else if (statsPos == new Vector2(screenWidth, 0))
               statsPos = new Vector2(screenWidth, screenHeight);
            else if (statsPos == new Vector2(screenWidth, screenHeight))
               statsPos = new Vector2(0, screenHeight);
            else
               statsPos = Vector2.Zero;
         }
         MouseState mouse = Mouse.GetState();
         if (mouse.ScrollWheelValue < scrollVal)
         {
            camera.ModifyZoom(-.1f);
         }
         else if (mouse.ScrollWheelValue > scrollVal)
         {
            camera.ModifyZoom(.1f);
         }
         scrollVal = mouse.ScrollWheelValue;

         Vector2 worldPos = camera.ScreenToWorld(mouse.Position.ToVector2());
         int x = (int)worldPos.X / Camera.PIXELS_ACROSS_TILE;
         int y = (int)worldPos.Y / Camera.PIXELS_ACROSS_TILE;
         hoveredChar = gm.GetCharOnTile(x, y);

         if (prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
         {
            string gameEvent = "";
            if (x > 0 && x < mapWidth && y > 0 && y < mapHeight)
            {
               gameEvent = gm.TileClick(x, y);
            }
            switch (gameEvent)
            {
               case "Set":
                  break;
               case "initialized":
                  drawGrid = false;
                  break;
               case "select":
                  ResetAllTileHighlights();
                  drawGrid = true;
                  selectedChar = gm.GetCharOnTile(x, y);
                  if (selectedChar.isAbleToMove || selectedChar.isAbleToAttack)
                  {
                     //HighlightMovementRange(selectedChar.GetBaseInitiative(), selectedChar.X, selectedChar.Y);
                     highlightMovement = true;
                  }
                  else
                  {
                     highlightMovement = false;
                  }
                  break;
               case "deselect":
                  drawGrid = false;
                  highlightMovement = false;
                  break;
               case "move":
                  drawGrid = false;
                  highlightMovement = false;
                  movingChar = selectedChar;
                  drawGrid = false;
                  break;
               case "attack":
                  drawGrid = false;
                  highlightMovement = false;
                  attackingChar = selectedChar;
                  if (gm.GetPath().Count > 1)
                     movingChar = selectedChar;
                  break;
            }
         }
         if (gm.GetBattleTurn() != 0)
         {
            string gameEvent = gm.EnemyAction();
            switch (gameEvent)
            {
               case "Set":
                  break;
               case "initialized":
                  drawGrid = false;
                  break;
               case "select":
                  ResetAllTileHighlights();
                  drawGrid = true;
                  //selectedChar = gm.GetCharOnTile(x, y);
                  if (selectedChar.isAbleToMove || selectedChar.isAbleToAttack)
                  {
                     //HighlightMovementRange(selectedChar.GetBaseInitiative(), selectedChar.X, selectedChar.Y);
                     highlightMovement = true;
                  }
                  else
                  {
                     highlightMovement = false;
                  }
                  break;
               case "deselect":
                  drawGrid = false;
                  highlightMovement = false;
                  break;
               case "move":
                  drawGrid = false;
                  highlightMovement = false;
                  selectedChar = gm.GetCharOnTile(gm.GetPath()[0].X, gm.GetPath()[0].Y);
                  movingChar = selectedChar;
                  drawGrid = false;
                  break;
               case "attack":
                  drawGrid = false;
                  highlightMovement = false;
                  selectedChar = gm.GetCharOnTile(gm.GetPath()[0].X, gm.GetPath()[0].Y);
                  attackingChar = selectedChar;
                  if (gm.GetPath().Count > 1)
                     movingChar = selectedChar;
                  break;
            }
         }
         prevKeyboard = state;
         prevMouse = mouse;
      }

      private void UpdateGameOver(GameTime gameTime)
      {
         MouseState state = Mouse.GetState();
         if (btnExit.HasMouseEntered())
            btnExit.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnMainMenu.HasMouseEntered())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnExit.HasMouseLeft())
            btnExit.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnMainMenu.HasMouseLeft())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (state.LeftButton == ButtonState.Pressed)
         {
            if (btnExit.CanClick())
            {
               btnExit.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
            else if (btnMainMenu.CanClick())
            {
               btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1d"];
            }
         }
         else if (prevMouse.LeftButton == ButtonState.Pressed && state.LeftButton == ButtonState.Released)
         {
            if (btnExit.CanClick())
            {
               Exit();
            }
            else if (btnMainMenu.CanClick())
            {
               gm.GS = GameState.MainMenu;
               LoadContent();
            }
         }
         prevMouse = state;
      }

      private void UpdateCharSelection(GameTime gameTime)
      {
         MouseState mouse = Mouse.GetState();
         lstArmy.HasMouseEntered();
         lstArmy.HasMouseLeft();
         lstSelected.HasMouseEntered();
         lstSelected.HasMouseLeft();
         if (mouse.ScrollWheelValue < scrollVal)
         {
            if (lstArmy.CanClick())
               lstArmy.ScrollDown();
            if (lstSelected.CanClick())
               lstSelected.ScrollDown();
         }
         else if (mouse.ScrollWheelValue > scrollVal)
         {
            if (lstArmy.CanClick())
               lstArmy.ScrollUp();
            if (lstSelected.CanClick())
               lstSelected.ScrollUp();
         }
         scrollVal = mouse.ScrollWheelValue;

         if (prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
         {
            if (lstArmy.CanClick())
            {
               if (lstArmy.SelectedItem != -1)
                  lstArmy.SelectedTexture = (Texture2D)btnTextures["textBoxWood"];
               lstArmy.Click();
               if (lstArmy.SelectedItem != -1)
                  lstArmy.SelectedTexture = (Texture2D)btnTextures["textBoxWoodh"];
            }
            if (lstSelected.CanClick())
            {
               if (lstSelected.SelectedItem != -1)
                  lstSelected.SelectedTexture = (Texture2D)btnTextures["textBoxWood"];
               lstSelected.Click();
               if (lstSelected.SelectedItem != -1)
                  lstSelected.SelectedTexture = (Texture2D)btnTextures["textBoxWoodh"];
            }
         }

         KeyboardState state = Keyboard.GetState();
         if (state.IsKeyUp(Keys.Down) && prevKeyboard.IsKeyDown(Keys.Down))
         {
            if (lstArmy.CanClick())
               lstArmy.ScrollDown();
            if (lstSelected.CanClick())
               lstSelected.ScrollDown();
         }
         if (state.IsKeyUp(Keys.Up) && prevKeyboard.IsKeyDown(Keys.Up))
         {
            if (lstArmy.CanClick())
               lstArmy.ScrollUp();
            if (lstSelected.CanClick())
               lstSelected.ScrollUp();
         }

         if (btnMainMenu.HasMouseEntered())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnStart.HasMouseEntered())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnMainMenu.HasMouseLeft())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnStart.HasMouseLeft())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnRemove.HasMouseEntered())
            btnRemove.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnSelect.HasMouseEntered())
            btnSelect.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnRemove.HasMouseLeft())
            btnRemove.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnSelect.HasMouseLeft())
            btnSelect.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (mouse.LeftButton == ButtonState.Pressed)
         {
            if (btnStart.CanClick())
               btnStart.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (btnMainMenu.CanClick())
               btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (btnRemove.CanClick())
               btnRemove.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (btnSelect.CanClick())
               btnSelect.Texture = (Texture2D)btnTextures["buttonStock1d"];
         }
         else if (prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
         {
            if (btnMainMenu.CanClick())
            {
               gm.GS = GameState.MainMenu;
               LoadContent();
            }
            else if (btnStart.CanClick())
            {
               ListBoxItem item = lstSelected.Remove(0);
               while (item != null)
               {
                  gm.AddToBattleList(((Character)item.obj).name);
                  item = lstSelected.Remove(0);
               }
               if (singlePlayer)
                  StartSingleplayer();
               else
               {
                  gm.GS = GameState.MultiplayerMenu;
                  LoadContent();
               }
            }
            else if (btnRemove.CanClick())
            {
               btnRemove.Texture = (Texture2D)btnTextures["buttonStock1h"];
               if (lstSelected.SelectedItem != -1)
               {
                  ListBoxItem item = lstSelected.Remove(lstSelected.SelectedItem);
                  if (item != null)
                     lstArmy.Add(item);
               }
            }
            else if (btnSelect.CanClick())
            {
               btnSelect.Texture = (Texture2D)btnTextures["buttonStock1h"];
               if (lstArmy.SelectedItem != -1)
               {
                  ListBoxItem item = lstArmy.Remove(lstArmy.SelectedItem);
                  if (item != null)
                     lstSelected.Add(item);
               }
            }
         }
         prevKeyboard = state;
         prevMouse = mouse;
      }

      private void UpdateMapSelection(GameTime gameTime)
      {
         MouseState mouse = Mouse.GetState();
         lstSelected.HasMouseEntered();
         lstSelected.HasMouseLeft();
         if (mouse.ScrollWheelValue < scrollVal)
         {
            if (lstSelected.CanClick())
               lstSelected.ScrollDown();
         }
         else if (mouse.ScrollWheelValue > scrollVal)
         {
            if (lstSelected.CanClick())
               lstSelected.ScrollUp();
         }
         scrollVal = mouse.ScrollWheelValue;

         if (prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
         {
            if (lstSelected.CanClick())
            {
               if (lstSelected.SelectedItem != -1)
                  lstSelected.SelectedTexture = (Texture2D)btnTextures["textBoxWood"];
               lstSelected.Click();
               if (lstSelected.SelectedItem != -1)
                  lstSelected.SelectedTexture = (Texture2D)btnTextures["textBoxWoodh"];
            }
         }

         KeyboardState state = Keyboard.GetState();
         if (state.IsKeyUp(Keys.Down) && prevKeyboard.IsKeyDown(Keys.Down))
         {
            if (lstSelected.CanClick())
               lstSelected.ScrollDown();
         }
         if (state.IsKeyUp(Keys.Up) && prevKeyboard.IsKeyDown(Keys.Up))
         {
            if (lstSelected.CanClick())
               lstSelected.ScrollUp();
         }

         if (btnMainMenu.HasMouseEntered())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnStart.HasMouseEntered())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnMainMenu.HasMouseLeft())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnStart.HasMouseLeft())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (mouse.LeftButton == ButtonState.Pressed)
         {
            if (btnStart.CanClick())
               btnStart.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (btnMainMenu.CanClick())
               btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1d"];
         }
         else if (prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
         {
            if (btnMainMenu.CanClick())
            {
               gm.GS = GameState.MainMenu;
               LoadContent();
            }
            else if (btnStart.CanClick())
            {
               ListBoxItem item = lstSelected.Remove(lstSelected.SelectedItem);
               if (item != null)
               {
                  btnStart.Texture = (Texture2D)btnTextures["buttonStock1h"];
                  gm.GS = GameState.CharacterSelection;
                  mapName = (string)item.obj;
                  LoadContent();
               }
            }
         }
         prevKeyboard = state;
         prevMouse = mouse;
      }

      private void UpdateShop(GameTime gameTime)
      {
         MouseState state = Mouse.GetState();
         //Handle the button entrance events for the main two buttons
         if (btnMainMenu.HasMouseEntered())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1h"];
         if (btnStart.HasMouseEntered())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1h"];
         //Handle the button entrance events for the six arrow buttons
         if (lArrowArcher.HasMouseEntered())
            lArrowArcher.Texture = (Texture2D)btnTextures["l_arrowh"];
         if (lArrowMage.HasMouseEntered())
            lArrowMage.Texture = (Texture2D)btnTextures["l_arrowh"];
         if (lArrowSoldier.HasMouseEntered())
            lArrowSoldier.Texture = (Texture2D)btnTextures["l_arrowh"];
         if (rArrowArcher.HasMouseEntered())
            rArrowArcher.Texture = (Texture2D)btnTextures["r_arrowh"];
         if (rArrowMage.HasMouseEntered())
            rArrowMage.Texture = (Texture2D)btnTextures["r_arrowh"];
         if (rArrowSoldier.HasMouseEntered())
            rArrowSoldier.Texture = (Texture2D)btnTextures["r_arrowh"];
         //Handle the button exit events for the two main buttons
         if (btnMainMenu.HasMouseLeft())
            btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1"];
         if (btnStart.HasMouseLeft())
            btnStart.Texture = (Texture2D)btnTextures["buttonStock1"];
         //Handle the button exit events for the six arrow buttons
         if (lArrowArcher.HasMouseLeft())
            lArrowArcher.Texture = (Texture2D)btnTextures["l_arrow"];
         if (lArrowMage.HasMouseLeft())
            lArrowMage.Texture = (Texture2D)btnTextures["l_arrow"];
         if (lArrowSoldier.HasMouseLeft())
            lArrowSoldier.Texture = (Texture2D)btnTextures["l_arrow"];
         if (rArrowArcher.HasMouseLeft())
            rArrowArcher.Texture = (Texture2D)btnTextures["r_arrow"];
         if (rArrowMage.HasMouseLeft())
            rArrowMage.Texture = (Texture2D)btnTextures["r_arrow"];
         if (rArrowSoldier.HasMouseLeft())
            rArrowSoldier.Texture = (Texture2D)btnTextures["r_arrow"];
         //Handle the button press events for all buttons
         if (state.LeftButton == ButtonState.Pressed)
         {
            if (btnStart.CanClick())
               btnStart.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (btnMainMenu.CanClick())
               btnMainMenu.Texture = (Texture2D)btnTextures["buttonStock1d"];
            else if (lArrowSoldier.CanClick())
               lArrowSoldier.Texture = (Texture2D)btnTextures["l_arrowd"];
            else if (lArrowMage.CanClick())
               lArrowMage.Texture = (Texture2D)btnTextures["l_arrowd"];
            else if (lArrowArcher.CanClick())
               lArrowArcher.Texture = (Texture2D)btnTextures["l_arrowd"];
            else if (rArrowSoldier.CanClick())
               rArrowSoldier.Texture = (Texture2D)btnTextures["r_arrowd"];
            else if (rArrowMage.CanClick())
               rArrowMage.Texture = (Texture2D)btnTextures["r_arrowd"];
            else if (rArrowArcher.CanClick())
               rArrowArcher.Texture = (Texture2D)btnTextures["r_arrowd"];
         }
         //Handle the button release events for all buttons
         else if (prevMouse.LeftButton == ButtonState.Pressed && state.LeftButton == ButtonState.Released)
         {
            if (btnMainMenu.CanClick())
            {
               gm.GS = GameState.MainMenu;
               LoadContent();
            }
            else if (btnStart.CanClick())
            {
               btnStart.Texture = (Texture2D)btnTextures["buttonStock1h"];
               if(gm.PurchaseUnits(numSoldiers,numArchers, numMages))
               {
                  numSoldiers = 0;
                  numArchers = 0;
                  numMages = 0;
               }
            }
            else if (lArrowSoldier.CanClick())
            {
               if (numSoldiers > 0)
                  --numSoldiers;
               lArrowSoldier.Texture = (Texture2D)btnTextures["l_arrowh"];
            }
            else if (lArrowMage.CanClick())
            {
               if (numMages > 0)
                  --numMages;
               lArrowMage.Texture = (Texture2D)btnTextures["l_arrowh"];
            }
            else if (lArrowArcher.CanClick())
            {
               if (numArchers > 0)
                  --numArchers;
               lArrowArcher.Texture = (Texture2D)btnTextures["l_arrowh"];
            }
            else if (rArrowSoldier.CanClick())
            {
               ++numSoldiers;
               rArrowSoldier.Texture = (Texture2D)btnTextures["r_arrowh"];
            }
            else if (rArrowArcher.CanClick())
            {
               ++numArchers;
               rArrowArcher.Texture = (Texture2D)btnTextures["r_arrowh"];
            }
            else if (rArrowMage.CanClick())
            {
               ++numMages;
               rArrowMage.Texture = (Texture2D)btnTextures["r_arrowh"];
            }
         }
         prevMouse = state;
      }

      /// <summary>
      /// This is called when the game should draw itself.
      /// </summary>
      /// <param name="gameTime">Provides a snapshot of timing values.</param>
      protected override void Draw(GameTime gameTime)
      {
         GraphicsDevice.Clear(Color.DimGray);
         switch (gm.GS)
         {
            case GameState.MainMenu:
               DrawMainMenu(gameTime);
               break;
            case GameState.Multiplayer:
               DrawSingleplayer(gameTime);
               break;
            case GameState.MultiplayerMenu:
               DrawMultiplayerMenu(gameTime);
               break;
            case GameState.Singleplayer:
               DrawSingleplayer(gameTime);
               break;
            case GameState.GameOver:
               DrawGameOver(gameTime);
               break;
            case GameState.serverLobby:
               spriteBatch.Begin();
               break;
            case GameState.CharacterSelection:
               DrawCharSelection(gameTime);
               break;
            case GameState.Shop:
               DrawShop(gameTime);
               break;
            case GameState.MapSelection:
               DrawMapSelection(gameTime);
               break;
         }
         spriteBatch.End();
         // TODO: Add your drawing code here

         base.Draw(gameTime);
      }

      /// <summary>
      /// All the drawing done each update of the game, while on the
      /// main menu screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void DrawMainMenu(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         btnMultiplayer.Draw(spriteBatch);
         btnSingleplayer.Draw(spriteBatch);
         btnExit.Draw(spriteBatch);
         btnLoad.Draw(spriteBatch);
         btnShop.Draw(spriteBatch);
      }

      /// <summary>
      /// All the drawing done each update of the game, while on the
      /// multiplayer screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void DrawMultiplayer(GameTime gameTime)
      {

      }

      /// <summary>
      /// All the drawing done each update of the game, while on the
      /// multiplayer menu screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void DrawMultiplayerMenu(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         btnMainMenu.x = 50;
         btnMainMenu.y = 50;
         btnMainMenu.Draw(spriteBatch);
         btnStart.x = screenWidth - 50 - btnStart.GetWidth();
         btnStart.y = 50;
         btnStart.Draw(spriteBatch);
         string userDesc = "Username";
         string ipDesc = "ip (e.g. 123.456.7.8)";
         string keyDesc = "Opponents username (optional)";
         Vector2 pos = new Vector2((screenWidth / 2) - (btnFont.MeasureString(userDesc).X / 2),
            (screenHeight / 2) - (txtIP.GetHeight() / 2) - txtUsername.GetHeight() -
            btnFont.MeasureString(userDesc).Y - btnFont.MeasureString(ipDesc).Y);
         spriteBatch.DrawString(btnFont, userDesc, pos, Color.White);
         float posTxtX = (screenWidth / 2) - (txtUsername.GetWidth() / 2);
         pos.Y += btnFont.MeasureString(userDesc).Y;
         txtUsername.x = posTxtX;
         txtUsername.y = pos.Y;
         pos.X = (screenWidth / 2) - (btnFont.MeasureString(ipDesc).X / 2);
         pos.Y += txtUsername.GetHeight();
         spriteBatch.DrawString(btnFont, ipDesc, pos, Color.White);
         pos.Y += btnFont.MeasureString(ipDesc).Y;
         txtIP.x = posTxtX;
         txtIP.y = pos.Y;
         pos.X = (screenWidth / 2) - (btnFont.MeasureString(keyDesc).X / 2);
         pos.Y += txtIP.GetHeight();
         spriteBatch.DrawString(btnFont, keyDesc, pos, Color.White);
         pos.Y += btnFont.MeasureString(keyDesc).Y;
         txtKey.x = posTxtX;
         txtKey.y = pos.Y;
         txtUsername.Draw(spriteBatch);
         txtKey.Draw(spriteBatch);
         txtIP.Draw(spriteBatch);
      }

      /// <summary>
      /// All the drawing done each update of the game, while on the
      /// singleplayer screen
      /// </summary>
      /// <param name="gameTime">The current gameTime of the game</param>
      private void DrawSingleplayer(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         spriteBatch.End();
         spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, camera.TransformationMat);
         spriteBatch.Draw(map_texture, new Vector2(0, 0), Color.White);
         if (drawGrid)
         {
            for (float pos = 0; pos < mapWidth; pos++)
            {
               Rectangle rectangle = new Rectangle((int)(pos * Camera.PIXELS_ACROSS_TILE) - 1,
                  0, (int)(2 / camera.Zoom), mapWidth * Camera.PIXELS_ACROSS_TILE);
               spriteBatch.Draw(tile_outline, rectangle, Color.Black);
               rectangle = new Rectangle(0, (int)(Camera.PIXELS_ACROSS_TILE * pos) - 1,
                  mapHeight * Camera.PIXELS_ACROSS_TILE, (int)(2 / camera.Zoom));
               spriteBatch.Draw(tile_outline, rectangle, Color.Black);
            }
         }
         if (highlightMovement)
         {
            for (int i = 0; i < mapWidth; i++)
               for (int j = 0; j < mapHeight; j++)
               {
                  if (gm.GetTile(i, j).isMovable)
                  {
                     spriteBatch.Draw(tile_highlight, new Vector2(i * Camera.PIXELS_ACROSS_TILE,
                        j * Camera.PIXELS_ACROSS_TILE), Color.White);
                  }
                  else if (gm.GetTile(i, j).isAttackable)
                  {
                     spriteBatch.Draw(tile_highlight, new Vector2(i * Camera.PIXELS_ACROSS_TILE,
                        j * Camera.PIXELS_ACROSS_TILE), Color.Red);
                  }
               }
         }
         foreach (Character c in gm.GetMMAllyArmy())
         {
            if (c.isAlive)
            {
               if (c != movingChar && c != attackingChar && c != attackedChar && c.X >= 0 && c.Y >= 0)
               {
                  DrawCharacter("0b", c);
               }
               else if (c == movingChar)
               {
                  List<Location> path = gm.GetPath();
                  DrawCharMovement("0b", path, gameTime);
               }
               else if (c == attackingChar)
               {
                  string texName = "0b";
                  if (c.charClass == charClassType.Magic)
                     texName += "mage";
                  else if (c.charClass == charClassType.Ranged)
                     texName += "archer";
                  if (attackStage == 0)
                     attackStage = DrawAttackStage0((Texture2D)characterTextures[texName], gameTime);
                  if (attackStage == 1)
                     attackStage = DrawAttackStage1((Texture2D)characterTextures[texName], gameTime);
                  if (attackStage == 2)
                     attackStage = DrawAttackStage2((Texture2D)characterTextures[texName], gameTime);
               }
            }
         }
         foreach (Character c in gm.GetMMenemyArmy())
         {
            if (c.isAlive)
            {
               if (c != movingChar && c != attackingChar && c != attackedChar && c.X >= 0 && c.Y >= 0)
                  DrawCharacter("0r", c);
               else if (c == movingChar)
               {
                  List<Location> path = gm.GetPath();
                  DrawCharMovement("0r", path, gameTime);
               }
               else if (c == attackingChar)
               {
                  string texName = "0r";
                  if (c.charClass == charClassType.Magic)
                     texName += "mage";
                  else if (c.charClass == charClassType.Ranged)
                     texName += "archer";
                  if (attackStage == 0)
                     attackStage = DrawAttackStage0((Texture2D)characterTextures[texName], gameTime);
                  if (attackStage == 1)
                     attackStage = DrawAttackStage1((Texture2D)characterTextures[texName], gameTime);
                  if (attackStage == 2)
                     attackStage = DrawAttackStage2((Texture2D)characterTextures[texName], gameTime);
               }
            }
         }
         spriteBatch.End();
         spriteBatch.Begin();
         if (hoveredChar != null)
            DrawStats();
         if (gameOver)
         {
            spriteBatch.End();
            UnloadContent();
            gm.GS = GameState.GameOver;
            LoadContent();
            spriteBatch.Begin();
            gameOver = false;
         }
      }

      private void DrawStats()
      {
         List<string> stats = new List<string>();
         stats.Add("Range: " + hoveredChar.attackRange);
         stats.Add("Class: " + hoveredChar.charClass);
         stats.Add("Dexterity: " + hoveredChar.dexterity);
         stats.Add("XP: " + hoveredChar.experience);
         stats.Add("Attack: " + hoveredChar.GetAttack());
         //stats.Add(hoveredChar.GetBaseDexterity() + "");
         //stats.Add(hoveredChar.GetBaseInitiative() + "");
         //stats.Add(hoveredChar.GetBaseStrength() + "");
         stats.Add("Strength: " + hoveredChar.GetStrength());
         stats.Add("HP: " + hoveredChar.health + "/" + hoveredChar.maxHealth);
         stats.Add("Attack Used: " + hoveredChar.isAbleToAttack);
         stats.Add("Move Used: " + hoveredChar.isAbleToMove);
         stats.Add("Level: " + hoveredChar.level);
         stats.Add("Luck: " + hoveredChar.luck);
         stats.Add("Magic Resistance: " + hoveredChar.magicResistance);
         stats.Add("Speed: " + hoveredChar.movementRange);
         stats.Add("Name: " + hoveredChar.name);
         stats.Add("Physical Resistance: " + hoveredChar.physicalResistance);
         if (statsPos.X != 0)
            statsPos.X = screenWidth - 250;
         if (statsPos.Y != 0)
            statsPos.Y = screenHeight - (stats.Count * (int)statsFont.MeasureString("0").Y) - 10;
         spriteBatch.Draw(stats_hud, new Rectangle((int)statsPos.X, (int)statsPos.Y, 250,
            stats.Count * (int)statsFont.MeasureString("0").Y + 10), Color.White);
         Vector2 pos = statsPos;
         pos.X += 5;
         pos.Y += 5;
         foreach (string s in stats)
         {
            spriteBatch.DrawString(statsFont, s, pos, Color.White);
            pos.Y += statsFont.MeasureString(s).Y;
         }
         if (statsPos.X != 0)
            statsPos.X = screenWidth;
         if (statsPos.Y != 0)
            statsPos.Y = screenHeight;
      }

      private void DrawCharMovement(string baseTexture, List<Location> path, GameTime gameTime)
      {
         if (movingChar.charClass == charClassType.Ranged)
            baseTexture += "archer";
         else if (movingChar.charClass == charClassType.Magic)
            baseTexture += "mage";
         Texture2D texture = (Texture2D)characterTextures[baseTexture];
         if (path.Count > 0)
         {
            if (movingPlayerLoc == new Vector2(-1, -1))
            {
               movingPlayerLoc = new Vector2(movingChar.X * Camera.PIXELS_ACROSS_TILE +
                  (Camera.PIXELS_ACROSS_TILE / 2 - texture.Width / 2),
                  movingChar.Y * Camera.PIXELS_ACROSS_TILE - (texture.Height - Camera.PIXELS_ACROSS_TILE));
               gm.MoveCharToNextPathLocation(movingChar, path);
            }
            if (path.Count > 0)
            {
               Location[] locations = path.ToArray();
               Vector2 target = new Vector2((locations[0].X * Camera.PIXELS_ACROSS_TILE) -
                  movingPlayerLoc.X + (Camera.PIXELS_ACROSS_TILE / 2 - texture.Width / 2),
                  (locations[0].Y * Camera.PIXELS_ACROSS_TILE) - movingPlayerLoc.Y -
                  (texture.Height - Camera.PIXELS_ACROSS_TILE));
               if (Math.Abs(target.X) <= 2 && Math.Abs(target.Y) <= 2)
               {
                  spriteBatch.Draw(texture, movingPlayerLoc, Color.White);
                  movingPlayerLoc = new Vector2(-1, -1);
               }
               else
               {
                  target.Normalize();
                  movingPlayerLoc += target * (float)gameTime.ElapsedGameTime.TotalSeconds * 150;
                  spriteBatch.Draw(texture, movingPlayerLoc, Color.White);
               }
            }
         }
         else
         {
            if (attackingChar == null)
               gm.SetMapManagerToStanby();
            movingChar = null;
            movingPlayerLoc = new Vector2(-1, -1);
         }
      }

      private int DrawAttackStage0(Texture2D texture, GameTime gameTime)
      {
         if (attackedChar == null)
            attackedChar = gm.GetAttackedChar();
         if (attackLoc == new Vector2(-1, -1))
            attackLoc = new Vector2((attackingChar.X * Camera.PIXELS_ACROSS_TILE) +
               (Camera.PIXELS_ACROSS_TILE / 2) - (texture.Width / 2),
               attackingChar.Y * Camera.PIXELS_ACROSS_TILE);
         Vector2 target = new Vector2((attackingChar.X * Camera.PIXELS_ACROSS_TILE) - attackLoc.X,
            (attackingChar.Y * Camera.PIXELS_ACROSS_TILE) - attackLoc.Y);
         if (Math.Abs(target.X) <= 2)
         {
            spriteBatch.Draw(texture, attackLoc, Color.White);
            return 1;
         }
         target.Normalize();
         attackLoc.X += target.X * (float)gameTime.ElapsedGameTime.TotalSeconds * 150;
         spriteBatch.Draw(texture, attackLoc, Color.White);
         string baseName = "";
         if (texture.Name.Contains("0b"))
            baseName = "0r";
         else
            baseName = "0b";
         DrawCharacter(baseName, attackedChar);
         return 0;
      }

      private int DrawAttackStage1(Texture2D texture, GameTime gameTime)
      {
         if (attackLoc == new Vector2(-1, -1))
            attackLoc = new Vector2((attackingChar.X * Camera.PIXELS_ACROSS_TILE),
               attackingChar.Y * Camera.PIXELS_ACROSS_TILE);
         Vector2 target = new Vector2(((attackingChar.X + 1) * Camera.PIXELS_ACROSS_TILE) -
            attackLoc.X - texture.Width, (attackingChar.Y * Camera.PIXELS_ACROSS_TILE) - attackLoc.Y);
         if (Math.Abs(target.X) <= 2)
         {
            spriteBatch.Draw(texture, attackLoc, Color.White);
            return 2;
         }
         target.Normalize();
         attackLoc.X += target.X * (float)gameTime.ElapsedGameTime.TotalSeconds * 150;
         spriteBatch.Draw(texture, attackLoc, Color.White);
         string baseName = "";
         if (texture.Name.Contains("0b"))
            baseName = "0r";
         else
            baseName = "0b";
         DrawCharacter(baseName, attackedChar);
         return 1;
      }

      private int DrawAttackStage2(Texture2D texture, GameTime gameTime)
      {
         if (attackLoc == new Vector2(-1, -1))
            attackLoc = new Vector2(((attackingChar.X + 1) * Camera.PIXELS_ACROSS_TILE) -
               texture.Width, (attackingChar.Y * Camera.PIXELS_ACROSS_TILE));
         Vector2 target = new Vector2((attackingChar.X * Camera.PIXELS_ACROSS_TILE) +
               (Camera.PIXELS_ACROSS_TILE / 2) - (texture.Width / 2) - attackLoc.X,
               attackingChar.Y * Camera.PIXELS_ACROSS_TILE - attackLoc.Y);
         if (Math.Abs(target.X) <= 2)
         {
            spriteBatch.Draw(texture, attackLoc, Color.White);
            attackLoc = new Vector2(-1, -1);
            attackingChar = null;
            string tempEvent = gm.ExecuteAttack();
            if (tempEvent == "over")
            {
               gameOver = true;
            }
            attackedChar = null;
            return 0;
         }
         target.Normalize();
         attackLoc.X += target.X * (float)gameTime.ElapsedGameTime.TotalSeconds * 150;
         spriteBatch.Draw(texture, attackLoc, Color.White);
         string baseName = "";
         if (texture.Name.Contains("0b"))
            baseName = "0r";
         else
            baseName = "0b";
         if (gm.GetHitOrMiss())
            DrawCharacter(baseName, attackedChar, Color.Red);
         else
            DrawCharacter(baseName, attackedChar, Color.White);
         return 2;
      }

      private void DrawCharSelection(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         btnStart.x = screenWidth - btnStart.GetWidth() - 5;
         btnStart.Draw(spriteBatch);
         btnMainMenu.Draw(spriteBatch);
         Vector2 statsPos = new Vector2((screenWidth / 2) - 225, 0);
         spriteBatch.Draw(stats_hud, new Rectangle(statsPos.ToPoint(), new Point(450, 100)), Color.White);
         btnSelect.x = (screenWidth / 2) + 1;
         btnSelect.y = statsPos.Y + 101;
         btnSelect.Draw(spriteBatch);
         lstSelected.x = (screenWidth / 2) + 1;
         lstSelected.y = btnSelect.y + btnSelect.GetHeight();
         lstSelected.SetHeight(screenHeight - lstSelected.y);
         lstSelected.Draw(spriteBatch);
         btnRemove.x = (screenWidth / 2) - 1 - btnRemove.GetWidth();
         btnRemove.y = btnSelect.y;
         btnRemove.Draw(spriteBatch);
         lstArmy.x = (screenWidth / 2) - 1 - lstArmy.GetWidth();
         lstArmy.y = lstSelected.y;
         lstArmy.SetHeight(screenHeight - lstArmy.y);
         lstArmy.Draw(spriteBatch);
         Vector2 pos = statsPos;
         pos.X += 10;
         pos.Y += 5;
         if (lstArmy.SelectedObj != null)
         {
            Character c = (Character)lstArmy.SelectedObj;
            List<string> stats = new List<string>();
            stats.Add("Strength: " + c.strength);
            stats.Add("Dexterity: " + c.dexterity);
            stats.Add("Physical Resistance: " + c.physicalResistance);
            stats.Add("Magic Resistance: " + c.magicResistance);
            foreach (string s in stats)
            {
               spriteBatch.DrawString(statsFont, s, pos, Color.White);
               pos.Y += statsFont.MeasureString(s).Y;
            }
         }
         pos.X = statsPos.X + 225 + 10;
         pos.Y = statsPos.Y + 5;
         if (lstSelected.SelectedObj != null)
         {
            Character c = (Character)lstSelected.SelectedObj;
            List<string> stats = new List<string>();
            stats.Add("Strength: " + c.strength);
            stats.Add("Dexterity: " + c.dexterity);
            stats.Add("Physical Resistance: " + c.physicalResistance);
            stats.Add("Magic Resistance: " + c.magicResistance);
            foreach (string s in stats)
            {
               spriteBatch.DrawString(statsFont, s, pos, Color.White);
               pos.Y += statsFont.MeasureString(s).Y;
            }
         }
      }

      private void DrawMapSelection(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         btnStart.x = screenWidth - btnStart.GetWidth() - 5;
         btnStart.Draw(spriteBatch);
         btnMainMenu.Draw(spriteBatch);
         lstSelected.x = 5;
         lstSelected.y = btnMainMenu.y + btnMainMenu.GetHeight() + 5;
         lstSelected.SetHeight(screenHeight - lstSelected.y);
         lstSelected.Draw(spriteBatch);
         if (lstSelected.SelectedItem != -1)
         {
            string name = (string)lstSelected.SelectedObj;
            Texture2D mapTex = Content.Load<Texture2D>(name);
            float x = screenWidth - lstSelected.x - lstSelected.GetWidth() - 5,
               y = screenHeight - lstSelected.y - 5;
            float min = Math.Min(x, y);
            spriteBatch.Draw(mapTex, new Rectangle((int)(lstSelected.x + lstSelected.GetWidth() + 5),
               (int)lstSelected.y, (int)min, (int)min), Color.White);
         }
      }

      private void DrawShop(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         Texture2D character = (Texture2D)characterTextures["0b"];
         Vector2 pos = new Vector2((screenWidth / 4) - (character.Width / 2),
            (screenHeight / 2) - (character.Height / 2));
         spriteBatch.Draw(character, pos, Color.White);
         Vector2 pos2 = new Vector2(pos.X + (character.Width / 2) - (lArrowSoldier.GetWidth() / 2) -
            btnFont.MeasureString("0").X - 10, pos.Y + character.Height + 15);
         lArrowSoldier.x = pos2.X;
         lArrowSoldier.y = pos2.Y;
         lArrowSoldier.Draw(spriteBatch);
         pos2 = new Vector2(pos2.X + btnFont.MeasureString("0").X + 12, pos2.Y - 5);
         spriteBatch.DrawString(btnFont, numSoldiers.ToString(), pos2, Color.White);
         pos2 = new Vector2(pos2.X + btnFont.MeasureString("0").X + 8, pos2.Y + 5);
         rArrowSoldier.x = pos2.X;
         rArrowSoldier.y = pos2.Y;
         rArrowSoldier.Draw(spriteBatch);
         character = (Texture2D)characterTextures["0barcher"];
         pos = new Vector2((screenWidth / 2) - (character.Width / 2), pos.Y - 20);
         spriteBatch.Draw(character, pos, Color.White);
         pos2 = new Vector2(pos.X + (character.Width / 2) - (lArrowArcher.GetWidth() / 2) -
            btnFont.MeasureString("0").X - 10, pos2.Y);
         lArrowArcher.x = pos2.X;
         lArrowArcher.y = pos2.Y;
         lArrowArcher.Draw(spriteBatch);
         pos2 = new Vector2(pos2.X + btnFont.MeasureString("0").X + 12, pos2.Y - 5);
         spriteBatch.DrawString(btnFont, numArchers.ToString(), pos2, Color.White);
         pos2 = new Vector2(pos2.X + btnFont.MeasureString("0").X + 8, pos2.Y + 5);
         rArrowArcher.x = pos2.X;
         rArrowArcher.y = pos2.Y;
         rArrowArcher.Draw(spriteBatch);
         character = (Texture2D)characterTextures["0bmage"];
         pos = new Vector2((screenWidth * 3 / 4) - (character.Width / 2), pos.Y + 20);
         spriteBatch.Draw(character, pos, Color.White);
         pos2 = new Vector2(pos.X + (character.Width / 2) - (lArrowMage.GetWidth() / 2) -
            btnFont.MeasureString("0").X - 10, pos2.Y);
         lArrowMage.x = pos2.X;
         lArrowMage.y = pos2.Y;
         lArrowMage.Draw(spriteBatch);
         pos2 = new Vector2(pos2.X + btnFont.MeasureString("0").X + 12, pos2.Y - 5);
         spriteBatch.DrawString(btnFont, numMages.ToString(), pos2, Color.White);
         pos2 = new Vector2(pos2.X + btnFont.MeasureString("0").X + 8, pos2.Y + 5);
         rArrowMage.x = pos2.X;
         rArrowMage.y = pos2.Y;
         rArrowMage.Draw(spriteBatch);
         btnMainMenu.x = 50;
         btnMainMenu.y = 50;
         btnMainMenu.Draw(spriteBatch);
         btnStart.x = (screenWidth / 2) - (btnStart.GetWidth() / 2);
         btnStart.y = screenHeight - btnStart.GetHeight() - 5;
         btnStart.Draw(spriteBatch);
         spriteBatch.DrawString(btnFont, "Gold: " + gm.GetArmyFunds(), new Vector2((screenWidth / 2) -
            (btnFont.MeasureString("Gold: " + gm.GetArmyFunds()).X / 2), 5), Color.White);
         int cost = UNIT_COST * (numArchers + numMages + numSoldiers);
         spriteBatch.DrawString(btnFont, "Gold: " + cost, new Vector2((screenWidth / 2) -
            (btnFont.MeasureString("Gold: " + cost).X / 2), btnStart.y - btnFont.MeasureString("Gold: " + cost).Y - 5), Color.White);
      }

      private void DrawGameOver(GameTime gameTime)
      {
         spriteBatch.Begin();
         spriteBatch.Draw(background, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White);
         Vector2 pos = new Vector2((screenWidth / 2) - (gameOverFont.MeasureString("Game   Over").X / 2), 75);
         spriteBatch.DrawString(gameOverFont, "Game  Over", pos, Color.White);
         pos = new Vector2(((screenWidth - gameOverFont.MeasureString("Game   Over").X) / 2) +
                     (gameOverFont.MeasureString("Game").X / 2) - (btnMainMenu.GetWidth() / 2),
                     75 + gameOverFont.MeasureString("Game   Over").Y);
         btnMainMenu.x = pos.X;
         pos = new Vector2(((screenWidth - gameOverFont.MeasureString("Game   Over").X) / 2) +
                     gameOverFont.MeasureString("Game  ").X + (gameOverFont.MeasureString("Over").X / 2)
                     - (btnMainMenu.GetWidth() / 2), 75 + gameOverFont.MeasureString("Game   Over").Y);
         btnExit.x = pos.X;
         btnMainMenu.Draw(spriteBatch);
         btnExit.Draw(spriteBatch);
      }

      private void HighlightMovementRange(int distanceToMove, int x, int y)
      {
         if (distanceToMove > 0)
         {
            Tile t = gm.GetTile(x, y);
            if (t.isPassable)
            {
               if (!t.isHighlighted)
               {
                  t.isHighlighted = true;
               }
               if (x - 1 >= 0)
                  HighlightMovementRange(distanceToMove - 1, x - 1, y);
               HighlightMovementRange(distanceToMove - 1, x + 1, y);
               if (y - 1 >= 0)
                  HighlightMovementRange(distanceToMove - 1, x, y - 1);
               HighlightMovementRange(distanceToMove - 1, x, y + 1);
            }
         }
      }

      private void ResetAllTileHighlights()
      {
         for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
               gm.GetTile(i, j).isHighlighted = false;
      }

      /// <summary>
      /// Used to draw a character at the given position, with the provided texture
      /// </summary>
      /// <param name="texture">The texture to draw</param>
      /// <param name="c">The character to get the position from</param>
      private void DrawCharacter(String baseTexture, Character c, Color? highlight = null)
      {
         if (c.charClass == charClassType.Ranged)
            baseTexture += "archer";
         else if (c.charClass == charClassType.Magic)
            baseTexture += "mage";
         Texture2D texture = (Texture2D)characterTextures[baseTexture];
         Color color = highlight ?? Color.White;
         spriteBatch.Draw(texture, new Vector2(c.X * Camera.PIXELS_ACROSS_TILE + (Camera.PIXELS_ACROSS_TILE / 2 - texture.Width / 2),
            c.Y * Camera.PIXELS_ACROSS_TILE - (texture.Height - Camera.PIXELS_ACROSS_TILE)), color);
      }

      /// <summary>
      /// The event handler whenever the window size is changed, used to keep the camera
      /// and background images displayed correctly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Window_ClientSizeChanged(object sender, System.EventArgs e)
      {
         graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.Viewport.Height;
         graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.Viewport.Width;
         screenHeight = graphics.PreferredBackBufferHeight;
         screenWidth = graphics.PreferredBackBufferWidth;
         camera.ViewportSize = new Vector2(screenWidth, screenHeight);
         camera.Move(Vector2.Zero);
      }

      private void SetActiveTextbox(TextBox textbox)
      {
         if (!textbox.text.Contains("_"))
         {
            selectedTxt.text = selectedTxt.text.TrimEnd('_');
            selectedTxt = textbox;
            selectedTxt.text += "_";
         }
      }

      /// <summary>
      /// The button click event when the multiplayer button is clicked
      /// on the main menu screen
      /// </summary>
      public void StartMultiplayer()
      {
         prevMouse = new MouseState();
         string serverEvent = gm.SetUpMultiplayerGame(txtIP.text.TrimEnd('_'), txtUsername.text.TrimEnd('_'), txtKey.text.TrimEnd('_'));
         LoadContent();
         highlightMovement = true;
         //gm.GS = GameState.GameOver;
         //LoadContent();
      }

      /// <summary>
      /// The button click event when the singleplayer button is clicked
      /// on the main menu screen
      /// </summary>
      public void StartSingleplayer()
      {
         prevMouse = new MouseState();
         gm.GS = GameState.Singleplayer;
         LoadContent();
         if (mapName != "" && mapName != null)
         {
            gm.StartSinglePlayerFight(mapName);
            camera.mapWidth = mapWidth = gm.GetMapWidth();
            camera.mapHeight = mapHeight = gm.GetMapHeight();
         }
         else
            gm.StartSinglePlayerFight();
         highlightMovement = true;
      }

      public void StartLoadedMap()
      {
         prevMouse = new MouseState();
         gm.GS = GameState.Singleplayer;
         gm.LoadFight();
         mapName = gm.GetMapName();
         mapWidth = camera.mapWidth = gm.GetMapWidth();
         mapHeight = camera.mapHeight = gm.GetMapHeight();
         LoadContent();

      }
   }
}
