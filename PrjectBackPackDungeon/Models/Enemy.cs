using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class Enemy
{
    public string Name { get; private set; }
    public int Hp { get; private set; }
    public int MaxHp { get; private set; }
    
    public int MinDamage { get; private set; }
    public int MaxDamage { get; private set; }
    
    public Rectangle Bounds { get; set; }
    private Texture2D _texture;
    private SpriteFont _font;
    private Random _random;

    // Animation
    private float _scale = 1.0f;
    private Color _color = Color.White;
    private float _flashTime = 0f;
    private Vector2 _shakeOffset = Vector2.Zero;
    private float _shakeTime = 0f;

    public bool IsDead => Hp <= 0;

    public Enemy(string name, int hp, int minDamage, int maxDamage, GraphicsDevice graphicsDevice, SpriteFont font)
    {
        Name = name;
        MaxHp = hp;
        Hp = hp;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        _font = font;
        _random = new Random();

        _texture = new Texture2D(graphicsDevice, 1, 1);
        _texture.SetData(new[] { Color.White });
        
        Bounds = new Rectangle(0, 0, 100, 100);
    }

    public void TakeDamage(int amount)
    {
        Hp -= amount;
        if (Hp < 0) Hp = 0;
        
        // Feedback
        _scale = 1.2f; // Punch
        _flashTime = 0.2f; // Flash rouge
        _shakeTime = 0.3f; // Shake
    }

    public int Attack()
    {
        // Feedback attaque
        _scale = 1.1f;
        return _random.Next(MinDamage, MaxDamage + 1);
    }
    
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Retour à l'échelle normale (Elasticité)
        _scale = MathHelper.Lerp(_scale, 1.0f, 10f * dt);
        
        // Flash
        if (_flashTime > 0)
        {
            _flashTime -= dt;
            _color = Color.Red;
        }
        else
        {
            _color = Color.White;
        }
        
        // Shake
        if (_shakeTime > 0)
        {
            _shakeTime -= dt;
            _shakeOffset = new Vector2((float)(_random.NextDouble() * 10 - 5), (float)(_random.NextDouble() * 10 - 5));
        }
        else
        {
            _shakeOffset = Vector2.Zero;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsDead) return;

        // Calcul du rectangle transformé
        Vector2 center = new Vector2(Bounds.Center.X, Bounds.Center.Y);
        Vector2 size = new Vector2(Bounds.Width, Bounds.Height) * _scale;
        Vector2 pos = center - (size / 2) + _shakeOffset;
        
        Rectangle drawRect = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);

        // Dessin de l'ennemi
        spriteBatch.Draw(_texture, drawRect, _color);
        
        // Nom de l'ennemi
        if (_font != null)
        {
            Vector2 textSize = _font.MeasureString(Name);
            Vector2 textPos = new Vector2(center.X - textSize.X / 2, drawRect.Top - 35);
            spriteBatch.DrawString(_font, Name, textPos, Color.White);
        }
        
        // Barre de vie
        int barWidth = (int)size.X;
        int barHeight = 10;
        int barY = drawRect.Top - 15;
        
        spriteBatch.Draw(_texture, new Rectangle((int)pos.X, barY, barWidth, barHeight), Color.Gray);
        float ratio = (float)Hp / MaxHp;
        spriteBatch.Draw(_texture, new Rectangle((int)pos.X, barY, (int)(barWidth * ratio), barHeight), Color.Green);
        
        // Texte PV
        if (_font != null)
        {
            string hpText = $"{Hp}/{MaxHp}";
            Vector2 hpSize = _font.MeasureString(hpText);
            spriteBatch.DrawString(_font, hpText, new Vector2(center.X - hpSize.X / 2, barY - 2), Color.White);
            
            string dmgText = $"ATK: {MinDamage}-{MaxDamage}";
            spriteBatch.DrawString(_font, dmgText, new Vector2(drawRect.Right + 10, center.Y), Color.Red);
        }
    }
}