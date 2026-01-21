using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class EffectManager
{
    private Random _random;
    private Texture2D _pixel;
    private SpriteFont _font;
    
    private Texture2D _texFingerLeft;
    private Texture2D _texFingerRight;
    private Texture2D _texArmLeft;
    private Texture2D _texArmRight;

    private class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Size;
        public float Life;     
        public float MaxLife;
        public float Rotation;
        public float RotationSpeed;
        public bool HasGravity;
        public bool IsSquare; 
    }
    private List<Particle> _particles;

    private enum ElementType { Text, ShyFingers, StrongArms, TotalDamage }

    private class FloatingElement
    {
        public ElementType Type;
        public string Text; 
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 BasePosition; 
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Scale;
        public float TargetScale;
        public float Rotation;
        public SpriteEffects Effects;
        public float AnimTimer;
    }
    private List<FloatingElement> _floatingElements;

    public EffectManager(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _random = new Random();
        _font = font;
        _particles = new List<Particle>();
        _floatingElements = new List<FloatingElement>();
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void SetFont(SpriteFont font) => _font = font;

    public void LoadContent(Texture2D fingerLeft, Texture2D fingerRight, Texture2D armLeft, Texture2D armRight)
    {
        _texFingerLeft = fingerLeft;
        _texFingerRight = fingerRight;
        _texArmLeft = armLeft;
        _texArmRight = armRight;
    }

    // --- EFFETS DE CLASSE ---

    public void AddTotalDamageImpact(Vector2 position, int amount, PlayerClass pClass)
    {
        _floatingElements.Add(new FloatingElement
        {
            Type = ElementType.TotalDamage,
            Text = amount.ToString(),
            Position = position,
            Color = Color.Yellow,
            Life = 1.5f, MaxLife = 1.5f,
            Scale = 0f, TargetScale = 3.0f 
        });

        for (int i = 0; i < 50; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = _random.Next(200, 600);
            _particles.Add(new Particle {
                Position = position,
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed,
                Color = _random.Next(2) == 0 ? Color.White : Color.Yellow,
                Size = _random.Next(4, 12),
                Life = 0.6f, MaxLife = 0.6f,
                IsSquare = true,
                HasGravity = false
            });
        }

        if (pClass == PlayerClass.Warrior) AddWarriorSlash(position);
        else if (pClass == PlayerClass.Rogue) AddRogueDoubleSlash(position);
        else AddMageArcaneBlast(position);

        TriggerShake(10f, 0.4f);
        TriggerFlash(Color.White * 0.5f, 0.15f);
    }

    public void AddWarriorSlash(Vector2 position)
    {
        for (int i = 0; i < 40; i++)
        {
            float offset = (i - 20) * 15f;
            Vector2 p = position + new Vector2(offset, offset * 0.3f);
            _particles.Add(new Particle { Position = p, Velocity = new Vector2(0, _random.Next(-50, 50)), Color = Color.White, Size = 6, Life = 0.4f, MaxLife = 0.4f, IsSquare = true });
        }
    }

    public void AddRogueDoubleSlash(Vector2 position)
    {
        for (int i = 0; i < 30; i++)
        {
            float offset = (i - 15) * 15f;
            _particles.Add(new Particle { Position = position + new Vector2(offset, offset), Color = Color.Red, Size = 8, Life = 0.4f, MaxLife = 0.4f, IsSquare = true });
            _particles.Add(new Particle { Position = position + new Vector2(offset, -offset), Color = Color.Red, Size = 8, Life = 0.4f, MaxLife = 0.4f, IsSquare = true });
        }
    }

    public void AddMageArcaneBlast(Vector2 position)
    {
        for (int i = 0; i < 60; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = _random.Next(300, 800);
            _particles.Add(new Particle { Position = position, Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, Color = Color.MediumPurple, Size = _random.Next(6, 15), Life = 0.7f, MaxLife = 0.7f, IsSquare = true });
        }
    }

    // --- API ---

    public void AddDamageText(Vector2 position, int amount, bool isCrit = false)
    {
        _floatingElements.Add(new FloatingElement { Type = ElementType.Text, Text = amount.ToString(), Position = position, Velocity = new Vector2(_random.Next(-50, 50), -150), Color = isCrit ? Color.Gold : Color.White, Life = 1.0f, MaxLife = 1.0f, Scale = isCrit ? 2.0f : 1.0f, TargetScale = isCrit ? 1.5f : 1.0f });
    }

    public void AddFumbleEffect(Vector2 position)
    {
        float fingerScale = 0.15f;
        _floatingElements.Add(new FloatingElement { Type = ElementType.ShyFingers, Texture = _texFingerLeft, BasePosition = position, Position = position + new Vector2(60, 20), Color = Color.White, Life = 3.0f, MaxLife = 3.0f, Scale = fingerScale, TargetScale = fingerScale });
        _floatingElements.Add(new FloatingElement { Type = ElementType.ShyFingers, Texture = _texFingerRight, BasePosition = position, Position = position + new Vector2(-60, 20), Color = Color.White, Life = 3.0f, MaxLife = 3.0f, Scale = fingerScale, TargetScale = fingerScale });
    }

    public void AddCriticalEffect(Vector2 position, Color diceColor)
    {
        float armScale = 0.25f;
        // INVERSION DES POSITIONS : ArmLeft à droite (+70), ArmRight à gauche (-70)
        _floatingElements.Add(new FloatingElement { 
            Type = ElementType.StrongArms, 
            Texture = _texArmLeft, 
            BasePosition = position, 
            Position = position + new Vector2(70, 0), // Était -70
            Color = Color.White, 
            Life = 3.0f, MaxLife = 3.0f, 
            Scale = armScale, TargetScale = armScale 
        });
        _floatingElements.Add(new FloatingElement { 
            Type = ElementType.StrongArms, 
            Texture = _texArmRight, 
            BasePosition = position, 
            Position = position + new Vector2(-70, 0), // Était +70
            Color = Color.White, 
            Life = 3.0f, MaxLife = 3.0f, 
            Scale = armScale, TargetScale = armScale 
        });

        for (int i = 0; i < 20; i++) {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = _random.Next(100, 300);
            _particles.Add(new Particle { Position = position, Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, Color = _random.Next(2) == 0 ? Color.Gold : diceColor, Size = _random.Next(4, 8), Life = 1.5f, MaxLife = 1.5f, HasGravity = true });
        }
    }

    public void AddText(Vector2 position, string text, Color color)
    {
        _floatingElements.Add(new FloatingElement { Type = ElementType.Text, Text = text, Position = position, Velocity = new Vector2(0, -60), Color = color, Life = 1.2f, MaxLife = 1.2f, Scale = 1.0f, TargetScale = 1.0f });
    }

    public void AddBloodEffect(Vector2 position, int count = 15)
    {
        for (int i = 0; i < count; i++) {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = _random.Next(100, 300);
            _particles.Add(new Particle { Position = position, Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, Color = Color.DarkRed, Size = _random.Next(3, 8), Life = 0.8f, MaxLife = 0.8f, HasGravity = true });
        }
    }

    public void AddHealEffect(Vector2 position, int count = 20)
    {
        for (int i = 0; i < count; i++) {
            _particles.Add(new Particle { Position = position + new Vector2(_random.Next(-40, 40), _random.Next(-40, 40)), Velocity = new Vector2(0, -_random.Next(50, 150)), Color = Color.LightGreen, Size = _random.Next(2, 5), Life = 1.5f, MaxLife = 1.5f, HasGravity = false });
        }
    }

    public void AddGoldEffect(Vector2 position, int count = 10)
    {
        for (int i = 0; i < count; i++) {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = _random.Next(50, 200);
            _particles.Add(new Particle { Position = position, Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, Color = Color.Gold, Size = _random.Next(3, 6), Life = 0.8f, MaxLife = 0.8f, HasGravity = true });
        }
    }

    public void AddExplosion(Vector2 position, Color color, int count = 10)
    {
        for (int i = 0; i < count; i++) {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = _random.Next(50, 250);
            _particles.Add(new Particle { Position = position, Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, Color = color, Size = _random.Next(3, 7), Life = 0.6f, MaxLife = 0.6f, HasGravity = false });
        }
    }

    // --- UPDATE & DRAW ---

    private float _shakeIntensity;
    private float _shakeDuration;
    private Vector2 _shakeOffset;
    private Color _flashColor;
    private float _flashDuration;
    private float _currentFlashTime;

    public void TriggerShake(float intensity, float duration) { _shakeIntensity = intensity; _shakeDuration = duration; }
    public void TriggerFlash(Color color, float duration) { _flashColor = color; _flashDuration = duration; _currentFlashTime = duration; }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_shakeDuration > 0) {
            _shakeDuration -= dt;
            _shakeOffset = new Vector2((float)(_random.NextDouble() * 2 - 1) * _shakeIntensity, (float)(_random.NextDouble() * 2 - 1) * _shakeIntensity);
        } else _shakeOffset = Vector2.Zero;

        if (_currentFlashTime > 0) _currentFlashTime -= dt;

        for (int i = _particles.Count - 1; i >= 0; i--) {
            var p = _particles[i];
            p.Position += p.Velocity * dt;
            p.Rotation += p.RotationSpeed * dt;
            p.Life -= dt;
            if (p.HasGravity) p.Velocity.Y += 600f * dt;
            else p.Velocity *= 0.94f;
            if (p.Life <= 0) _particles.RemoveAt(i); else _particles[i] = p;
        }

        for (int i = _floatingElements.Count - 1; i >= 0; i--) {
            var t = _floatingElements[i];
            t.AnimTimer += dt;
            t.Life -= dt;

            if (t.Type == ElementType.Text || t.Type == ElementType.TotalDamage) {
                t.Position += t.Velocity * dt;
                t.Scale = MathHelper.Lerp(t.Scale, t.TargetScale, 15f * dt);
            }
            else if (t.Type == ElementType.ShyFingers) {
                float slide = (float)Math.Abs(Math.Sin(t.AnimTimer * 6f)) * 40f;
                int dir = (t.Position.X > t.BasePosition.X) ? 1 : -1;
                t.Position.X = t.BasePosition.X + (dir * (60f - slide));
            }
            else if (t.Type == ElementType.StrongArms) {
                t.Rotation = (float)Math.Sin(t.AnimTimer * 8f) * 0.15f;
                t.Scale = t.TargetScale + (float)Math.Sin(t.AnimTimer * 10f) * 0.05f;
            }

            if (t.Life <= 0) _floatingElements.RemoveAt(i); else _floatingElements[i] = t;
        }
    }

    public Matrix GetShakeMatrix() => Matrix.CreateTranslation(_shakeOffset.X, _shakeOffset.Y, 0);

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var p in _particles) {
            float alpha = p.Life / p.MaxLife;
            Vector2 origin = p.IsSquare ? Vector2.Zero : new Vector2(0.5f);
            spriteBatch.Draw(_pixel, new Rectangle((int)p.Position.X, (int)p.Position.Y, (int)p.Size, (int)p.Size), null, p.Color * alpha, p.Rotation, origin, SpriteEffects.None, 0f);
        }

        foreach (var t in _floatingElements) {
            float alpha = Math.Min(1.0f, t.Life * 4);
            if (t.Texture != null) {
                Vector2 origin = new Vector2(t.Texture.Width / 2f, t.Texture.Height / 2f);
                spriteBatch.Draw(t.Texture, t.Position, null, t.Color * alpha, t.Rotation, origin, t.Scale, t.Effects, 0f);
            } else if (_font != null && t.Text != null) {
                Vector2 origin = _font.MeasureString(t.Text) / 2;
                Color c = t.Color * alpha;
                if (t.Type == ElementType.TotalDamage) {
                    spriteBatch.DrawString(_font, t.Text, t.Position + new Vector2(4,4), Color.Black * alpha, 0f, origin, t.Scale, SpriteEffects.None, 0f);
                }
                spriteBatch.DrawString(_font, t.Text, t.Position, c, 0f, origin, t.Scale, SpriteEffects.None, 0f);
            }
        }
    }

    public void DrawFlash(SpriteBatch spriteBatch, Rectangle bounds) { if (_currentFlashTime > 0) spriteBatch.Draw(_pixel, bounds, _flashColor * (_currentFlashTime / _flashDuration)); }
}