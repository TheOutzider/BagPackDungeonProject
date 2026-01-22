using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrjectBackPackDungeon;

public class InventoryGrid
{
    private const int GridWidth = 7; 
    private const int GridHeight = 8; // Passage de 9 à 8 lignes
    private const int CellSize = 42; 
    private const int CellPadding = 2;

    private Vector2 _position; 
    private Rectangle _gridBounds;
    private Rectangle _trashZone;
    private List<Item> _items;
    private Texture2D _cellTexture;
    private Texture2D _bagTexture;
    private SpriteFont _font;
    
    private float _bagScale;
    private Vector2 _gridOffset; 

    private Item _draggedItem;
    private Item _hoveredItem;
    private Point _originalGridPos;

    public List<Item> Items => _items;

    public int TotalStr => _items.Sum(i => i.ActiveStr);
    public int TotalDex => _items.Sum(i => i.ActiveDex);
    public int TotalInt => _items.Sum(i => i.ActiveInt);
    public int TotalLuck => _items.Sum(i => i.ActiveLuck);

    public event Action<Item> OnItemUsed;

    public InventoryGrid(Vector2 position, GraphicsDevice graphicsDevice, SpriteFont font, Texture2D bagTexture)
    {
        _position = position;
        _font = font;
        _bagTexture = bagTexture;
        _items = new List<Item>();

        _cellTexture = new Texture2D(graphicsDevice, 1, 1);
        _cellTexture.SetData(new[] { Color.White });

        float targetWidth = 480f;
        _bagScale = targetWidth / _bagTexture.Width;
        
        int totalGridW = GridWidth * (CellSize + CellPadding);
        int totalGridH = GridHeight * (CellSize + CellPadding);

        // Décalage vers le bas : on passe de 750px à 800px sur l'image originale
        float gridStartY = 800f * _bagScale;
        float gridStartX = (targetWidth - totalGridW) / 2f;
        
        _gridOffset = new Vector2(gridStartX, gridStartY);
        _gridBounds = new Rectangle((int)(_position.X + _gridOffset.X), (int)(_position.Y + _gridOffset.Y), totalGridW, totalGridH);
        
        _trashZone = new Rectangle((int)_position.X + 50, (int)(_position.Y + _bagTexture.Height * _bagScale) - 120, (int)targetWidth - 100, 60);
    }

    public void AddItem(Item item)
    {
        for (int y = 0; y <= GridHeight - item.Height; y++)
        {
            for (int x = 0; x <= GridWidth - item.Width; x++)
            {
                if (IsSpaceAvailable(x, y, item.Width, item.Height, null))
                {
                    item.GridX = x;
                    item.GridY = y;
                    _items.Add(item);
                    RecalculateSynergies();
                    return;
                }
            }
        }
    }
    
    public void RemoveItem(Item item)
    {
        if (_items.Contains(item))
        {
            _items.Remove(item);
            RecalculateSynergies();
        }
    }

    private void RecalculateSynergies()
    {
        foreach (var item in _items) item.ResetStats();

        foreach (var source in _items)
        {
            if (!source.HasSynergy) continue;

            foreach (var target in _items)
            {
                if (source == target) continue;
                if (source.SynergyTarget != ItemType.Other && target.Type != source.SynergyTarget) continue;

                if (AreAdjacent(source, target))
                {
                    target.ActiveStr += source.SynergyBonusStr;
                    target.ActiveDex += source.SynergyBonusDex;
                    target.ActiveInt += source.SynergyBonusInt;
                }
            }
        }
    }

    private bool AreAdjacent(Item a, Item b)
    {
        Rectangle rA = new Rectangle(a.GridX, a.GridY, a.Width, a.Height);
        Rectangle rB = new Rectangle(b.GridX, b.GridY, b.Width, b.Height);
        rA.Inflate(1, 1);
        return rA.Intersects(rB);
    }

    public void Update(GameTime gameTime, Vector2 mousePosition, bool isLeftDown, bool isRightClicked)
    {
        _hoveredItem = null;

        if (isLeftDown)
        {
            if (_draggedItem == null)
            {
                foreach (var item in _items)
                {
                    Rectangle itemRect = GetItemRectangle(item);
                    if (itemRect.Contains(mousePosition))
                    {
                        _draggedItem = item;
                        item.IsDragging = true;
                        _originalGridPos = new Point(item.GridX, item.GridY);
                        item.DragOffset = mousePosition - new Vector2(itemRect.X, itemRect.Y);
                        break;
                    }
                }
            }
        }
        else
        {
            if (_draggedItem != null)
            {
                DropItem(mousePosition);
                if (_draggedItem != null)
                {
                    _draggedItem.IsDragging = false;
                    _draggedItem = null;
                }
                RecalculateSynergies();
            }
            else
            {
                foreach (var item in _items)
                {
                    if (GetItemRectangle(item).Contains(mousePosition))
                    {
                        _hoveredItem = item;
                        if (isRightClicked && item.Type == ItemType.Consumable)
                        {
                            OnItemUsed?.Invoke(item);
                        }
                        break;
                    }
                }
            }
        }
    }

    private void DropItem(Vector2 mousePosition)
    {
        if (_trashZone.Contains(mousePosition))
        {
            _items.Remove(_draggedItem);
            _draggedItem = null;
            return;
        }

        Vector2 localPos = mousePosition - _draggedItem.DragOffset - new Vector2(_gridBounds.X, _gridBounds.Y);
        int cellTotalSize = CellSize + CellPadding;
        int gridX = (int)System.Math.Round(localPos.X / cellTotalSize);
        int gridY = (int)System.Math.Round(localPos.Y / cellTotalSize);

        bool insideGrid = gridX >= 0 && gridY >= 0 && 
                          gridX + _draggedItem.Width <= GridWidth && 
                          gridY + _draggedItem.Height <= GridHeight;

        if (insideGrid && IsSpaceAvailable(gridX, gridY, _draggedItem.Width, _draggedItem.Height, _draggedItem))
        {
            _draggedItem.GridX = gridX;
            _draggedItem.GridY = gridY;
            return;
        }

        _draggedItem.GridX = _originalGridPos.X;
        _draggedItem.GridY = _originalGridPos.Y;
    }

    private bool IsSpaceAvailable(int targetX, int targetY, int width, int height, Item ignoreItem)
    {
        foreach (var item in _items)
        {
            if (item == ignoreItem) continue;
            bool overlapX = targetX < item.GridX + item.Width && targetX + width > item.GridX;
            bool overlapY = targetY < item.GridY + item.Height && targetY + height > item.GridY;
            if (overlapX && overlapY) return false;
        }
        return true;
    }

    private Rectangle GetItemRectangle(Item item)
    {
        int cellTotalSize = CellSize + CellPadding;
        Vector2 pos = new Vector2(_gridBounds.X, _gridBounds.Y) + new Vector2(item.GridX * cellTotalSize, item.GridY * cellTotalSize);
        int width = item.Width * CellSize + (item.Width - 1) * CellPadding;
        int height = item.Height * CellSize + (item.Height - 1) * CellPadding;
        return new Rectangle((int)pos.X, (int)pos.Y, width, height);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 mousePosition)
    {
        spriteBatch.Draw(_bagTexture, _position, null, Color.White, 0f, Vector2.Zero, _bagScale, SpriteEffects.None, 0f);

        int cellTotalSize = CellSize + CellPadding;
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector2 cellPos = new Vector2(_gridBounds.X, _gridBounds.Y) + new Vector2(x * cellTotalSize, y * cellTotalSize);
                Rectangle cellRect = new Rectangle((int)cellPos.X, (int)cellPos.Y, CellSize, CellSize);
                spriteBatch.Draw(_cellTexture, cellRect, Color.Black * 0.15f);
            }
        }

        if (_draggedItem != null)
        {
            bool isHoveringTrash = _trashZone.Contains(mousePosition);
            spriteBatch.Draw(_cellTexture, _trashZone, isHoveringTrash ? Color.Red * 0.6f : Color.DarkRed * 0.4f);
            if (_font != null)
            {
                string text = "DISCARD ITEM";
                Vector2 size = _font.MeasureString(text) * 0.7f;
                spriteBatch.DrawString(_font, text, new Vector2(_trashZone.Center.X - size.X/2, _trashZone.Center.Y - size.Y/2), Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
            }
        }

        foreach (var item in _items)
        {
            if (item.IsDragging) continue;
            DrawItem(spriteBatch, item, GetItemRectangle(item));
        }

        if (_draggedItem != null)
        {
            Vector2 drawPos = mousePosition - _draggedItem.DragOffset;
            Rectangle rect = new Rectangle((int)drawPos.X, (int)drawPos.Y, _draggedItem.Width * CellSize + (_draggedItem.Width-1)*CellPadding, _draggedItem.Height * CellSize + (_draggedItem.Height-1)*CellPadding);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X + 5, rect.Y + 5, rect.Width, rect.Height), Color.Black * 0.5f);
            DrawItem(spriteBatch, _draggedItem, rect);
            
            if (!_trashZone.Contains(mousePosition))
            {
                Vector2 localPos = mousePosition - _draggedItem.DragOffset - new Vector2(_gridBounds.X, _gridBounds.Y);
                int gridX = (int)System.Math.Round(localPos.X / cellTotalSize);
                int gridY = (int)System.Math.Round(localPos.Y / cellTotalSize);
                bool valid = gridX >= 0 && gridY >= 0 && gridX + _draggedItem.Width <= GridWidth && gridY + _draggedItem.Height <= GridHeight && IsSpaceAvailable(gridX, gridY, _draggedItem.Width, _draggedItem.Height, _draggedItem);
                if (!valid) spriteBatch.Draw(_cellTexture, rect, Color.Red * 0.3f);
            }
        }

        if (_hoveredItem != null && _draggedItem == null)
        {
            DrawTooltip(spriteBatch, _hoveredItem, mousePosition);
        }
    }

    private void DrawItem(SpriteBatch spriteBatch, Item item, Rectangle rect)
    {
        spriteBatch.Draw(_cellTexture, rect, item.Color);
        if (item.ActiveStr > item.BaseStr || item.ActiveDex > item.BaseDex || item.ActiveInt > item.BaseInt)
        {
            int b = 2;
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, rect.Width, b), Color.Gold);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Bottom - b, rect.Width, b), Color.Gold);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, b, rect.Height), Color.Gold);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.Right - b, rect.Y, b, rect.Height), Color.Gold);
        }

        if (_font != null)
        {
            string name = item.Name;
            Vector2 textSize = _font.MeasureString(name);
            float scale = 0.6f;
            if (textSize.X * scale > rect.Width - 6) scale = (rect.Width - 6) / textSize.X;
            Vector2 pos = new Vector2(rect.Center.X - (textSize.X * scale) / 2, rect.Center.Y - (textSize.Y * scale) / 2);
            spriteBatch.DrawString(_font, name, pos + new Vector2(1, 1), Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, name, pos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawTooltip(SpriteBatch spriteBatch, Item item, Vector2 mousePos)
    {
        if (_font == null) return;

        Color rarityColor = item.GetRarityColor();
        string title = item.Name.ToUpper();
        string type = item.Type.ToString();
        string desc = item.Description;
        
        string stats = "";
        if (item.ActiveStr > 0) stats += $"+{item.ActiveStr} STR\n";
        if (item.ActiveDex > 0) stats += $"+{item.ActiveDex} DEX\n";
        if (item.ActiveInt > 0) stats += $"+{item.ActiveInt} INT\n";
        if (item.DiceType != DiceType.None) stats += $"Adds: {item.DiceType}\n";

        string fullText = $"{title}\n{type}\n\n{stats}\n{desc}";
        Vector2 size = _font.MeasureString(fullText) * 0.9f;
        
        int offsetX = 45; 
        int offsetY = 10;
        if (mousePos.X + offsetX + size.X + 20 > 1920) offsetX = (int)(-size.X - 50);

        Rectangle rect = new Rectangle((int)mousePos.X + offsetX, (int)mousePos.Y + offsetY, (int)size.X + 20, (int)size.Y + 20);

        spriteBatch.Draw(_cellTexture, rect, new Color(20, 20, 25) * 0.98f);
        spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, rect.Width, 30), rarityColor * 0.6f);
        
        spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), rarityColor);
        spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), rarityColor);
        spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), rarityColor);
        spriteBatch.Draw(_cellTexture, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), rarityColor);

        spriteBatch.DrawString(_font, fullText, new Vector2(rect.X + 10, rect.Y + 5), Color.White, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
    }
}
