using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MissileDefense
{
    // Missile class
    public class Missile : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private Texture2D texture;
        private Rectangle rectangle;
        private Random random;

        public Missile(Game game) : base(game)
        {
            spriteBatch = game.Services.GetService<SpriteBatch>();
            texture = game.Content.Load<Texture2D>("missile");
            random = new Random();
            rectangle = new Rectangle(random.Next(0, Game.Window.ClientBounds.Width - 10), Game.Window.ClientBounds.Height, 10, 20);
        }

        public override void Update(GameTime gameTime)
        {
            rectangle.Y -= 5;
            if (rectangle.Y < 0)
            {
                this.Dispose();
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, rectangle, Color.Red);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        public Rectangle Rectangle
        {
            get { return rectangle; }
        }
    }

    // Player class
    public class Player : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private Texture2D texture;
        private Rectangle rectangle;

        public Player(Game game) : base(game)
        {
            spriteBatch = game.Services.GetService<SpriteBatch>();
            texture = game.Content.Load<Texture2D>("player");
            rectangle = new Rectangle(Game.Window.ClientBounds.Width / 2 - 25, Game.Window.ClientBounds.Height - 60, 50, 50);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Left) && rectangle.Left > 0)
            {
                rectangle.X -= 5;
            }
            if (keyboardState.IsKeyDown(Keys.Right) && rectangle.Right < Game.Window.ClientBounds.Width)
            {
                rectangle.X += 5;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, rectangle, Color.Blue);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        public Rectangle Rectangle
        {
            get { return rectangle; }
        }
    }

    // Game class
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private List<Missile> missiles;
        private Player player;
        private SpriteFont font;
        private int score;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            missiles = new List<Missile>();
            score = 0;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService<SpriteBatch>(spriteBatch);
            player = new Player(this);
            Components.Add(player);
            font = Content.Load<SpriteFont>("font");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                Missile missile = new Missile(this);
                missiles.Add(missile);
                Components.Add(missile);
            }

            foreach (Missile missile in missiles)
            {
                if (player.Rectangle.Intersects(missile.Rectangle))
                {
                    missile.Dispose();
                    score++;
                    Console.WriteLine("Missile intercepted!");
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Score: " + score, new Vector2(10, 10), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
