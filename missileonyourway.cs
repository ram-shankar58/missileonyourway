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

public class Game1 : Game
{
    // ...

    public void PlayExplosionEffect(Vector2 position)
    {
        // Trigger particle effect at the specified position
        // Replace this with your actual particle effect triggering logic
        // For example:
        // explosionParticles.Position = position;
        // explosionParticles.Trigger();
    }

    // ...
}

