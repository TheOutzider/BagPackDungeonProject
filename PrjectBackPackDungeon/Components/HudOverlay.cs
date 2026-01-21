using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class HudOverlay
{
    private Rectangle _sceneBounds; // Zone B (Centre)
    private Texture2D _circleTexture;
    private SpriteFont _font; // Ajout de la font pour afficher le texte

    // Données affichées
    private float _hpRatio = 1f;
    private float _manaRatio = 1f;
    private string _hpText = "";
    private string _floorText = ""; // Texte de l'étage

    public HudOverlay(Rectangle sceneBounds, GraphicsDevice graphicsDevice, SpriteFont font = null)
    {
        _sceneBounds = sceneBounds;
        _font = font;

        // Création d'une texture de cercle (comme pour les dés)
        int size = 100;
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
    }

    public void UpdateStats(int currentHp, int maxHp, int currentMana, int maxMana, int dungeonLevel = 1, int floorNumber = 1)
    {
        _hpRatio = (float)currentHp / maxHp;
        _manaRatio = (float)currentMana / maxMana;
        _hpText = $"{currentHp}/{maxHp}";
        _floorText = $"Floor {dungeonLevel}-{floorNumber}";
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int orbRadius = 80;
        int portraitRadius = 60;
        
        // Positionnement en bas de la zone centrale
        int yPos = _sceneBounds.Bottom - orbRadius - 20;
        int centerX = _sceneBounds.Center.X;

        // Orbe de Vie (Gauche)
        Vector2 hpOrbPos = new Vector2(centerX - 150, yPos);
        DrawOrb(spriteBatch, hpOrbPos, orbRadius, Color.DarkRed, Color.Red, _hpRatio);

        // Orbe de Mana (Droite)
        Vector2 manaOrbPos = new Vector2(centerX + 150, yPos);
        DrawOrb(spriteBatch, manaOrbPos, orbRadius, Color.DarkBlue, Color.Blue, _manaRatio);
        
        // Portrait "Doom-style" (Centre)
        Vector2 portraitPos = new Vector2(centerX, yPos);
        DrawPortrait(spriteBatch, portraitPos, portraitRadius);
        
        // Affichage du texte de l'étage (En haut à droite de la zone de jeu)
        if (_font != null && !string.IsNullOrEmpty(_floorText))
        {
            Vector2 textSize = _font.MeasureString(_floorText);
            Vector2 textPos = new Vector2(_sceneBounds.Right - textSize.X - 20, _sceneBounds.Top + 20);
            spriteBatch.DrawString(_font, _floorText, textPos, Color.Gold);
        }
    }

    private void DrawOrb(SpriteBatch spriteBatch, Vector2 position, int radius, Color bgColor, Color fgColor, float fillRatio)
    {
        float scale = (radius * 2f) / _circleTexture.Width;
        Vector2 origin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);

        // Fond de l'orbe
        spriteBatch.Draw(_circleTexture, position, null, bgColor, 0f, origin, scale, SpriteEffects.None, 0f);

        // Remplissage (avec un masque pour ne dessiner que la partie basse)
        int fillHeight = (int)(_circleTexture.Height * fillRatio);
        if (fillHeight > 0)
        {
            Rectangle sourceRect = new Rectangle(0, _circleTexture.Height - fillHeight, _circleTexture.Width, fillHeight);
            Vector2 fillPosition = position + new Vector2(0, radius - (fillHeight * scale));
            spriteBatch.Draw(_circleTexture, fillPosition, sourceRect, fgColor, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        
        // Cadre par-dessus
        spriteBatch.Draw(_circleTexture, position, null, Color.Black, 0f, origin, scale * 0.95f, SpriteEffects.None, 0f);
    }

    private void DrawPortrait(SpriteBatch spriteBatch, Vector2 position, int radius)
    {
        float scale = (radius * 2f) / _circleTexture.Width;
        Vector2 origin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);
        
        // Fond du portrait
        spriteBatch.Draw(_circleTexture, position, null, Color.Gray, 0f, origin, scale, SpriteEffects.None, 0f);
        
        // "Visage" (un simple carré pour l'instant)
        // Change de couleur selon les PV ?
        Color faceColor = _hpRatio > 0.5f ? Color.SandyBrown : (_hpRatio > 0.2f ? Color.Orange : Color.Red);
        spriteBatch.Draw(_circleTexture, position, null, faceColor, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
    }
}