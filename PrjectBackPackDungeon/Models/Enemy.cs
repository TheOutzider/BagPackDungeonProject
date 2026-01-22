using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class Enemy
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    
    public Color Tint { get; set; } = Color.White;
    public List<StatusEffect> Effects { get; private set; } = new List<StatusEffect>();
    public List<EnemyAbility> Abilities { get; private set; } = new List<EnemyAbility>();
    
    public Rectangle Bounds { get; set; }
    private Texture2D _texture;
    private SpriteFont _font;
    private Random _random;

    private float _scale = 1.0f;
    private Color _currentColor = Color.White;
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

        if (graphicsDevice != null)
        {
            _texture = new Texture2D(graphicsDevice, 1, 1);
            _texture.SetData(new[] { Color.White });
        }
        
        Bounds = new Rectangle(0, 0, 100, 100);
        
        // Capacité par défaut
        Abilities.Add(new EnemyAbility("Attack", AbilityType.Attack, 0));
    }

    public EnemyAbility ChooseAbility()
    {
        // Si PV bas, plus de chance de se soigner si possible
        if (Hp < MaxHp * 0.3f)
        {
            var heal = Abilities.FirstOrDefault(a => a.Type == AbilityType.Heal);
            if (heal != null && _random.NextDouble() < 0.6) return heal;
        }
        
        return Abilities[_random.Next(Abilities.Count)];
    }

    public void ApplyEffect(StatusEffectType type, int duration, int intensity = 1)
    {
        var existing = Effects.FirstOrDefault(e => e.Type == type);
        if (existing != null)
        {
            existing.Duration += duration;
            existing.Intensity = Math.Max(existing.Intensity, intensity);
        }
        else
        {
            Effects.Add(new StatusEffect(type, duration, intensity));
        }
    }

    public int ProcessEffects()
    {
        int totalDamage = 0;
        var toRemove = new List<StatusEffect>();

        foreach (var effect in Effects)
        {
            switch (effect.Type)
            {
                case StatusEffectType.Poison:
                    totalDamage += effect.Intensity;
                    break;
                case StatusEffectType.Regen:
                    Hp = Math.Min(Hp + effect.Intensity, MaxHp);
                    break;
            }
            
            effect.Duration--;
            if (effect.Duration <= 0) toRemove.Add(effect);
        }

        foreach (var e in toRemove) Effects.Remove(e);
        if (totalDamage > 0) TakeDamage(totalDamage);
        return totalDamage;
    }

    public void TakeDamage(int amount)
    {
        int finalDamage = amount;
        var shield = Effects.FirstOrDefault(e => e.Type == StatusEffectType.Shield);
        if (shield != null)
        {
            if (shield.Intensity >= finalDamage)
            {
                shield.Intensity -= finalDamage;
                finalDamage = 0;
            }
            else
            {
                finalDamage -= shield.Intensity;
                Effects.Remove(shield);
            }
        }

        if (Effects.Any(e => e.Type == StatusEffectType.Vulnerable))
            finalDamage = (int)(finalDamage * 1.5f);

        Hp -= finalDamage;
        if (Hp < 0) Hp = 0;
        
        _scale = 1.2f;
        _flashTime = 0.2f;
        _shakeTime = 0.3f;
    }

    public int GetAttackDamage()
    {
        _scale = 1.1f;
        int dmg = _random.Next(MinDamage, MaxDamage + 1);
        if (Effects.Any(e => e.Type == StatusEffectType.Weak))
            dmg = (int)(dmg * 0.75f);
        return dmg;
    }
    
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _scale = MathHelper.Lerp(_scale, 1.0f, 10f * dt);
        
        if (_flashTime > 0)
        {
            _flashTime -= dt;
            _currentColor = Color.Red;
        }
        else
        {
            _currentColor = Tint;
        }
        
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
        if (IsDead || _texture == null) return;

        Vector2 center = new Vector2(Bounds.Center.X, Bounds.Center.Y);
        Vector2 size = new Vector2(Bounds.Width, Bounds.Height) * _scale;
        Vector2 pos = center - (size / 2) + _shakeOffset;
        Rectangle drawRect = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);

        spriteBatch.Draw(_texture, drawRect, _currentColor);
        
        if (_font != null)
        {
            Vector2 textSize = _font.MeasureString(Name);
            spriteBatch.DrawString(_font, Name, new Vector2(center.X - textSize.X / 2, drawRect.Top - 35), Color.White);
            
            string effectsText = string.Join(" ", Effects.Select(e => $"[{e.Type.ToString().Substring(0,1)}:{e.Intensity}]"));
            if (!string.IsNullOrEmpty(effectsText))
                spriteBatch.DrawString(_font, effectsText, new Vector2(drawRect.Left, drawRect.Bottom + 5), Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
        }
        
        int barWidth = (int)size.X;
        int barHeight = 10;
        int barY = drawRect.Top - 15;
        spriteBatch.Draw(_texture, new Rectangle((int)pos.X, barY, barWidth, barHeight), Color.Gray);
        float ratio = (float)Hp / MaxHp;
        spriteBatch.Draw(_texture, new Rectangle((int)pos.X, barY, (int)(barWidth * ratio), barHeight), Color.Green);
    }
}
