using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

public class RoomCard
{
    public Room Room { get; private set; }
    private Rectangle _baseBounds;
    
    private float _scale = 1.0f;
    private float _targetScale = 1.0f;
    private float _hoverLerp = 0f;
    
    private Texture2D _texture;
    private Texture2D _pixel;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    
    private bool _isHovered;
    public event Action OnClick;

    public RoomCard(Room room, Rectangle bounds, GraphicsDevice gd, SpriteFont font, SpriteFont titleFont, Texture2D texture)
    {
        Room = room;
        _baseBounds = bounds;
        _font = font;
        _titleFont = titleFont;
        _texture = texture;
        
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        // Si la texture est nulle, on utilise le pixel blanc comme fallback
        if (_texture == null) _texture = _pixel;
    }

    public void Update(GameTime gameTime, Vector2 mousePos, bool isClick)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _isHovered = _baseBounds.Contains(mousePos);

        if (_isHovered)
        {
            _targetScale = 1.1f;
            _hoverLerp = MathHelper.Lerp(_hoverLerp, 1f, 10f * dt);
            if (isClick) OnClick?.Invoke();
        }
        else
        {
            _targetScale = 1.0f;
            _hoverLerp = MathHelper.Lerp(_hoverLerp, 0f, 10f * dt);
        }

        _scale = MathHelper.Lerp(_scale, _targetScale, 15f * dt);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 center = new Vector2(_baseBounds.Center.X, _baseBounds.Center.Y);
        Vector2 size = new Vector2(_baseBounds.Width, _baseBounds.Height);
        
        // Sécurité pour l'origine
        Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

        // 1. Ombre portée
        float shadowDist = 10f + (_hoverLerp * 15f);
        spriteBatch.Draw(_texture, center + new Vector2(shadowDist, shadowDist), null, Color.Black * 0.4f, 0f, origin, _scale * (size.X / _texture.Width), SpriteEffects.None, 0f);

        // 2. La Carte (Texture)
        spriteBatch.Draw(_texture, center, null, Color.White, 0f, origin, _scale * (size.X / _texture.Width), SpriteEffects.None, 0f);
        
        // 3. Bordure dorée au survol
        if (_hoverLerp > 0)
        {
            DrawSimpleBorder(spriteBatch, center, size, Color.Gold * _hoverLerp, 4);
        }

        // 4. Textes
        string typeText = Room.Type.ToString().ToUpper();
        Vector2 typeSize = _titleFont.MeasureString(typeText);
        spriteBatch.DrawString(_titleFont, typeText, center + new Vector2(0, -size.Y/2 + 40), Color.White, 0f, typeSize / 2, _scale * 0.5f, SpriteEffects.None, 0f);

        if (!string.IsNullOrEmpty(Room.EnemyName))
        {
            string name = Room.EnemyName;
            Vector2 nameSize = _font.MeasureString(name);
            spriteBatch.DrawString(_font, name, center + new Vector2(0, -40), Color.White, 0f, nameSize / 2, _scale * 1.1f, SpriteEffects.None, 0f);
        }
    }

    private void DrawSimpleBorder(SpriteBatch sb, Vector2 center, Vector2 size, Color color, int b)
    {
        float w = size.X * _scale;
        float h = size.Y * _scale;
        sb.Draw(_pixel, new Rectangle((int)(center.X - w/2), (int)(center.Y - h/2), (int)w, b), color);
        sb.Draw(_pixel, new Rectangle((int)(center.X - w/2), (int)(center.Y + h/2 - b), (int)w, b), color);
        sb.Draw(_pixel, new Rectangle((int)(center.X - w/2), (int)(center.Y - h/2), b, (int)h), color);
        sb.Draw(_pixel, new Rectangle((int)(center.X + w/2 - b), (int)(center.Y - h/2), b, (int)h), color);
    }
}
