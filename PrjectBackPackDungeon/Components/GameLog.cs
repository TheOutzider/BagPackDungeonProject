using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class GameLog
{
    private Rectangle _bounds;
    private Texture2D _pixel;
    private SpriteFont _font;
    
    private List<string> _messages;
    private List<Color> _colors;
    private const int MaxMessages = 10; 

    public GameLog(Rectangle bounds, GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _bounds = bounds;
        _font = font;
        _messages = new List<string>();
        _colors = new List<Color>();

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        AddMessage("Welcome to the Dungeon!", Color.White);
        AddMessage("Drag items to your inventory.", Color.Gray);
        AddMessage("Press SPACE to roll dice.", Color.Gold);
    }

    public void AddMessage(string message, Color color)
    {
        _messages.Add(message);
        _colors.Add(color);
        
        if (_messages.Count > MaxMessages)
        {
            _messages.RemoveAt(0);
            _colors.RemoveAt(0);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // 1. Fond (Parchemin sombre)
        spriteBatch.Draw(_pixel, _bounds, new Color(25, 25, 30));
        
        // 2. Bordures décoratives
        int b = 3;
        Color borderColor = new Color(60, 60, 70);
        spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, b), borderColor); // Top
        spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Bottom - b, _bounds.Width, b), borderColor); // Bottom
        spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, b, _bounds.Height), borderColor); // Left
        spriteBatch.Draw(_pixel, new Rectangle(_bounds.Right - b, _bounds.Y, b, _bounds.Height), borderColor); // Right

        // 3. Titre "JOURNAL"
        if (_font != null)
        {
            string title = "JOURNAL";
            Vector2 titleSize = _font.MeasureString(title) * 0.7f;
            // Petit fond pour le titre
            spriteBatch.Draw(_pixel, new Rectangle(_bounds.X + 20, _bounds.Y - 10, (int)titleSize.X + 20, 20), new Color(25, 25, 30));
            spriteBatch.DrawString(_font, title, new Vector2(_bounds.X + 30, _bounds.Y - 12), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
        }

        // 4. Dessin des messages
        int y = _bounds.Y + 15;
        int lineHeight = 24;
        
        for (int i = 0; i < _messages.Count; i++)
        {
            if (_font != null)
            {
                // On dessine une petite ombre sous le texte pour la lisibilité
                spriteBatch.DrawString(_font, _messages[i], new Vector2(_bounds.X + 22, y + 2), Color.Black * 0.5f, 0f, Vector2.Zero, 0.85f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_font, _messages[i], new Vector2(_bounds.X + 20, y), _colors[i], 0f, Vector2.Zero, 0.85f, SpriteEffects.None, 0f);
            }
            
            y += lineHeight;
        }
    }
}
