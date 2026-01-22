using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class InfoPanel
{
    private Rectangle _bounds;
    private Texture2D _pixel;
    private SpriteFont _font;
    
    private Rectangle _statsArea;
    private Rectangle _skillsArea;

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

        // On divise le panneau en deux : Stats en haut, Skills en bas
        int padding = 15;
        _statsArea = new Rectangle(bounds.X + padding, bounds.Y + padding, bounds.Width - padding * 2, (int)(bounds.Height * 0.4f));
        _skillsArea = new Rectangle(bounds.X + padding, _statsArea.Bottom + padding, bounds.Width - padding * 2, bounds.Height - _statsArea.Height - padding * 3);
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
        // 1. Fond des panneaux avec bordures
        DrawSection(spriteBatch, _statsArea, "ATTRIBUTES", new Color(40, 40, 45));
        DrawSection(spriteBatch, _skillsArea, "SKILLS", new Color(40, 40, 45));

        // 2. Dessin des Attributs
        int startY = _statsArea.Y + 60;
        int spacing = 70;
        DrawAttribute(spriteBatch, Color.OrangeRed, "STR", _str.ToString(), startY);
        DrawAttribute(spriteBatch, Color.LimeGreen, "DEX", _dex.ToString(), startY + spacing);
        DrawAttribute(spriteBatch, Color.DodgerBlue, "INT", _int.ToString(), startY + spacing * 2);
        DrawAttribute(spriteBatch, Color.Gold, "LUCK", _luck.ToString(), startY + spacing * 3);
    }

    private void DrawSection(SpriteBatch sb, Rectangle area, string title, Color bgColor)
    {
        // Ombre
        sb.Draw(_pixel, new Rectangle(area.X + 4, area.Y + 4, area.Width, area.Height), Color.Black * 0.4f);
        // Fond
        sb.Draw(_pixel, area, bgColor);
        // Bordure
        int b = 2;
        sb.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, b), Color.Gray * 0.5f);
        sb.Draw(_pixel, new Rectangle(area.X, area.Bottom - b, area.Width, b), Color.Gray * 0.5f);
        sb.Draw(_pixel, new Rectangle(area.X, area.Y, b, area.Height), Color.Gray * 0.5f);
        sb.Draw(_pixel, new Rectangle(area.Right - b, area.Y, b, area.Height), Color.Gray * 0.5f);

        // Titre
        if (_font != null)
        {
            Vector2 titleSize = _font.MeasureString(title) * 0.8f;
            sb.DrawString(_font, title, new Vector2(area.X + 10, area.Y - 10), Color.Gold, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
        }
    }

    private void DrawAttribute(SpriteBatch spriteBatch, Color color, string label, string value, int y)
    {
        // Petite barre de fond pour l'attribut
        Rectangle bar = new Rectangle(_statsArea.X + 10, y, _statsArea.Width - 20, 50);
        spriteBatch.Draw(_pixel, bar, Color.Black * 0.2f);
        
        // Icône colorée
        spriteBatch.Draw(_pixel, new Rectangle(bar.X + 5, y + 5, 40, 40), color);
        
        if (_font != null)
        {
            spriteBatch.DrawString(_font, label, new Vector2(bar.X + 55, y + 5), Color.White * 0.9f, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, value, new Vector2(bar.X + 55, y + 22), Color.White, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, 0f);
        }
    }

    public Rectangle GetSkillsArea() => _skillsArea;
}
