using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FullGameExample
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private AnimatedSprite playerSprite;
        private List<AdvancedMissile> missiles;
        private List<PowerUp> powerUps;
        private List<Enemy> enemies;
        private Texture2D playerTexture;
        private Texture2D missileTexture;
        private Texture2D powerUpTexture;
        private Texture2D enemyTexture;
        private SpriteFont font;
        private int score;
        private AdvancedPlayer player;
        private Texture2D backgroundTexture;
        private Texture2D particleTexture;
        private ParticleSystem explosionParticles;
        private Camera2D camera;
        private Vector2 cameraOffset;
        private SoundEffect backgroundMusic;
        private SoundEffect explosionSound;
        private SoundEffect missileSound;
        private SoundEffectInstance backgroundMusicInstance;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            missiles = new List<AdvancedMissile>();
            powerUps = new List<PowerUp>();
            score = 0;
            enemies = new List<Enemy>();
        }

        protected override void Initialize()
        {
            camera = new Camera2D(GraphicsDevice.Viewport);
            cameraOffset = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService<SpriteBatch>(spriteBatch);

            playerTexture = Content.Load<Texture2D>("player_animation");
            playerSprite = new AnimatedSprite(playerTexture, 4, 4, 0.1);

            missileTexture = Content.Load<Texture2D>("missile");
            powerUpTexture = Content.Load<Texture2D>("powerup");
            enemyTexture = Content.Load<Texture2D>("enemy");
            backgroundTexture = Content.Load<Texture2D>("background");
            particleTexture = Content.Load<Texture2D>("particle");

            font = Content.Load<SpriteFont>("font");
            explosionSound = Content.Load<SoundEffect>("explosion");
            backgroundMusic = Content.Load<SoundEffect>("background_music");
            backgroundMusicInstance = backgroundMusic.CreateInstance();
            backgroundMusicInstance.IsLooped = true;
            backgroundMusicInstance.Play();
            missileSound = Content.Load<SoundEffect>("missile_sound");

            explosionParticles = new ParticleSystem(particleTexture);
            explosionParticles.ParticleColor = Color.Yellow;
            explosionParticles.ParticleSpeed = 5f;
            explosionParticles.ParticleSize = 8;
            explosionParticles.ParticleLifetime = TimeSpan.FromSeconds(1f);

            InitializeEnemies();
            InitializePowerUps();

            player = new AdvancedPlayer(this, playerTexture, 100, 50, missileSound);
            Components.Add(player);

            base.LoadContent();
        }

        private void InitializeEnemies()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 position = new Vector2(i * 150, 200);
                Enemy enemy = new Enemy(this, enemyTexture, position, explosionSound);
                enemies.Add(enemy);
                Components.Add(enemy);
            }
        }

        private void InitializePowerUps()
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 position = new Vector2(i * 200, 400);
                PowerUp powerUp = new PowerUp(this, powerUpTexture, position, explosionSound);
                powerUps.Add(powerUp);
                Components.Add(powerUp);
            }
        }

        protected override void UnloadContent()
        {
            // Unload any non-ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            playerSprite.Update(gameTime);

            UpdateMissiles(gameTime);
            UpdatePowerUps(gameTime);
            UpdateEnemies(gameTime);

            CheckCollisions();

            camera.Position = player.Position + cameraOffset;

            base.Update(gameTime);
        }

        private void UpdateMissiles(GameTime gameTime)
        {
            foreach (AdvancedMissile missile in missiles.ToArray())
            {
                missile.Update(gameTime);

                if (missile.Rectangle.Y > GraphicsDevice.Viewport.Height)
                {
                    missile.Dispose();
                    missiles.Remove(missile);
                }
            }
        }

        private void UpdatePowerUps(GameTime gameTime)
        {
            foreach (PowerUp powerUp in powerUps.ToArray())
            {
                powerUp.Update(gameTime);

                if (powerUp.Rectangle.Y > GraphicsDevice.Viewport.Height)
                {
                    powerUp.Dispose();
                    powerUps.Remove(powerUp);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies.ToArray())
            {
                enemy.Update(gameTime);

                if (enemy.IsDestroyed)
                {
                    enemy.Dispose();
                    enemies.Remove(enemy);
                }
            }
        }

        private void CheckCollisions()
        {
            foreach (AdvancedMissile missile in missiles.ToArray())
            {
                foreach (Enemy enemy in enemies.ToArray())
                {
                    if (missile.Rectangle.Intersects(enemy.Rectangle))
                    {
                        missile.Dispose();
                        missiles.Remove(missile);
                        enemy.TakeDamage(missile.Damage);
                        PlayExplosionEffect(enemy.Position);
                    }
                }
            }

            foreach (PowerUp powerUp in powerUps.ToArray())
            {
                if (player.Rectangle.Intersects(powerUp.Rectangle))
                {
                    powerUp.Collect();
                    powerUps.Remove(powerUp);
                    score++;
                    Console.WriteLine("Power-up collected! Score: " + score);
                }
            }
        }

        private void PlayExplosionEffect(Vector2 position)
        {
            explosionParticles.Position = position;
            explosionParticles.Trigger();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, camera.TransformMatrix);

            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

            foreach (AdvancedMissile missile in missiles)
            {
                missile.Draw(gameTime);
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(gameTime);
            }

            playerSprite.Draw(spriteBatch, player.Position);

            explosionParticles.Draw(spriteBatch);

            spriteBatch.DrawString(font, "Score: " + score, new Vector2(10, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    // ... (Particle, AdvancedPlayer, Missile, PowerUp, Enemy, AnimatedSprite, Camera2D classes)

    public class AdvancedMissile : DrawableGameComponent
    {
        private Texture2D texture;
        private Vector2 position;
        private Rectangle rectangle;
        private int speed;
        private int damage;

        public Rectangle Rectangle
        {
            get { return rectangle; }
        }

        public int Damage
        {
            get { return damage; }
        }

        public AdvancedMissile(Game game, Texture2D texture, int x, int y, int damage) : base(game)
        {
            this.texture = texture;
            position = new Vector2(x, y);
            rectangle = new Rectangle(x, y, texture.Width, texture.Height);
            speed = 10;
            this.damage = damage;
        }

        public override void Update(GameTime gameTime)
        {
            position.Y -= speed;
            rectangle.Y = (int)position.Y;

            if (position.Y < 0)
                Dispose();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
            spriteBatch.Draw(texture, position, Color.White);
            base.Draw(gameTime);
        }

        public void Dispose()
        {
            Game.Components.Remove(this);
        }
    }

    // ... (Other classes and methods)

    public class Program
    {
        static void Main()
        {
            using (var game = new Game1())
            {
                game.Run();
            }
        }
    }
}
