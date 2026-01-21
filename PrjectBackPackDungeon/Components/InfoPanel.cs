using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class InfoPanel
{
    private Rectangle _bounds;
    private Texture2D _pixel;
    private SpriteFont _font;
    
    // Sous-zones
    private Rectangle _statsArea;

    // Stats à afficher
    private int _str;
    private int _dex;
    private int _int;
    private int _luck;

    public InfoPanel(Rectangle bounds, GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _bounds = bounds;
        _font = font;

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _statsArea = new Rectangle(bounds.X + 10, bounds.Y + 10, bounds.Width - 20, bounds.Height - 20);
    }

    public void UpdateStats(int str, int dex, int intelligence, int luck)
    {
        _str = str;
        _dex = dex;
        _int = intelligence;
        _luck = luck;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Fond du panneau Stats
        spriteBatch.Draw(_pixel, _statsArea, Color.Black * 0.5f);
        
        // Titre "STATS"
        if (_font != null)
        {
            spriteBatch.DrawString(_font, "STATS", new Vector2(_statsArea.X + 10, _statsArea.Y + 5), Color.DarkGray);
        }

        // Attributs
        int startY = _statsArea.Y + 50;
        DrawAttribute(spriteBatch, Color.Orange, "STR", _str.ToString(), startY);
        DrawAttribute(spriteBatch, Color.Green, "DEX", _dex.ToString(), startY + 60);
        DrawAttribute(spriteBatch, Color.Purple, "INT", _int.ToString(), startY + 120);
        DrawAttribute(spriteBatch, Color.Yellow, "LUCK", _luck.ToString(), startY + 180);
    }

    private void DrawAttribute(SpriteBatch spriteBatch, Color color, string label, string value, int y)
    {
        // Icône
        spriteBatch.Draw(_pixel, new Rectangle(_statsArea.X + 10, y, 40, 40), color);
        
        if (_font != null)
        {
            spriteBatch.DrawString(_font, label, new Vector2(_statsArea.X + 60, y), Color.White);
            spriteBatch.DrawString(_font, value, new Vector2(_statsArea.X + 60, y + 20), Color.Gold);
        }
        else
        {
            // Fallback
            spriteBatch.Draw(_pixel, new Rectangle(_statsArea.X + 60, y + 15, 150, 10), Color.White);
        }
    }
}