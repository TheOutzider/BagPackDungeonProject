using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrjectBackPackDungeon;

public class Button
{
    private Rectangle _bounds;
    private string _text;
    private SpriteFont _font;
    
    // Textures
    private Texture2D _textureIdle;
    private Texture2D _textureHover;
    private bool _hasTextures;
    private float _scale = 1f;
    
    // Couleurs (Fallback)
    private Texture2D _pixel;
    private Color _colorNormal = Color.DarkSlateGray;
    private Color _colorHover = Color.SlateGray;
    private Color _colorPressed = Color.DarkGray;
    private Color _textColor = Color.White;

    private bool _isHovered;
    private bool _isPressed;

    public event Action OnClick;

    // Constructeur Graphique (avec images et scale)
    public Button(Vector2 position, Texture2D idle, Texture2D hover, float scale = 1f)
    {
        _textureIdle = idle;
        _textureHover = hover;
        _hasTextures = true;
        _scale = scale;
        
        // La taille du bouton dépend de l'image et du scale
        _bounds = new Rectangle((int)position.X, (int)position.Y, (int)(idle.Width * scale), (int)(idle.Height * scale));
    }

    // Constructeur Texte (Fallback)
    public Button(Rectangle bounds, string text, SpriteFont font, GraphicsDevice graphicsDevice)
    {
        _bounds = bounds;
        _text = text;
        _font = font;
        _hasTextures = false;

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Update(Vector2 mousePosition, bool isMousePressed)
    {
        _isHovered = _bounds.Contains(mousePosition);

        if (_isHovered)
        {
            if (isMousePressed)
            {
                _isPressed = true;
            }
            else if (_isPressed) // Relâchement du clic sur le bouton
            {
                _isPressed = false;
                OnClick?.Invoke();
            }
        }
        else
        {
            _isPressed = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_hasTextures)
        {
            Texture2D currentTexture = _isHovered ? _textureHover : _textureIdle;
            
            // Position de dessin (le Rectangle _bounds est déjà à la bonne taille et position)
            Vector2 position = new Vector2(_bounds.X, _bounds.Y);
            
            if (_isPressed)
            {
                position.Y += 2;
            }
            
            spriteBatch.Draw(currentTexture, position, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        }
        else
        {
            // Mode Fallback (Rectangle + Texte)
            Color color = _isPressed ? _colorPressed : (_isHovered ? _colorHover : _colorNormal);
            
            spriteBatch.Draw(_pixel, new Rectangle(_bounds.X + 4, _bounds.Y + 4, _bounds.Width, _bounds.Height), Color.Black * 0.5f);
            spriteBatch.Draw(_pixel, _bounds, color);
            
            if (_isHovered)
            {
                int border = 2;
                spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, border), Color.Gold);
                spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Bottom - border, _bounds.Width, border), Color.Gold);
                spriteBatch.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, border, _bounds.Height), Color.Gold);
                spriteBatch.Draw(_pixel, new Rectangle(_bounds.Right - border, _bounds.Y, border, _bounds.Height), Color.Gold);
            }

            if (_font != null)
            {
                Vector2 textSize = _font.MeasureString(_text);
                Vector2 textPos = new Vector2(
                    _bounds.Center.X - textSize.X / 2,
                    _bounds.Center.Y - textSize.Y / 2
                );
                spriteBatch.DrawString(_font, _text, textPos, _textColor);
            }
        }
    }
}