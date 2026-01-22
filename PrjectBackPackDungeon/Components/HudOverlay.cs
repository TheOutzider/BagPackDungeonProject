using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrjectBackPackDungeon;

public enum PortraitState
{
    Idle,
    Damaged,
    Healing,
    Attacking,
    Injured50, 
    Injured25, 
    Injured5   
}

public class HudOverlay
{
    private Rectangle _sceneBounds; 
    private Texture2D _circleTexture;
    private Texture2D _pixel;
    private SpriteFont _font;
    private ContentManager _content;

    private float _hpRatio = 1f;
    private float _manaRatio = 1f;
    private string _hpText = "";
    private string _manaText = "";
    private string _floorText = "";
    
    private float _waveTimer = 0f;

    private PortraitState _currentPortraitState;
    private PortraitState _basePortraitState; 
    private float _stateTimer; 

    private Dictionary<PortraitState, Texture2D> _portraitTextures;
    private PlayerClass _playerClass;

    public HudOverlay(Rectangle sceneBounds, GraphicsDevice graphicsDevice, SpriteFont font = null, ContentManager content = null)
    {
        _sceneBounds = sceneBounds;
        _font = font;
        _content = content;

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        int size = 256; 
        _circleTexture = new Texture2D(graphicsDevice, size, size);
        Color[] data = new Color[size * size];
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                    data[y * size + x] = Color.White;
                else
                    data[y * size + x] = Color.Transparent;
            }
        }
        _circleTexture.SetData(data);

        _currentPortraitState = PortraitState.Idle;
        _basePortraitState = PortraitState.Idle;
        _stateTimer = 0f;
        _portraitTextures = new Dictionary<PortraitState, Texture2D>();
    }

    public void SetPlayerClass(PlayerClass pClass)
    {
        _playerClass = pClass;
        _portraitTextures.Clear();
        string prefix = pClass.ToString().ToLower();

        try {
            _portraitTextures[PortraitState.Idle] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_idle");
            _portraitTextures[PortraitState.Attacking] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_attack");
            _portraitTextures[PortraitState.Damaged] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_hit");
            _portraitTextures[PortraitState.Healing] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_heal");
            _portraitTextures[PortraitState.Injured50] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_minus_50");
            _portraitTextures[PortraitState.Injured25] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_minus_25");
            _portraitTextures[PortraitState.Injured5] = _content.Load<Texture2D>($"PNG/portrait_{prefix}_minus_25"); 
        } catch {
            System.Diagnostics.Debug.WriteLine($"Failed to load some portraits for {pClass}");
        }
    }

    public void UpdateStats(int currentHp, int maxHp, int currentMana, int maxMana, int dungeonLevel = 1, int floorNumber = 1)
    {
        _hpRatio = MathHelper.Clamp((float)currentHp / maxHp, 0, 1);
        _manaRatio = MathHelper.Clamp((float)currentMana / maxMana, 0, 1);
        _hpText = $"{currentHp}/{maxHp}";
        _manaText = $"{currentMana}/{maxMana}";
        _floorText = $"Floor {dungeonLevel}-{floorNumber}";
    }

    public void Update(GameTime gameTime, int currentHp, int maxHp)
    {
        _waveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_stateTimer > 0)
        {
            _stateTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_stateTimer <= 0) _currentPortraitState = _basePortraitState;
        }
        else
        {
            float hpRatio = (float)currentHp / maxHp;
            if (hpRatio <= 0.25f) _basePortraitState = PortraitState.Injured25;
            else if (hpRatio <= 0.50f) _basePortraitState = PortraitState.Injured50;
            else _basePortraitState = PortraitState.Idle;

            _currentPortraitState = _basePortraitState;
        }
    }

    public void SetPortraitState(PortraitState state, float duration = 0f)
    {
        _currentPortraitState = state;
        _stateTimer = duration;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int orbRadius = 75; 
        int portraitSize = 160; // Taille du cadre carré
        int margin = 10;
        int yOrbPos = _sceneBounds.Bottom - orbRadius - margin;
        int centerX = _sceneBounds.Center.X;

        DrawOrb(spriteBatch, new Vector2(_sceneBounds.Left + orbRadius + margin, yOrbPos), orbRadius, new Color(40, 0, 0), Color.Red, _hpRatio, _hpText, "HP");
        DrawOrb(spriteBatch, new Vector2(_sceneBounds.Right - orbRadius - margin, yOrbPos), orbRadius, new Color(0, 0, 40), Color.DodgerBlue, _manaRatio, _manaText, "MP");
        
        // Portrait (Cadre Carré)
        DrawPortrait(spriteBatch, new Vector2(centerX, _sceneBounds.Bottom), portraitSize);
        
        if (_font != null && !string.IsNullOrEmpty(_floorText))
        {
            Vector2 textSize = _font.MeasureString(_floorText);
            Vector2 textPos = new Vector2(_sceneBounds.Right - textSize.X - 20, _sceneBounds.Top + 20);
            spriteBatch.DrawString(_font, _floorText, textPos, Color.Gold);
        }
    }

    private void DrawOrb(SpriteBatch spriteBatch, Vector2 position, int radius, Color bgColor, Color fgColor, float fillRatio, string valueText, string label)
    {
        float scale = (radius * 2f) / _circleTexture.Width;
        Vector2 origin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);

        spriteBatch.Draw(_circleTexture, position, null, new Color(50, 50, 55), 0f, origin, scale * 1.08f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_circleTexture, position, null, new Color(10, 10, 15), 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(_circleTexture, position, null, bgColor, 0f, origin, scale * 0.95f, SpriteEffects.None, 0f);

        if (fillRatio > 0)
        {
            int texSize = _circleTexture.Width;
            int baseFillHeight = (int)(texSize * fillRatio);
            float amplitude = 6f; float frequency = 0.08f; float speed = 5f; 
            int step = 2;
            for (int i = 0; i < texSize; i += step)
            {
                float waveOffset = (float)Math.Sin(_waveTimer * speed + i * frequency) * amplitude;
                int currentFillHeight = Math.Clamp(baseFillHeight + (int)waveOffset, 0, texSize);
                if (currentFillHeight > 0)
                {
                    Rectangle sourceRect = new Rectangle(i, texSize - currentFillHeight, step, currentFillHeight);
                    Vector2 stripPos = position + new Vector2((-texSize / 2f + i) * scale, radius - (currentFillHeight * scale));
                    spriteBatch.Draw(_circleTexture, stripPos, sourceRect, fgColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }
        spriteBatch.Draw(_circleTexture, position - new Vector2(radius * 0.3f, radius * 0.3f), null, Color.White * 0.12f, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);

        if (_font != null)
        {
            float textScale = 0.8f;
            Vector2 textSize = _font.MeasureString(valueText) * textScale;
            spriteBatch.DrawString(_font, valueText, position - textSize / 2, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            Vector2 labelSize = _font.MeasureString(label) * 0.7f;
            spriteBatch.DrawString(_font, label, position - new Vector2(labelSize.X / 2, radius + 25), Color.Gold * 0.9f, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
        }
    }

    private void DrawPortrait(SpriteBatch spriteBatch, Vector2 position, int size)
    {
        // On centre le portrait sur la position X, et on l'aligne verticalement pour qu'il dépasse sur le journal
        Rectangle frameRect = new Rectangle((int)position.X - size / 2, (int)position.Y - size / 2, size, size);
        
        // 1. Ombre portée
        spriteBatch.Draw(_pixel, new Rectangle(frameRect.X + 6, frameRect.Y + 6, frameRect.Width, frameRect.Height), Color.Black * 0.6f);

        // 2. Fond du cadre (Plaque métallique sombre)
        spriteBatch.Draw(_pixel, frameRect, new Color(40, 40, 45));
        
        // 3. Bordure décorative (Style Plaque)
        int b = 4;
        Color borderColor = new Color(180, 140, 40); // Or
        spriteBatch.Draw(_pixel, new Rectangle(frameRect.X, frameRect.Y, frameRect.Width, b), borderColor); // Top
        spriteBatch.Draw(_pixel, new Rectangle(frameRect.X, frameRect.Bottom - b, frameRect.Width, b), borderColor); // Bottom
        spriteBatch.Draw(_pixel, new Rectangle(frameRect.X, frameRect.Y, b, frameRect.Height), borderColor); // Left
        spriteBatch.Draw(_pixel, new Rectangle(frameRect.Right - b, frameRect.Y, b, frameRect.Height), borderColor); // Right

        // 4. Texture du portrait
        if (_portraitTextures.ContainsKey(_currentPortraitState))
        {
            Texture2D tex = _portraitTextures[_currentPortraitState];
            // On dessine le portrait à l'intérieur du cadre (avec un petit padding)
            int padding = 8;
            Rectangle destRect = new Rectangle(frameRect.X + padding, frameRect.Y + padding, frameRect.Width - padding * 2, frameRect.Height - padding * 2);
            spriteBatch.Draw(tex, destRect, Color.White);
        }
        else
        {
            // Fallback si pas de texture
            Color faceColor = _hpRatio > 0.5f ? Color.SandyBrown : (_hpRatio > 0.2f ? Color.Orange : Color.Red);
            spriteBatch.Draw(_pixel, new Rectangle(frameRect.X + 10, frameRect.Y + 10, frameRect.Width - 20, frameRect.Height - 20), faceColor);
        }

        // 5. Texte "YOU"
        if (_font != null)
        {
            string text = "YOU";
            Vector2 textSize = _font.MeasureString(text) * 0.8f;
            spriteBatch.DrawString(_font, text, new Vector2(position.X - textSize.X / 2, frameRect.Bottom - 25), Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
        }
    }
}
