using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class GameLog
{
    private Rectangle _bounds;
    private Texture2D _pixel;
    private SpriteFont _font;
    
    // Liste des messages
    private List<string> _messages;
    private List<Color> _colors;
    private const int MaxMessages = 8; // Nombre de lignes visibles

    public GameLog(Rectangle bounds, GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _bounds = bounds;
        _font = font;
        _messages = new List<string>();
        _colors = new List<Color>();

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        // Messages de bienvenue
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
        // Fond du Log (Noir semi-transparent)
        spriteBatch.Draw(_pixel, _bounds, Color.Black * 0.8f);
        
        // Bordure haute
        spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, 2), Color.Gray);

        // Dessin des lignes
        int y = _bounds.Y + 10;
        int lineHeight = 20;
        
        for (int i = 0; i < _messages.Count; i++)
        {
            if (_font != null)
            {
                spriteBatch.DrawString(_font, _messages[i], new Vector2(_bounds.X + 10, y), _colors[i]);
            }
            else
            {
                // Fallback si pas de font
                int textWidth = _messages[i].Length * 10; 
                spriteBatch.Draw(_pixel, new Rectangle(_bounds.X + 10, y, textWidth, 10), _colors[i]);
            }
            
            y += lineHeight;
        }
        
        // Indicateur "LOG"
        if (_font != null)
        {
            spriteBatch.DrawString(_font, "LOG", new Vector2(_bounds.Right - 40, _bounds.Top + 5), Color.White);
        }
    }
}