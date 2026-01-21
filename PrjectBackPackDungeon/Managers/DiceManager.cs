using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class DiceManager
{
    private List<Dice> _dices;
    private Rectangle _arenaBounds;
    private EffectManager _effectManager;
    private SpriteFont _font;
    private Texture2D _pixel;
    
    // Textures des dés
    private Dictionary<DiceType, Texture2D> _diceTextures;
    
    public bool IsRolling { get; private set; }
    public event Action<int> OnTurnFinished;
    
    private float _finishTimer;
    private bool _impactTriggered;
    private bool _turnEndedTriggered;
    private PlayerClass _currentPlayerClass;

    public DiceManager(GraphicsDevice graphicsDevice, Rectangle arenaBounds, SpriteFont font, Microsoft.Xna.Framework.Content.ContentManager content)
    {
        _dices = new List<Dice>();
        _arenaBounds = arenaBounds;
        _font = font;
        
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Chargement des textures de dés
        _diceTextures = new Dictionary<DiceType, Texture2D>();
        try {
            _diceTextures[DiceType.D4_Basic] = content.Load<Texture2D>("PNG/dice_d4");
            _diceTextures[DiceType.D6_Fire] = content.Load<Texture2D>("PNG/dice_d6");
            _diceTextures[DiceType.D6_Ice] = content.Load<Texture2D>("PNG/dice_d6");
            _diceTextures[DiceType.D8_Basic] = content.Load<Texture2D>("PNG/dice_d8");
            _diceTextures[DiceType.D10_Basic] = content.Load<Texture2D>("PNG/dice_d10");
            _diceTextures[DiceType.D12_Basic] = content.Load<Texture2D>("PNG/dice_d12");
            _diceTextures[DiceType.D20_Steel] = content.Load<Texture2D>("PNG/dice_d20");
        } catch {
            System.Diagnostics.Debug.WriteLine("Failed to load some dice textures!");
        }
    }

    public void SetEffectManager(EffectManager em) => _effectManager = em;

    public void ThrowDices(List<Item> items, PlayerClass pClass)
    {
        if (IsRolling) return;

        _dices.Clear();
        _turnEndedTriggered = false;
        _impactTriggered = false;
        _finishTimer = 0;
        _currentPlayerClass = pClass;
        
        Random rng = new Random();
        var diceItems = items.Where(i => i.DiceType != DiceType.None).ToList();
        int count = diceItems.Count;
        
        for (int i = 0; i < count; i++)
        {
            var item = diceItems[i];
            int cols = (int)Math.Ceiling(Math.Sqrt(count));
            int row = i / cols;
            int col = i % cols;
            
            float spacing = 160f; // Plus d'espace pour les sprites
            float startX = _arenaBounds.Center.X - ((cols - 1) * spacing) / 2f;
            float startY = _arenaBounds.Center.Y - ((count / cols) * spacing) / 2f;
            
            Vector2 targetPos = new Vector2(startX + col * spacing, startY + row * spacing);
            
            int maxVal = GetMaxVal(item.DiceType);
            int targetValue = rng.Next(1, maxVal + 1);
            float delay = i * 0.3f;
            
            _dices.Add(new Dice(item.DiceType, targetPos, targetValue, maxVal, GetColorForDice(item.DiceType), delay));
        }

        if (_dices.Count > 0) IsRolling = true;
    }

    private int GetMaxVal(DiceType type)
    {
        return type switch {
            DiceType.D4_Basic => 4,
            DiceType.D8_Basic => 8,
            DiceType.D10_Basic => 10,
            DiceType.D12_Basic => 12,
            DiceType.D20_Steel => 20,
            _ => 6
        };
    }

    private Color GetColorForDice(DiceType type)
    {
        return type switch {
            DiceType.D4_Basic => Color.LimeGreen,
            DiceType.D6_Fire => Color.OrangeRed,
            DiceType.D6_Ice => Color.CornflowerBlue,
            DiceType.D8_Basic => Color.MediumPurple,
            DiceType.D10_Basic => Color.DodgerBlue,
            DiceType.D12_Basic => Color.Crimson,
            DiceType.D20_Steel => Color.Silver,
            _ => Color.White
        };
    }

    public void Update(GameTime gameTime)
    {
        if (!IsRolling) return;

        bool allFinished = true;
        foreach (var dice in _dices)
        {
            bool justPopped = dice.Update(gameTime);
            if (justPopped)
            {
                AudioManager.PlayRetroPop();
                _effectManager?.TriggerShake(2.0f, 0.1f);
                if (dice.IsCritical) _effectManager?.AddCriticalEffect(dice.Position, dice.Color);
                else if (dice.IsFumble) _effectManager?.AddFumbleEffect(dice.Position);
            }
            if (dice.State != DiceState.Idle) allFinished = false;
        }

        if (allFinished)
        {
            _finishTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_finishTimer > 0.3f && !_impactTriggered)
            {
                _impactTriggered = true;
                AudioManager.PlayRetroImpact();
                int total = _dices.Sum(d => d.Value);
                _effectManager?.AddTotalDamageImpact(new Vector2(_arenaBounds.Center.X, _arenaBounds.Center.Y), total, _currentPlayerClass);
            }
            if (_finishTimer > 1.5f && !_turnEndedTriggered)
            {
                _turnEndedTriggered = true;
                IsRolling = false;
                OnTurnFinished?.Invoke(_dices.Sum(d => d.Value));
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var dice in _dices)
        {
            if (!dice.IsVisible) continue;
            DrawDice2D(spriteBatch, dice);
        }
    }

    private void DrawDice2D(SpriteBatch spriteBatch, Dice dice)
    {
        if (!_diceTextures.ContainsKey(dice.Type)) return;
        Texture2D tex = _diceTextures[dice.Type];
        
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
        float baseScale = 100f / Math.Max(tex.Width, tex.Height); // On normalise la taille à ~100px
        float finalScale = baseScale * dice.Scale;

        // 1. Ombre portée
        spriteBatch.Draw(tex, dice.Position + new Vector2(8, 8), null, Color.Black * 0.3f, dice.Rotation, origin, finalScale, SpriteEffects.None, 0f);

        // 2. Le dé (teinté avec sa couleur)
        Color drawColor = dice.Color;
        if (dice.IsCritical) drawColor = Color.Lerp(dice.Color, Color.White, 0.4f);
        
        spriteBatch.Draw(tex, dice.Position, null, drawColor, dice.Rotation, origin, finalScale, SpriteEffects.None, 0f);

        // 3. Chiffre
        string text = dice.Value.ToString();
        Vector2 textSize = _font.MeasureString(text);
        float textScale = (dice.IsCritical ? 1.8f : 1.4f) * dice.Scale;
        
        // Ombre texte
        spriteBatch.DrawString(_font, text, dice.Position + new Vector2(2, 2), Color.Black * 0.8f, 0f, textSize / 2, textScale, SpriteEffects.None, 0f);
        // Texte principal
        Color textColor = dice.IsCritical ? Color.Gold : (dice.IsFumble ? Color.Red : Color.White);
        spriteBatch.DrawString(_font, text, dice.Position, textColor, 0f, textSize / 2, textScale, SpriteEffects.None, 0f);
    }

    public void DebugSpawnAllDice()
    {
        var items = new List<Item>();
        foreach (DiceType type in Enum.GetValues(typeof(DiceType)))
        {
            if (type != DiceType.None) items.Add(new Item("Debug", 1, 1, Color.White, type));
        }
        ThrowDices(items, PlayerClass.Warrior);
    }
}