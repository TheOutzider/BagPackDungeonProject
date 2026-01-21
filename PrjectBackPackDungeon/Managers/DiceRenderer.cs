using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrjectBackPackDungeon;

/// <summary>
/// Obsolète : Le rendu des dés est maintenant géré en 2D directement dans DiceManager.
/// </summary>
public class DiceRenderer
{
    public DiceRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
    {
    }

    public void DrawShadow(SpriteBatch spriteBatch, Vector2 screenPos, float height)
    {
    }

    public void Draw(Dice dice, Vector2 screenPos)
    {
    }
}