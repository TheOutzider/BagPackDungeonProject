using System;
using Microsoft.Xna.Framework;

namespace PrjectBackPackDungeon;

public enum DiceState { Waiting, Popping, Idle }

public class Dice
{
    public DiceType Type { get; private set; }
    public int Value { get; private set; }
    public int MaxValue { get; private set; }
    public DiceState State { get; private set; }
    
    public Vector2 Position;
    public float Rotation; // Rotation 2D simple
    public float Scale { get; private set; }
    public Color Color { get; private set; }

    private float _timer;
    private float _delay;
    private Vector2 _basePosition;
    private Vector2 _shakeOffset;
    private Random _rng = new Random();

    public bool IsCritical => Value == MaxValue;
    public bool IsFumble => Value == 1;
    public bool IsVisible => State != DiceState.Waiting;

    public Dice(DiceType type, Vector2 screenPos, int targetValue, int maxValue, Color color, float delay)
    {
        Type = type;
        Value = targetValue;
        MaxValue = maxValue;
        Color = color;
        _delay = delay;
        
        _basePosition = screenPos;
        Position = _basePosition;
        Rotation = 0f;
        Scale = 0f;
        State = DiceState.Waiting;
        _timer = 0f;
    }

    public bool Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (State == DiceState.Waiting)
        {
            _delay -= dt;
            if (_delay <= 0)
            {
                State = DiceState.Popping;
                _timer = 0;
                return true; 
            }
            return false;
        }

        _timer += dt;

        if (State == DiceState.Popping)
        {
            // Effet de Pop 2D
            Scale = MathHelper.Lerp(Scale, 1.4f, 20f * dt);
            if (_timer > 0.1f)
            {
                Scale = 1.0f;
                State = DiceState.Idle;
            }
        }

        // Feedback continu
        if (IsFumble)
        {
            _shakeOffset = new Vector2(_rng.NextSingle() * 4 - 2, _rng.NextSingle() * 4 - 2);
            Rotation = (float)Math.Sin(_timer * 20f) * 0.1f;
        }
        else if (IsCritical)
        {
            _shakeOffset = new Vector2(0, (float)Math.Sin(_timer * 8f) * 8f);
            Rotation += dt * 2f; // Tourne lentement
        }
        else
        {
            _shakeOffset = Vector2.Zero;
            Rotation = 0f;
        }

        Position = _basePosition + _shakeOffset;
        return false;
    }
}