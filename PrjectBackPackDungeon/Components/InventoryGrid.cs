using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrjectBackPackDungeon;

public class InventoryGrid
{
    private const int GridWidth = 6;
    private const int GridHeight = 8;
    private const int CellSize = 60;
    private const int CellPadding = 4;

    private Vector2 _position;
    private Rectangle _trashZone;
    private List<Item> _items;
    private Texture2D _cellTexture;
    private SpriteFont _font;

    private Item _draggedItem;
    private Item _hoveredItem;
    private Point _originalGridPos;
    private Point _originalSize;

    public List<Item> Items => _items;

    public int TotalStr => _items.Sum(i => i.ActiveStr);
    public int TotalDex => _items.Sum(i => i.ActiveDex);
    public int TotalInt => _items.Sum(i => i.ActiveInt);
    public int TotalLuck => _items.Sum(i => i.ActiveLuck);

    // Événement quand un item est utilisé (Clic Droit sur Consommable)
    public event Action<Item> OnItemUsed;

    public InventoryGrid(Vector2 position, GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _position = position;
        _font = font;
        _items = new List<Item>();

        _cellTexture = new Texture2D(graphicsDevice, 1, 1);
        _cellTexture.SetData(new[] { Color.White });

        int gridPixelHeight = GridHeight * (CellSize + CellPadding);
        _trashZone = new Rectangle((int)position.X, (int)position.Y + gridPixelHeight + 20, GridWidth * (CellSize + CellPadding), 60);
    }

    public void AddItem(Item item)
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
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
        System.Diagnostics.Debug.WriteLine($"No space for {item.Name}");
    }
    
    public void RemoveItem(Item item)
    {
        if (_items.Contains(item))
        {
            _items.Remove(item);
            RecalculateSynergies();
        }
    }
    
    public void Clear()
    {
        _items.Clear();
        RecalculateSynergies();
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
        bool touchX = (rA.Right == rB.Left || rA.Left == rB.Right) && (rA.Top < rB.Bottom && rA.Bottom > rB.Top);
        bool touchY = (rA.Bottom == rB.Top || rA.Top == rB.Bottom) && (rA.Left < rB.Right && rA.Right > rB.Left);
        return touchX || touchY;
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
                        _originalSize = new Point(item.Width, item.Height);
                        item.DragOffset = mousePosition - new Vector2(itemRect.X, itemRect.Y);
                        break;
                    }
                }
            }
            else
            {
                if (isRightClicked)
                {
                    // Si on tient l'objet, clic droit = Rotation
                    RotateDraggedItem();
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
                // Pas de drag en cours
                foreach (var item in _items)
                {
                    if (GetItemRectangle(item).Contains(mousePosition))
                    {
                        _hoveredItem = item;
                        
                        // Clic Droit sur un item posé = Utilisation (si Consommable)
                        if (isRightClicked)
                        {
                            if (item.Type == ItemType.Consumable)
                            {
                                OnItemUsed?.Invoke(item);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    private void RotateDraggedItem()
    {
        int temp = _draggedItem.Width;
        _draggedItem.Width = _draggedItem.Height;
        _draggedItem.Height = temp;
    }

    private void DropItem(Vector2 mousePosition)
    {
        if (_trashZone.Contains(mousePosition))
        {
            _items.Remove(_draggedItem);
            _draggedItem = null;
            return;
        }

        Vector2 localPos = mousePosition - _draggedItem.DragOffset - _position;
        int cellTotalSize = CellSize + CellPadding;
        
        int gridX = (int)System.Math.Round(localPos.X / cellTotalSize);
        int gridY = (int)System.Math.Round(localPos.Y / cellTotalSize);

        bool insideGrid = gridX >= 0 && gridY >= 0 && 
                          gridX + _draggedItem.Width <= GridWidth && 
                          gridY + _draggedItem.Height <= GridHeight;

        if (insideGrid)
        {
            if (IsSpaceAvailable(gridX, gridY, _draggedItem.Width, _draggedItem.Height, _draggedItem))
            {
                _draggedItem.GridX = gridX;
                _draggedItem.GridY = gridY;
                return;
            }
        }

        _draggedItem.GridX = _originalGridPos.X;
        _draggedItem.GridY = _originalGridPos.Y;
        _draggedItem.Width = _originalSize.X;
        _draggedItem.Height = _originalSize.Y;
    }

    private bool IsSpaceAvailable(int targetX, int targetY, int width, int height, Item ignoreItem)
    {
        foreach (var item in _items)
        {
            if (item == ignoreItem) continue;

            bool overlapX = targetX < item.GridX + item.Width && targetX + width > item.GridX;
            bool overlapY = targetY < item.GridY + item.Height && targetY + height > item.GridY;

            if (overlapX && overlapY)
            {
                return false;
            }
        }
        return true;
    }

    private Rectangle GetItemRectangle(Item item)
    {
        int cellTotalSize = CellSize + CellPadding;
        Vector2 pos = _position + new Vector2(item.GridX * cellTotalSize, item.GridY * cellTotalSize);
        int width = item.Width * CellSize + (item.Width - 1) * CellPadding;
        int height = item.Height * CellSize + (item.Height - 1) * CellPadding;
        return new Rectangle((int)pos.X, (int)pos.Y, width, height);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 mousePosition)
    {
        int cellTotalSize = CellSize + CellPadding;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector2 cellPos = _position + new Vector2(x * cellTotalSize, y * cellTotalSize);
                spriteBatch.Draw(_cellTexture, new Rectangle((int)cellPos.X, (int)cellPos.Y, CellSize, CellSize), Color.Gray * 0.3f);
            }
        }

        if (_draggedItem != null)
        {
            bool isHoveringTrash = _trashZone.Contains(mousePosition);
            Color trashColor = isHoveringTrash ? Color.Red : Color.DarkRed;
            spriteBatch.Draw(_cellTexture, _trashZone, trashColor);
            if (_font != null)
            {
                string text = "TRASH";
                Vector2 size = _font.MeasureString(text);
                spriteBatch.DrawString(_font, text, new Vector2(_trashZone.Center.X - size.X/2, _trashZone.Center.Y - size.Y/2), Color.White);
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
            int width = _draggedItem.Width * CellSize + (_draggedItem.Width - 1) * CellPadding;
            int height = _draggedItem.Height * CellSize + (_draggedItem.Height - 1) * CellPadding;
            Rectangle rect = new Rectangle((int)drawPos.X, (int)drawPos.Y, width, height);
            
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X + 5, rect.Y + 5, rect.Width, rect.Height), Color.Black * 0.5f);
            DrawItem(spriteBatch, _draggedItem, rect);
            
            if (!_trashZone.Contains(mousePosition))
            {
                Vector2 localPos = mousePosition - _draggedItem.DragOffset - _position;
                int gridX = (int)System.Math.Round(localPos.X / cellTotalSize);
                int gridY = (int)System.Math.Round(localPos.Y / cellTotalSize);
                
                bool insideGrid = gridX >= 0 && gridY >= 0 && 
                                  gridX + _draggedItem.Width <= GridWidth && 
                                  gridY + _draggedItem.Height <= GridHeight;
                
                bool valid = insideGrid && IsSpaceAvailable(gridX, gridY, _draggedItem.Width, _draggedItem.Height, _draggedItem);

                if (!valid)
                {
                    spriteBatch.Draw(_cellTexture, rect, Color.Red * 0.5f);
                }
            }
        }
        
        DrawInventoryStats(spriteBatch);

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
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.Gold);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), Color.Gold);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.Gold);
            spriteBatch.Draw(_cellTexture, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), Color.Gold);
        }

        if (_font != null)
        {
            Vector2 textSize = _font.MeasureString(item.Name);
            float scale = 1f;
            if (textSize.X > rect.Width - 10) scale = (rect.Width - 10) / textSize.X;
            
            Vector2 pos = new Vector2(rect.Center.X - (textSize.X * scale) / 2, rect.Center.Y - (textSize.Y * scale) / 2);
            spriteBatch.DrawString(_font, item.Name, pos, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawInventoryStats(SpriteBatch spriteBatch)
    {
        if (_font == null) return;

        int startY = (int)_position.Y + (GridHeight * (CellSize + CellPadding)) + 90; 
        
        int d4 = _items.Count(i => i.DiceType == DiceType.D4_Basic);
        int d6Fire = _items.Count(i => i.DiceType == DiceType.D6_Fire);
        int d6Ice = _items.Count(i => i.DiceType == DiceType.D6_Ice);
        int d8 = _items.Count(i => i.DiceType == DiceType.D8_Basic);
        int d10 = _items.Count(i => i.DiceType == DiceType.D10_Basic);
        int d12 = _items.Count(i => i.DiceType == DiceType.D12_Basic);
        int d20 = _items.Count(i => i.DiceType == DiceType.D20_Steel);
        
        string stats = $"ITEMS: {_items.Count}\n\nDICE POOL:\n";
        if (d4 > 0) stats += $"D4: {d4}\n";
        if (d6Fire > 0) stats += $"D6 (Fire): {d6Fire}\n";
        if (d6Ice > 0) stats += $"D6 (Ice): {d6Ice}\n";
        if (d8 > 0) stats += $"D8: {d8}\n";
        if (d10 > 0) stats += $"D10: {d10}\n";
        if (d12 > 0) stats += $"D12: {d12}\n";
        if (d20 > 0) stats += $"D20 (Steel): {d20}\n";
        
        spriteBatch.DrawString(_font, stats, new Vector2(_position.X, startY), Color.White);
    }

    private void DrawTooltip(SpriteBatch spriteBatch, Item item, Vector2 mousePos)
    {
        if (_font == null) return;

        string text = $"{item.Name} ({item.Type})\n{item.Description}\n";
        
        if (item.Type == ItemType.Consumable) text += "(Right Click to Use)\n";

        if (item.ActiveStr > 0) text += $"+{item.ActiveStr} STR" + (item.ActiveStr > item.BaseStr ? " (Boosted!)\n" : "\n");
        if (item.ActiveDex > 0) text += $"+{item.ActiveDex} DEX" + (item.ActiveDex > item.BaseDex ? " (Boosted!)\n" : "\n");
        if (item.ActiveInt > 0) text += $"+{item.ActiveInt} INT" + (item.ActiveInt > item.BaseInt ? " (Boosted!)\n" : "\n");
        
        if (item.DiceType != DiceType.None) text += $"Adds: {item.DiceType}\n";
        
        if (item.HasSynergy)
        {
            text += $"\nSYNERGY:\nAdjacent {item.SynergyTarget} get:\n";
            if (item.SynergyBonusStr > 0) text += $"+{item.SynergyBonusStr} STR ";
            if (item.SynergyBonusDex > 0) text += $"+{item.SynergyBonusDex} DEX ";
            if (item.SynergyBonusInt > 0) text += $"+{item.SynergyBonusInt} INT ";
        }

        Vector2 size = _font.MeasureString(text);
        Rectangle tooltipRect = new Rectangle((int)mousePos.X + 15, (int)mousePos.Y + 15, (int)size.X + 20, (int)size.Y + 20);
        
        spriteBatch.Draw(_cellTexture, tooltipRect, Color.Black * 0.9f);
        spriteBatch.Draw(_cellTexture, new Rectangle(tooltipRect.X, tooltipRect.Y, tooltipRect.Width, 2), Color.White);
        spriteBatch.Draw(_cellTexture, new Rectangle(tooltipRect.X, tooltipRect.Bottom, tooltipRect.Width, 2), Color.White);
        spriteBatch.Draw(_cellTexture, new Rectangle(tooltipRect.X, tooltipRect.Y, 2, tooltipRect.Height), Color.White);
        spriteBatch.Draw(_cellTexture, new Rectangle(tooltipRect.Right, tooltipRect.Y, 2, tooltipRect.Height + 2), Color.White);

        spriteBatch.DrawString(_font, text, new Vector2(tooltipRect.X + 10, tooltipRect.Y + 10), Color.White);
    }
}