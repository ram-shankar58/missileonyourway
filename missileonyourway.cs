using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace YourNamespace
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

            player = new AdvancedPlayer(this, playerTexture, 100, 50);
            Components.Add(player);

            font = Content.Load<SpriteFont>("font");
            explosionSound = Content.Load<SoundEffect>("explosion");
            backgroundMusic = Content.Load<SoundEffect>("background_music");
            backgroundMusicInstance = backgroundMusic.CreateInstance();
            backgroundMusicInstance.IsLooped = true;
            backgroundMusicInstance.Play();

            explosionParticles = new ParticleSystem(particleTexture);
            explosionParticles.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            explosionParticles.ParticleColor = Color.Yellow;
            explosionParticles.ParticleSpeed = 5f;
            explosionParticles.ParticleSize = 8;
            explosionParticles.ParticleLifetime = TimeSpan.FromSeconds(1f);

            InitializeEnemies();
            InitializePowerUps();

            base.LoadContent();
        }

        private void InitializeEnemies()
        {
            // Create and position enemies within the game world
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
            // Create and position power-ups within the game world
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

    public class ParticleSystem
    {
        private Texture2D particleTexture;
        private List<Particle> particles;

        public ParticleSystem(Texture2D particleTexture)
        {
            this.particleTexture = particleTexture;
            particles = new List<Particle>();
        }

        public void Trigger(Vector2 position, Color color)
        {
            for (int i = 0; i < 100; i++)
            {
                float angle = MathHelper.ToRadians(i * 3.6f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Particle particle = new Particle(particleTexture, position, velocity, color);
                particles.Add(particle);
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle particle = particles[i];
                particle.Update(gameTime);

                if (particle.IsExpired)
                {
                    particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Particle particle in particles)
            {
                particle.Draw(spriteBatch);
            }
        }
    }

    public class Particle
    {
        private Texture2D texture;
        private Vector2 position;
        private Vector2 velocity;
        private float rotation;
        private float scale;
        private float lifetime;
        private float timer;

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity, Color color)
        {
            this.texture = texture;
            this.position = position;
            this.velocity = velocity;
            this.rotation = 0f;
            this.scale = 1f;
            this.lifetime = 1f;
            this.timer = 0f;
        }

        public bool IsExpired
        {
            get { return timer >= lifetime; }
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timer += elapsed;

            position += velocity * elapsed;
            rotation += MathHelper.ToRadians(1f);
            scale = 1f - timer / lifetime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Color color = new Color(255, 255, 255, 255) * (1f - timer / lifetime);

            spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }

    public class AdvancedPlayer : DrawableGameComponent
    {
        private Texture2D texture;
        private Vector2 position;
        private Rectangle rectangle;
        private SoundEffect missileSound;
        private TimeSpan missileDelay;
        private TimeSpan previousMissileTime;

        public AdvancedPlayer(Game game, Texture2D texture, int x, int y) : base(game)
        {
            this.texture = texture;
            position = new Vector2(x, y);
            rectangle = new Rectangle(x, y, texture.Width, texture.Height);
            missileSound = game.Content.Load<SoundEffect>("missile_sound");
            missileDelay = TimeSpan.FromSeconds(0.5);
            previousMissileTime = TimeSpan.Zero;
        }
            public Rectangle Rectangle
    {
        get { return rectangle; }
    }

    public Vector2 Position
    {
        get { return position; }
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        // Player movement logic
        if (keyboardState.IsKeyDown(Keys.Left))
            position.X -= 5;
        if (keyboardState.IsKeyDown(Keys.Right))
            position.X += 5;
        if (keyboardState.IsKeyDown(Keys.Up))
            position.Y -= 5;
        if (keyboardState.IsKeyDown(Keys.Down))
            position.Y += 5;

        // Player shooting logic
        if (keyboardState.IsKeyDown(Keys.Space))
        {
            TimeSpan elapsed = gameTime.TotalGameTime - previousMissileTime;
            if (elapsed >= missileDelay)
            {
                FireMissile();
                previousMissileTime = gameTime.TotalGameTime;
            }
        }

        rectangle.X = (int)position.X;
        rectangle.Y = (int)position.Y;

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
        spriteBatch.Draw(texture, position, Color.White);
        base.Draw(gameTime);
    }

    private void FireMissile()
    {
        Vector2 missilePosition = new Vector2(position.X + texture.Width / 2, position.Y);
        AdvancedMissile missile = new AdvancedMissile(Game, missileTexture, missilePosition, 10);
        missiles.Add(missile);
        Game.Components.Add(missile);

        missileSound.Play();
    }
}

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

    public AdvancedMissile(Game game, Texture2D texture, Vector2 position, int damage) : base(game)
    {
        this.texture = texture;
        this.position = position;
        rectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
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

public class PowerUp : DrawableGameComponent
{
    private Texture2D texture;
    private Vector2 position;
    private Rectangle rectangle;
    private SoundEffect collectSound;

    public Rectangle Rectangle
    {
        get { return rectangle; }
    }

    public PowerUp(Game game, Texture2D texture, Vector2 position, SoundEffect collectSound) : base(game)
    {
        this.texture = texture;
        this.position = position;
        rectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        this.collectSound = collectSound;
    }

    public override void Update(GameTime gameTime)
    {
        position.Y += 2;
        rectangle.Y = (int)position.Y;

        if (position.Y > GraphicsDevice.Viewport.Height)
            Dispose();

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
        spriteBatch.Draw(texture, position, Color.White);
        base.Draw(gameTime);
    }

    public void Collect()
    {
        // Handle power-up collection logic
        // Replace this with your actual logic
        // For example, increase player's health or score
        // For example:
        // player.Health += 10;
        // player.Score += 100;

        collectSound.Play();
    }

    public void Dispose()
    {
        Game.Components.Remove(this);
    }
}

public class Enemy : DrawableGameComponent
{
    private Texture2D texture;
    private Vector2 position;
    private Rectangle rectangle;
    private SoundEffect explosionSound;
    private bool isDestroyed;

    public bool IsDestroyed
    {
        get { return isDestroyed; }
    }

    public Rectangle Rectangle
    {
        get { return rectangle; }
    }

    public Enemy(Game game, Texture2D texture, Vector2 position, SoundEffect explosionSound) : base(game)
    {
        this.texture = texture;
        this.position = position;
        rectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        this.explosionSound = explosionSound;
        isDestroyed = false;
    }

    public void TakeDamage(int damage)
    {
        // Handle enemy damage logic
        // For example, update health and check if destroyed
        // Replace this with your actual logic
        // For example:
        // health -= damage;
        // if (health <= 0)
        // {
        //     isDestroyed = true;
        // }

        // If destroyed, play explosion sound and trigger particle effect
        if (isDestroyed)
        {
            explosionSound.Play();
            Game1 game = (Game1)Game;
            game.PlayExplosionEffect(position);
        }
    }

    public override void Update(GameTime gameTime)
    {
        // Update logic for the enemy
        // ...

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw the enemy
        // ...

        base.Draw(gameTime);
    }

    public void Dispose()
    {
        Game.Components.Remove(this);
    }
}

public class AnimatedSprite
{
    private Texture2D texture;
    private int frameWidth;
    private int frameHeight;
    private int currentFrame;
    private int totalFrames;
    private double frameTime;
    private double elapsedFrameTime;

    public AnimatedSprite(Texture2D texture, int rows, int columns, double frameTime)
    {
        this.texture = texture;
        frameWidth = texture.Width / columns;
        frameHeight = texture.Height / rows;
        currentFrame = 0;
        totalFrames = rows * columns;
        this.frameTime = frameTime;
        elapsedFrameTime = 0;
    }

    public void Update(GameTime gameTime)
    {
        elapsedFrameTime += gameTime.ElapsedGameTime.TotalSeconds;

        if (elapsedFrameTime >= frameTime)
        {
            currentFrame++;
            if (currentFrame >= totalFrames)
                currentFrame = 0;
            elapsedFrameTime = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        int row = currentFrame / (texture.Width / frameWidth);
        int column = currentFrame % (texture.Width / frameWidth);

        Rectangle sourceRectangle = new Rectangle(frameWidth * column, frameHeight * row, frameWidth, frameHeight);
        Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight);

        spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
    }
}

public class Camera2D
{
    private Viewport viewport;
    private Vector2 position;
    private float zoom;
    private float rotation;

    public Camera2D(Viewport viewport)
    {
        this.viewport = viewport;
        position = Vector2.Zero;
        zoom = 1f;
        rotation = 0f;
    }

    public Matrix TransformMatrix
    {
        get
        {
            return Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0)) *
                   Matrix.CreateRotationZ(rotation) *
                   Matrix.CreateScale(zoom) *
                   Matrix.CreateTranslation(new Vector3(viewport.Width / 2, viewport.Height / 2, 0));
        }
    }

    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    public float Zoom
    {
        get { return zoom; }
        set { zoom = value; }
    }

    public float Rotation
    {
        get { return rotation; }
        set { rotation = value; }
    }
}

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
