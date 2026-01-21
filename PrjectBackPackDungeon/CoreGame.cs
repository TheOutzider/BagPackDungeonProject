using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrjectBackPackDungeon;

public enum GameState
{
    TitleScreen,
    SlotSelection,
    ClassSelection,
    Playing,
    Loot, 
    GameOver,
    Options,
    RoomSelection, 
    Shop, 
    Event 
}

public enum PlayerClass
{
    Warrior,
    Mage,
    Rogue
}

public class CoreGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int VirtualWidth = 1920;
    private const int VirtualHeight = 1080;

    private RenderTarget2D _renderTarget;
    private Rectangle _destinationRectangle;
    private Texture2D _pixel;
    private Texture2D _circleTexture;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private SpriteFont _cloisterFont; 
    
    // Textures UI
    private Texture2D _texLogo;
    private Texture2D _texBtnNewGameIdle;
    private Texture2D _texBtnNewGameHover;
    private Texture2D _texBtnOptionIdle;
    private Texture2D _texBtnOptionHover;
    private Texture2D _texBtnQuitIdle;
    private Texture2D _texBtnQuitHover;
    
    // Textures Effets & Curseur
    private Texture2D _texCursorNormal;
    private Texture2D _texCursorClicked;
    private Texture2D _texFingerLeft;  
    private Texture2D _texFingerRight; 
    private Texture2D _texArmLeft;
    private Texture2D _texArmRight;

    private GameState _currentState = GameState.TitleScreen;
    private PlayerClass _selectedClass = PlayerClass.Warrior;
    private int _currentSlotIndex = 0;

    private List<Button> _titleButtons;
    private List<Button> _slotButtons;
    private List<Button> _deleteButtons;
    private List<Button> _classButtons;
    private List<Button> _optionButtons;
    private Button _backButton;
    
    private List<Button> _roomButtons;
    private List<Room> _nextRoomOptions;
    
    private List<Button> _shopButtons;
    private List<Button> _eventButtons;
    private string _eventText;
    
    private struct Particle { public Vector2 Pos; public float Speed; public float Size; public Color Color; }
    private List<Particle> _bgParticles;
    private Random _random = new Random();

    private InventoryGrid _inventoryGrid;
    private DiceManager _diceManager;
    private InfoPanel _infoPanel; 
    private HudOverlay _hudOverlay;
    private GameLog _gameLog;
    private FloorManager _floorManager;
    private Enemy _currentEnemy;
    private EffectManager _effectManager;
    
    private List<Skill> _playerSkills;
    private List<Button> _skillButtons;
    private List<Relic> _playerRelics;
    
    private List<Item> _lootOptions;
    private Rectangle[] _lootRects;
    
    private int _baseStr = 5;
    private int _baseDex = 5;
    private int _baseInt = 5;
    private int _baseLuck = 5;
    
    private int _currentStr;
    private int _currentDex;
    private int _currentInt;
    private int _currentLuck;

    private int _playerHp = 100;
    private int _playerMaxHp = 100;
    private int _playerMana = 50;
    private int _playerMaxMana = 50;
    private int _playerGold = 0;
    
    private bool _waitingForNextRoom = false;
    
    private MouseState _prevMouseState;
    private KeyboardState _prevKeyboardState;
    
    private Rectangle diceBounds;

    public CoreGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false; 

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;
    }

    protected override void Initialize()
    {
        base.Initialize();
        UpdateDestinationRectangle();
        
        SettingsManager.LoadSettings();
        ApplySettings();
        
        _bgParticles = new List<Particle>();
        for (int i = 0; i < 50; i++)
        {
            _bgParticles.Add(new Particle 
            { 
                Pos = new Vector2(_random.Next(VirtualWidth), _random.Next(VirtualHeight)),
                Speed = (float)_random.NextDouble() * 2 + 0.5f,
                Size = _random.Next(2, 6),
                Color = Color.White * ((float)_random.NextDouble() * 0.2f)
            });
        }
    }
    
    private void ApplySettings()
    {
        _graphics.IsFullScreen = SettingsManager.Settings.IsFullScreen;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        int size = 64;
        _circleTexture = new Texture2D(GraphicsDevice, size, size);
        Color[] data = new Color[size * size];
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (Vector2.Distance(new Vector2(x, y), center) <= radius)
                    data[y * size + x] = Color.White;
                else
                    data[y * size + x] = Color.Transparent;
            }
        }
        _circleTexture.SetData(data);

        try
        {
            _font = Content.Load<SpriteFont>("MainFont");
            _titleFont = Content.Load<SpriteFont>("TitleFont");
            _cloisterFont = Content.Load<SpriteFont>("Fonts/CloisterBlack");
            
            _texLogo = Content.Load<Texture2D>("PNG/title_logo");
            _texBtnNewGameIdle = Content.Load<Texture2D>("PNG/nouvellepartie_idle");
            _texBtnNewGameHover = Content.Load<Texture2D>("PNG/nouvellepartie_hover");
            _texBtnOptionIdle = Content.Load<Texture2D>("PNG/option_idle");
            _texBtnOptionHover = Content.Load<Texture2D>("PNG/option_hover");
            _texBtnQuitIdle = Content.Load<Texture2D>("PNG/quitter_idle");
            _texBtnQuitHover = Content.Load<Texture2D>("PNG/quitter_hover");
            
            _texCursorNormal = Content.Load<Texture2D>("PNG/cursor_normal");
            _texCursorClicked = Content.Load<Texture2D>("PNG/cursor_clicked");
            _texFingerLeft = Content.Load<Texture2D>("PNG/finger_pointing_left");
            _texFingerRight = Content.Load<Texture2D>("PNG/finger_pointing_right");
            _texArmLeft = Content.Load<Texture2D>("PNG/arm_strong_left");
            _texArmRight = Content.Load<Texture2D>("PNG/arm_strong_right");
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("Content load failed!");
        }

        _effectManager = new EffectManager(GraphicsDevice, _cloisterFont);
        _effectManager.LoadContent(_texFingerLeft, _texFingerRight, _texArmLeft, _texArmRight);

        _titleButtons = new List<Button>();
        
        float btnScale = 0.2f;
        int btnSpacing = 20;
        
        int btnWidth = (int)(_texBtnNewGameIdle.Width * btnScale);
        int btnHeight = (int)(_texBtnNewGameIdle.Height * btnScale);
        int startX = (VirtualWidth - btnWidth) / 2;
        int startY = 600; 

        var btnPlay = new Button(new Vector2(startX, startY), _texBtnNewGameIdle, _texBtnNewGameHover, btnScale);
        btnPlay.OnClick += () => {
            RefreshSlotButtons();
            _currentState = GameState.SlotSelection;
        };
        _titleButtons.Add(btnPlay);
        
        var btnOption = new Button(new Vector2(startX, startY + btnHeight + btnSpacing), _texBtnOptionIdle, _texBtnOptionHover, btnScale);
        btnOption.OnClick += () => {
            RefreshOptionButtons();
            _currentState = GameState.Options;
        };
        _titleButtons.Add(btnOption);
        
        var btnQuit = new Button(new Vector2(startX, startY + (btnHeight + btnSpacing) * 2), _texBtnQuitIdle, _texBtnQuitHover, btnScale);
        btnQuit.OnClick += Exit;
        _titleButtons.Add(btnQuit);

        _classButtons = new List<Button>();
        var btnWar = new Button(new Rectangle(VirtualWidth/2 - 400, 400, 250, 300), "WARRIOR\n\nHigh HP\nMelee", _font, GraphicsDevice);
        btnWar.OnClick += () => StartGame(PlayerClass.Warrior);
        _classButtons.Add(btnWar);
        
        var btnMage = new Button(new Rectangle(VirtualWidth/2 - 125, 400, 250, 300), "MAGE\n\nHigh Mana\nMagic", _font, GraphicsDevice);
        btnMage.OnClick += () => StartGame(PlayerClass.Mage);
        _classButtons.Add(btnMage);
        
        var btnRogue = new Button(new Rectangle(VirtualWidth/2 + 150, 400, 250, 300), "ROGUE\n\nBalanced\nCrit", _font, GraphicsDevice);
        btnRogue.OnClick += () => StartGame(PlayerClass.Rogue);
        _classButtons.Add(btnRogue);
        
        _backButton = new Button(new Rectangle(50, 50, 100, 50), "BACK", _font, GraphicsDevice);
        _backButton.OnClick += () => _currentState = GameState.TitleScreen;
    }
    
    private void RefreshOptionButtons()
    {
        _optionButtons = new List<Button>();
        int centerX = VirtualWidth / 2;
        int startY = 300;
        
        string fsText = SettingsManager.Settings.IsFullScreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF";
        var btnFs = new Button(new Rectangle(centerX - 150, startY, 300, 60), fsText, _font, GraphicsDevice);
        btnFs.OnClick += () => {
            SettingsManager.Settings.IsFullScreen = !SettingsManager.Settings.IsFullScreen;
            ApplySettings();
            SettingsManager.SaveSettings();
            RefreshOptionButtons();
        };
        _optionButtons.Add(btnFs);
        
        string musicText = $"MUSIC: {(int)(SettingsManager.Settings.MusicVolume * 100)}%";
        var btnMusic = new Button(new Rectangle(centerX - 150, startY + 80, 300, 60), musicText, _font, GraphicsDevice);
        btnMusic.OnClick += () => {
            SettingsManager.Settings.MusicVolume += 0.1f;
            if (SettingsManager.Settings.MusicVolume > 1.0f) SettingsManager.Settings.MusicVolume = 0.0f;
            SettingsManager.SaveSettings();
            RefreshOptionButtons();
        };
        _optionButtons.Add(btnMusic);
        
        string sfxText = $"SFX: {(int)(SettingsManager.Settings.SfxVolume * 100)}%";
        var btnSfx = new Button(new Rectangle(centerX - 150, startY + 160, 300, 60), sfxText, _font, GraphicsDevice);
        btnSfx.OnClick += () => {
            SettingsManager.Settings.SfxVolume += 0.1f;
            if (SettingsManager.Settings.SfxVolume > 1.0f) SettingsManager.Settings.SfxVolume = 0.0f;
            SettingsManager.SaveSettings();
            RefreshOptionButtons();
        };
        _optionButtons.Add(btnSfx);
    }

    private void RefreshSlotButtons()
    {
        _slotButtons = new List<Button>();
        _deleteButtons = new List<Button>();
        
        int slotWidth = 600;
        int slotHeight = 150;
        int startY = 300;
        int centerX = VirtualWidth / 2;

        for (int i = 0; i < 3; i++)
        {
            int slotIndex = i;
            SaveData data = SaveManager.LoadGame(slotIndex);
            
            string text = $"SLOT {i + 1}\nEMPTY";
            if (data != null)
            {
                text = $"SLOT {i + 1}\n{data.Class} - Floor {data.DungeonLevel}-{data.FloorNumber}\n{data.SaveDate.ToShortDateString()} {data.SaveDate.ToShortTimeString()}";
                
                var btnDel = new Button(new Rectangle(centerX + slotWidth/2 + 20, startY + (i * 200), 50, 50), "X", _font, GraphicsDevice);
                btnDel.OnClick += () => {
                    SaveManager.DeleteSave(slotIndex);
                    RefreshSlotButtons();
                };
                _deleteButtons.Add(btnDel);
            }

            var btnSlot = new Button(new Rectangle(centerX - slotWidth/2, startY + (i * 200), slotWidth, slotHeight), text, _font, GraphicsDevice);
            btnSlot.OnClick += () => {
                _currentSlotIndex = slotIndex;
                if (data == null)
                {
                    _currentState = GameState.ClassSelection;
                }
                else
                {
                    LoadGame(slotIndex);
                }
            };
            _slotButtons.Add(btnSlot);
        }
    }

    private void InitGameplayComponents()
    {
        int sideWidth = (int)(VirtualWidth * 0.25f);
        int centerWidth = VirtualWidth - (sideWidth * 2);
        int gameHeight = (int)(VirtualHeight * 0.8f);
        int logHeight = VirtualHeight - gameHeight;

        _infoPanel = new InfoPanel(new Rectangle(0, 0, sideWidth, VirtualHeight), GraphicsDevice, _font);
        
        Rectangle arenaBounds = new Rectangle(sideWidth, 0, centerWidth, gameHeight);
        diceBounds = arenaBounds;
        diceBounds.Inflate(-20, -20);
        diceBounds.Height -= 80; 
        
        _diceManager = new DiceManager(GraphicsDevice, diceBounds, _cloisterFont, Content); 
        _diceManager.SetEffectManager(_effectManager);
        _diceManager.OnTurnFinished += OnTurnFinished;

        _hudOverlay = new HudOverlay(arenaBounds, GraphicsDevice, _font);
        _gameLog = new GameLog(new Rectangle(sideWidth, gameHeight, centerWidth, logHeight), GraphicsDevice, _font);

        int startX = sideWidth + centerWidth + 50; 
        int startY = 100; 
        _inventoryGrid = new InventoryGrid(new Vector2(startX, startY), GraphicsDevice, _font);
        _inventoryGrid.OnItemUsed += OnItemUsed; 
    }

    private void InitSkills()
    {
        _playerSkills = new List<Skill>();
        _skillButtons = new List<Button>();

        switch (_selectedClass)
        {
            case PlayerClass.Warrior:
                _playerSkills.Add(new Skill("Bash", 10, SkillType.DirectDamage, 15, "Deal 15 DMG"));
                _playerSkills.Add(new Skill("Heal", 15, SkillType.Heal, 20, "Heal 20 HP"));
                break;
            case PlayerClass.Mage:
                _playerSkills.Add(new Skill("Fireball", 20, SkillType.DirectDamage, 30, "Deal 30 DMG"));
                _playerSkills.Add(new Skill("Heal", 10, SkillType.Heal, 25, "Heal 25 HP"));
                _playerSkills.Add(new Skill("Reroll", 5, SkillType.Reroll, 0, "Reroll Dice"));
                break;
            case PlayerClass.Rogue:
                _playerSkills.Add(new Skill("Stab", 10, SkillType.DirectDamage, 20, "Deal 20 DMG"));
                _playerSkills.Add(new Skill("Reroll", 5, SkillType.Reroll, 0, "Reroll Dice"));
                break;
        }

        int startX = (int)(VirtualWidth * 0.25f) + 20;
        int startY = (int)(VirtualHeight * 0.8f) - 70;
        int btnWidth = 150;
        int btnHeight = 50;
        int spacing = 10;

        for (int i = 0; i < _playerSkills.Count; i++)
        {
            var skill = _playerSkills[i];
            var btn = new Button(new Rectangle(startX + (i * (btnWidth + spacing)), startY, btnWidth, btnHeight), 
                $"{skill.Name}\n{skill.ManaCost} MP", _font, GraphicsDevice);
            
            btn.OnClick += () => UseSkill(skill);
            _skillButtons.Add(btn);
        }
    }

    private void UseSkill(Skill skill)
    {
        if (_currentState != GameState.Playing) return;
        if (_waitingForNextRoom) return;

        if (_playerMana >= skill.ManaCost)
        {
            bool success = false;
            switch (skill.Type)
            {
                case SkillType.Heal:
                    if (_playerHp < _playerMaxHp)
                    {
                        _playerHp = Math.Min(_playerHp + skill.Value, _playerMaxHp);
                        _gameLog.AddMessage($"Used {skill.Name}. Healed {skill.Value} HP.", Color.Cyan);
                        _effectManager.AddText(new Vector2(VirtualWidth/2, VirtualHeight/2), $"+{skill.Value} HP", Color.Green);
                        _effectManager.AddHealEffect(new Vector2(VirtualWidth/2, VirtualHeight/2));
                        success = true;
                    }
                    else
                    {
                        _gameLog.AddMessage("HP already full!", Color.Gray);
                    }
                    break;
                    
                case SkillType.DirectDamage:
                    if (_currentEnemy != null && !_currentEnemy.IsDead)
                    {
                        _currentEnemy.TakeDamage(skill.Value);
                        _gameLog.AddMessage($"Used {skill.Name}. Dealt {skill.Value} DMG.", Color.Cyan);
                        
                        Vector2 enemyCenter = new Vector2(_currentEnemy.Bounds.Center.X, _currentEnemy.Bounds.Center.Y);
                        _effectManager.AddDamageText(enemyCenter, skill.Value, false);
                        
                        if (_selectedClass == PlayerClass.Warrior) _effectManager.AddWarriorSlash(enemyCenter);
                        else if (_selectedClass == PlayerClass.Rogue) _effectManager.AddRogueDoubleSlash(enemyCenter);
                        else _effectManager.AddMageArcaneBlast(enemyCenter);

                        if (_currentEnemy.IsDead)
                        {
                            _gameLog.AddMessage($"{_currentEnemy.Name} is defeated!", Color.Green);
                            _effectManager.AddExplosion(enemyCenter, Color.Gold, 30); 
                            GenerateLoot();
                        }
                        success = true;
                    }
                    else
                    {
                        _gameLog.AddMessage("No enemy to target!", Color.Gray);
                    }
                    break;
                    
                case SkillType.Reroll:
                    if (_diceManager.IsRolling)
                    {
                        _gameLog.AddMessage("Dice are already rolling!", Color.Gray);
                    }
                    else
                    {
                        _diceManager.ThrowDices(_inventoryGrid.Items, _selectedClass);
                        _gameLog.AddMessage($"Used {skill.Name}. Rerolling dice...", Color.Cyan);
                        success = true;
                    }
                    break;
            }

            if (success)
            {
                _playerMana -= skill.ManaCost;
                _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);
                SaveGame();
            }
        }
        else
        {
            _gameLog.AddMessage("Not enough Mana!", Color.Red);
        }
    }

    private void StartGame(PlayerClass pClass)
    {
        _selectedClass = pClass;
        
        InitGameplayComponents();
        InitSkills();
        
        _playerRelics = new List<Relic>();
        _playerGold = 50; 
        
        switch (pClass)
        {
            case PlayerClass.Warrior:
                _playerMaxHp = 120; _playerMaxMana = 40;
                _baseStr = 10; _baseDex = 5; _baseInt = 2; _baseLuck = 3;
                _inventoryGrid.AddItem(new Item("Sword", 1, 3, Color.Orange, DiceType.D6_Fire, ItemType.Weapon) { BaseStr = 2 });
                _inventoryGrid.AddItem(new Item("Shield", 2, 2, Color.LightBlue, DiceType.D6_Ice, ItemType.Armor) { BaseStr = 1 });
                break;
                
            case PlayerClass.Mage:
                _playerMaxHp = 80; _playerMaxMana = 100;
                _baseStr = 2; _baseDex = 4; _baseInt = 10; _baseLuck = 5;
                _inventoryGrid.AddItem(new Item("Staff", 1, 3, Color.Purple, DiceType.D6_Fire, ItemType.Weapon) { BaseInt = 3 });
                _inventoryGrid.AddItem(new Item("Potion", 1, 1, Color.Red, DiceType.None, ItemType.Consumable));
                _inventoryGrid.AddItem(new Item("Tome", 2, 2, Color.DarkBlue, DiceType.D6_Ice, ItemType.Accessory) { BaseInt = 2 });
                break;
                
            case PlayerClass.Rogue:
                _playerMaxHp = 100; _playerMaxMana = 60;
                _baseStr = 4; _baseDex = 10; _baseInt = 4; _baseLuck = 8;
                _inventoryGrid.AddItem(new Item("Dagger", 1, 2, Color.Gray, DiceType.D6_Fire, ItemType.Weapon) { BaseDex = 2 });
                _inventoryGrid.AddItem(new Item("Dagger", 1, 2, Color.Gray, DiceType.D6_Fire, ItemType.Weapon) { BaseDex = 2 });
                _inventoryGrid.AddItem(new Item("Cloak", 2, 2, Color.DarkGray, DiceType.D6_Ice, ItemType.Armor) { BaseDex = 1, BaseLuck = 2 });
                break;
        }
        _playerHp = _playerMaxHp;
        _playerMana = _playerMaxMana;
        
        _floorManager = new FloorManager(1);
        _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);

        _currentState = GameState.RoomSelection;
        GenerateRoomSelection();
        
        SaveGame();
    }

    private void SaveGame()
    {
        var data = new SaveData
        {
            DungeonLevel = _floorManager.DungeonLevel,
            FloorNumber = _floorManager.RoomNumber,
            WorldSeed = _floorManager.Seed,
            Class = _selectedClass,
            Hp = _playerHp,
            MaxHp = _playerMaxHp,
            Mana = _playerMana,
            MaxMana = _playerMaxMana,
            Gold = _playerGold,
            InventoryItems = _inventoryGrid.Items.Select(i => new ItemData
            {
                Name = i.Name,
                GridX = i.GridX,
                GridY = i.GridY,
                Width = i.Width,
                Height = i.Height,
                BonusStr = i.BaseStr,
                BonusDex = i.BaseDex,
                BonusInt = i.BaseInt,
                BonusLuck = i.BaseLuck,
                DiceType = i.DiceType,
                Rarity = i.Rarity,
                Type = i.Type,
                SynergyBonusStr = i.SynergyBonusStr,
                SynergyBonusDex = i.SynergyBonusDex,
                SynergyBonusInt = i.SynergyBonusInt,
                SynergyTarget = i.SynergyTarget
            }).ToList(),
            Relics = _playerRelics.Select(r => new RelicData
            {
                Name = r.Name,
                Description = r.Description,
                EffectType = r.EffectType,
                Value = r.Value,
                StatTarget = r.StatTarget
            }).ToList(),
            
            CurrentRoom = new RoomData
            {
                Type = _floorManager.CurrentRoom.Type,
                Description = _floorManager.CurrentRoom.Description,
                EnemyName = _floorManager.CurrentRoom.EnemyName,
                EnemyHp = _floorManager.CurrentRoom.EnemyHp,
                EnemyMinDmg = _floorManager.CurrentRoom.EnemyMinDmg,
                EnemyMaxDmg = _floorManager.CurrentRoom.EnemyMaxDmg
            }
        };
        
        SaveManager.SaveGame(data, _currentSlotIndex);
    }

    private void LoadGame(int slotIndex)
    {
        var data = SaveManager.LoadGame(slotIndex);
        if (data == null) return;

        _selectedClass = data.Class;
        
        InitGameplayComponents();
        InitSkills();
        
        _playerHp = data.Hp;
        _playerMaxHp = data.MaxHp;
        _playerMana = data.Mana;
        _playerMaxMana = data.MaxMana;
        _playerGold = data.Gold;
        
        _playerRelics = new List<Relic>();
        if (data.Relics != null)
        {
            foreach (var rData in data.Relics)
            {
                _playerRelics.Add(new Relic(rData.Name, rData.Description, rData.EffectType, rData.Value, rData.StatTarget));
            }
        }
        
        foreach (var itemData in data.InventoryItems)
        {
            Color color = Color.Gray; 
            if (itemData.Rarity == ItemRarity.Unique) color = Color.OrangeRed;
            else if (itemData.Type == ItemType.Gem) color = Color.Red; 
            
            var item = new Item(itemData.Name, itemData.Width, itemData.Height, color, itemData.DiceType, itemData.Type)
            {
                GridX = itemData.GridX,
                GridY = itemData.GridY,
                BaseStr = itemData.BonusStr,
                BaseDex = itemData.BonusDex,
                BaseInt = itemData.BonusInt,
                BaseLuck = itemData.BonusLuck,
                Rarity = itemData.Rarity,
                SynergyBonusStr = itemData.SynergyBonusStr,
                SynergyBonusDex = itemData.SynergyBonusDex,
                SynergyBonusInt = itemData.SynergyBonusInt,
                SynergyTarget = itemData.SynergyTarget
            };
            
            _inventoryGrid.Items.Add(item);
        }
        
        _floorManager = new FloorManager(data);
        
        _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);
        
        LoadCurrentRoom();
    }

    private void OnItemUsed(Item item)
    {
        if (item.Name.Contains("Potion"))
        {
            int heal = 20;
            foreach (var relic in _playerRelics.Where(r => r.EffectType == RelicEffectType.OnHeal))
            {
                heal += relic.Value;
                _gameLog.AddMessage($"Relic {relic.Name} boosted heal!", Color.Cyan);
            }
            
            _playerHp = Math.Min(_playerHp + heal, _playerMaxHp);
            _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);
            _gameLog.AddMessage($"Used {item.Name}. Recovered {heal} HP.", Color.Green);
            
            _effectManager.AddText(new Vector2(VirtualWidth/2, VirtualHeight/2), $"+{heal} HP", Color.Green);
            _effectManager.AddHealEffect(new Vector2(VirtualWidth/2, VirtualHeight/2));
            _effectManager.TriggerFlash(Color.Green * 0.5f, 0.2f);
            
            _inventoryGrid.RemoveItem(item);
            SaveGame(); 
        }
        else
        {
            _gameLog.AddMessage($"Cannot use {item.Name}.", Color.Gray);
        }
    }

    private void LoadCurrentRoom()
    {
        Room room = _floorManager.CurrentRoom;
        _gameLog.AddMessage($"--- Floor {_floorManager.DungeonLevel}-{_floorManager.RoomNumber}: {room.Type} ---", Color.Cyan);
        _gameLog.AddMessage(room.Description, Color.White);

        _currentEnemy = null;
        _waitingForNextRoom = false;

        if (room.Type == RoomType.Combat || room.Type == RoomType.Elite || room.Type == RoomType.Boss)
        {
            _currentState = GameState.Playing;
            SpawnEnemy(room.EnemyName, room.EnemyHp, room.EnemyMinDmg, room.EnemyMaxDmg);
            
            foreach (var relic in _playerRelics.Where(r => r.EffectType == RelicEffectType.StartOfCombat))
            {
                if (relic.StatTarget == "Mana")
                {
                    _playerMana = Math.Min(_playerMana + relic.Value, _playerMaxMana);
                    _gameLog.AddMessage($"Relic {relic.Name} restored {relic.Value} Mana.", Color.Cyan);
                }
            }
            _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);
        }
        else if (room.Type == RoomType.Rest)
        {
            _currentState = GameState.Playing;
            int heal = 30;
            int mana = 20;
            _playerHp = Math.Min(_playerHp + heal, _playerMaxHp);
            _playerMana = Math.Min(_playerMana + mana, _playerMaxMana);
            _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);
            
            _gameLog.AddMessage($"You rest. +{heal} HP, +{mana} MP.", Color.Green);
            _effectManager.AddText(new Vector2(VirtualWidth/2, VirtualHeight/2), "RESTING...", Color.Cyan);
            _effectManager.AddHealEffect(new Vector2(VirtualWidth/2, VirtualHeight/2));
            
            _waitingForNextRoom = true;
            _gameLog.AddMessage("Press ENTER to continue.", Color.Yellow);
            
            SaveGame(); 
        }
        else if (room.Type == RoomType.Shop)
        {
            _currentState = GameState.Shop;
            InitShop();
        }
        else if (room.Type == RoomType.Event)
        {
            _currentState = GameState.Event;
            InitEvent();
        }
    }
    
    private void GenerateRoomSelection()
    {
        _nextRoomOptions = _floorManager.GenerateNextRoomOptions();
        _roomButtons = new List<Button>();
        
        int startX = VirtualWidth / 2 - 300;
        int y = VirtualHeight / 2 - 100;
        
        if (_nextRoomOptions.Count == 1)
        {
            var room = _nextRoomOptions[0];
            var btn = new Button(new Rectangle(VirtualWidth/2 - 100, y, 200, 250), $"BOSS\n{room.EnemyName}", _font, GraphicsDevice);
            btn.OnClick += () => {
                _floorManager.SetNextRoom(room);
                LoadCurrentRoom();
                SaveGame();
            };
            _roomButtons.Add(btn);
        }
        else
        {
            for (int i = 0; i < _nextRoomOptions.Count; i++)
            {
                var room = _nextRoomOptions[i];
                string text = $"{room.Type}";
                if (room.Type == RoomType.Combat) text += $"\n{room.EnemyName}";
                
                var btn = new Button(new Rectangle(startX + (i * 220), y, 200, 250), text, _font, GraphicsDevice);
                btn.OnClick += () => {
                    _floorManager.SetNextRoom(room);
                    LoadCurrentRoom();
                    SaveGame();
                };
                _roomButtons.Add(btn);
            }
        }
        
        _currentState = GameState.RoomSelection;
    }
    
    private void InitShop()
    {
        _shopButtons = new List<Button>();
        int centerX = VirtualWidth / 2;
        int startY = 300;
        
        var btnPotion = new Button(new Rectangle(centerX - 200, startY, 400, 60), "Buy Potion (50G)", _font, GraphicsDevice);
        btnPotion.OnClick += () => {
            if (_playerGold >= 50)
            {
                _playerGold -= 50;
                _inventoryGrid.AddItem(new Item("Potion", 1, 1, Color.Red, DiceType.None, ItemType.Consumable));
                _gameLog.AddMessage("Bought Potion.", Color.Green);
                SaveGame();
            }
            else _gameLog.AddMessage("Not enough Gold!", Color.Red);
        };
        _shopButtons.Add(btnPotion);
        
        var btnItem = new Button(new Rectangle(centerX - 200, startY + 80, 400, 60), "Buy Random Item (100G)", _font, GraphicsDevice);
        btnItem.OnClick += () => {
            if (_playerGold >= 100)
            {
                _playerGold -= 100;
                _inventoryGrid.AddItem(ItemGenerator.GenerateItem(_floorManager.DungeonLevel));
                _gameLog.AddMessage("Bought Item.", Color.Green);
                SaveGame();
            }
            else _gameLog.AddMessage("Not enough Gold!", Color.Red);
        };
        _shopButtons.Add(btnItem);
        
        var btnLeave = new Button(new Rectangle(centerX - 100, startY + 200, 200, 60), "Leave", _font, GraphicsDevice);
        btnLeave.OnClick += () => {
            GenerateRoomSelection();
        };
        _shopButtons.Add(btnLeave);
    }
    
    private void InitEvent()
    {
        _eventButtons = new List<Button>();
        int centerX = VirtualWidth / 2;
        int startY = 400;
        
        _eventText = "You find a strange altar. Do you want to pray?";
        
        var btnPray = new Button(new Rectangle(centerX - 250, startY, 200, 60), "Pray (-20G, +50HP)", _font, GraphicsDevice);
        btnPray.OnClick += () => {
            if (_playerGold >= 20)
            {
                _playerGold -= 20;
                _playerHp = Math.Min(_playerHp + 50, _playerMaxHp);
                _gameLog.AddMessage("You feel blessed.", Color.Green);
                _effectManager.AddHealEffect(new Vector2(VirtualWidth/2, VirtualHeight/2));
                GenerateRoomSelection();
            }
            else _gameLog.AddMessage("You need an offering.", Color.Red);
        };
        _eventButtons.Add(btnPray);
        
        var btnSteal = new Button(new Rectangle(centerX + 50, startY, 200, 60), "Steal (+50G, -20HP)", _font, GraphicsDevice);
        btnSteal.OnClick += () => {
            _playerGold += 50;
            _playerHp -= 20;
            _gameLog.AddMessage("You stole from the altar!", Color.Red);
            _effectManager.AddBloodEffect(new Vector2(VirtualWidth/2, VirtualHeight/2));
            _effectManager.TriggerShake(5f, 0.2f);
            
            if (_playerHp <= 0) _currentState = GameState.GameOver;
            else GenerateRoomSelection();
        };
        _eventButtons.Add(btnSteal);
        
        var btnIgnore = new Button(new Rectangle(centerX - 100, startY + 100, 200, 60), "Ignore", _font, GraphicsDevice);
        btnIgnore.OnClick += () => {
            _gameLog.AddMessage("You walk away.", Color.White);
            GenerateRoomSelection();
        };
        _eventButtons.Add(btnIgnore);
    }

    private void SpawnEnemy(string name, int hp, int minDmg, int maxDmg)
    {
        int sideWidth = (int)(VirtualWidth * 0.25f);
        int centerWidth = VirtualWidth - (sideWidth * 2);
        int gameHeight = (int)(VirtualHeight * 0.8f);
        Rectangle arenaBounds = new Rectangle(sideWidth, 0, centerWidth, gameHeight);

        _currentEnemy = new Enemy(name, hp, minDmg, maxDmg, GraphicsDevice, _font);
        
        int x = arenaBounds.Center.X - (_currentEnemy.Bounds.Width / 2);
        int y = arenaBounds.Center.Y - (_currentEnemy.Bounds.Height / 2) - 50;
        
        _currentEnemy.Bounds = new Rectangle(x, y, _currentEnemy.Bounds.Width, _currentEnemy.Bounds.Height);
        
        _gameLog.AddMessage($"A wild {name} appears! (ATK: {minDmg}-{maxDmg})", Color.Red);
    }

    private void GenerateLoot()
    {
        _lootOptions = new List<Item>();
        _lootRects = new Rectangle[3];
        
        int startX = VirtualWidth / 2 - 300;
        int y = VirtualHeight / 2 - 100;
        
        int goldGain = _random.Next(10, 30) + (_floorManager.DungeonLevel * 5);
        _playerGold += goldGain;
        _gameLog.AddMessage($"You found {goldGain} Gold!", Color.Gold);
        _effectManager.AddGoldEffect(new Vector2(VirtualWidth/2, VirtualHeight/2));

        if (_random.NextDouble() < 0.10)
        {
            var relicItem = new Item("Mysterious Relic", 1, 1, Color.Gold, DiceType.None, ItemType.Other);
            relicItem.Description = "A powerful artifact.";
            relicItem.Rarity = ItemRarity.Unique;
            _lootOptions.Add(relicItem);
            _lootRects[0] = new Rectangle(startX, y, 200, 250);
            
            for (int i = 1; i < 3; i++)
            {
                _lootOptions.Add(ItemGenerator.GenerateItem(_floorManager.RoomNumber));
                _lootRects[i] = new Rectangle(startX + (i * 220), y, 200, 250);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                _lootOptions.Add(ItemGenerator.GenerateItem(_floorManager.RoomNumber));
                _lootRects[i] = new Rectangle(startX + (i * 220), y, 200, 250);
            }
        }
        
        _currentState = GameState.Loot;
        _gameLog.AddMessage("Victory! Choose a reward.", Color.Gold);
        
        SaveGame(); 
    }

    private void OnTurnFinished(int totalDamage)
    {
        if (_currentState == GameState.GameOver) return;

        if (_currentEnemy != null && !_currentEnemy.IsDead)
        {
            int strBonus = _currentStr / 2;
            int finalDamage = totalDamage + strBonus;
            
            _currentEnemy.TakeDamage(finalDamage);
            _gameLog.AddMessage($"You dealt {finalDamage} damage! (Dice: {totalDamage} + Str: {strBonus})", Color.Orange);
            
            Vector2 enemyCenter = new Vector2(_currentEnemy.Bounds.Center.X, _currentEnemy.Bounds.Center.Y);
            _effectManager.AddBloodEffect(enemyCenter); 

            if (_currentEnemy.IsDead)
            {
                _gameLog.AddMessage($"{_currentEnemy.Name} is defeated!", Color.Green);
                _effectManager.AddExplosion(enemyCenter, Color.Gold, 30); 
                GenerateLoot();
            }
            else
            {
                int enemyDmg = _currentEnemy.Attack();
                int defense = _currentDex / 5;
                int finalEnemyDmg = Math.Max(0, enemyDmg - defense);
                
                _playerHp -= finalEnemyDmg;
                if (_playerHp < 0) _playerHp = 0;
                
                _hudOverlay.UpdateStats(_playerHp, _playerMaxHp, _playerMana, _playerMaxMana, _floorManager.DungeonLevel, _floorManager.RoomNumber);
                _gameLog.AddMessage($"{_currentEnemy.Name} attacks for {finalEnemyDmg} damage! (Blocked: {defense})", Color.Red);
                
                if (finalEnemyDmg > 0)
                {
                    _effectManager.TriggerFlash(Color.Red * 0.3f, 0.3f);
                    _effectManager.TriggerShake(10f, 0.3f);
                    _effectManager.AddDamageText(new Vector2(VirtualWidth/2, VirtualHeight/2 + 200), finalEnemyDmg, true);
                    _effectManager.AddBloodEffect(new Vector2(VirtualWidth/2, VirtualHeight/2 + 200)); 
                }

                if (_playerHp <= 0)
                {
                    _currentState = GameState.GameOver;
                    _gameLog.AddMessage("YOU DIED. Game Over.", Color.DarkRed);
                    _gameLog.AddMessage("Press R to return to Title.", Color.White);
                    SaveManager.DeleteSave(_currentSlotIndex); 
                }
            }
        }
        else if (_waitingForNextRoom)
        {
        }
        else
        {
            _gameLog.AddMessage($"Rolled {totalDamage}, but no target!", Color.Gray);
        }
    }

    private void OnResize(object sender, EventArgs e)
    {
        UpdateDestinationRectangle();
    }

    private void UpdateDestinationRectangle()
    {
        var screenSize = GraphicsDevice.PresentationParameters.Bounds;
        float scaleX = (float)screenSize.Width / VirtualWidth;
        float scaleY = (float)screenSize.Height / VirtualHeight;
        float scale = Math.Min(scaleX, scaleY);
        int newWidth = (int)(VirtualWidth * scale);
        int newHeight = (int)(VirtualHeight * scale);
        int posX = (screenSize.Width - newWidth) / 2;
        int posY = (screenSize.Height - newHeight) / 2;
        _destinationRectangle = new Rectangle(posX, posY, newWidth, newHeight);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
        float scaleX = (float)VirtualWidth / _destinationRectangle.Width;
        float scaleY = (float)VirtualHeight / _destinationRectangle.Height;
        Vector2 mouseVirtualPos = new Vector2(
            (mouseScreenPos.X - _destinationRectangle.X) * scaleX,
            (mouseScreenPos.Y - _destinationRectangle.Y) * scaleY
        );
        bool isLeftPressed = mouseState.LeftButton == ButtonState.Pressed;

        for (int i = 0; i < _bgParticles.Count; i++)
        {
            var p = _bgParticles[i];
            p.Pos.Y -= p.Speed;
            if (p.Pos.Y < 0) p.Pos.Y = VirtualHeight;
            _bgParticles[i] = p;
        }
        
        if (_effectManager != null) _effectManager.Update(gameTime);
        if (_currentEnemy != null) _currentEnemy.Update(gameTime); 

        switch (_currentState)
        {
            case GameState.TitleScreen:
                foreach (var btn in _titleButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.SlotSelection:
                foreach (var btn in _slotButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                foreach (var btn in _deleteButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                _backButton.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.ClassSelection:
                foreach (var btn in _classButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                _backButton.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.Options:
                foreach (var btn in _optionButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                _backButton.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.RoomSelection:
                foreach (var btn in _roomButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.Shop:
                foreach (var btn in _shopButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.Event:
                foreach (var btn in _eventButtons) btn.Update(mouseVirtualPos, isLeftPressed);
                break;
            case GameState.Playing:
                UpdatePlaying(gameTime, mouseVirtualPos, isLeftPressed, mouseState, keyboardState);
                break;
            case GameState.Loot:
                UpdateLoot(mouseState, mouseVirtualPos);
                break;
            case GameState.GameOver:
                UpdateGameOver(keyboardState);
                break;
        }

        _prevMouseState = mouseState;
        _prevKeyboardState = keyboardState;

        base.Update(gameTime);
    }
    
    private void UpdateGameOver(KeyboardState kState)
    {
        if (kState.IsKeyDown(Keys.R) && !_prevKeyboardState.IsKeyDown(Keys.R))
        {
            _currentState = GameState.TitleScreen;
        }
    }

    private void UpdateLoot(MouseState mouseState, Vector2 mouseVirtualPos)
    {
        if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
        {
            for (int i = 0; i < _lootRects.Length; i++)
            {
                if (_lootRects[i].Contains(mouseVirtualPos))
                {
                    var selectedItem = _lootOptions[i];
                    
                    if (selectedItem.Name == "Mysterious Relic")
                    {
                        var relic = new Relic("Holy Grail", "Heals +10 HP when using potions.", RelicEffectType.OnHeal, 10);
                        if (_random.Next(2) == 0)
                            relic = new Relic("Mana Crystal", "+10 Mana at start of combat.", RelicEffectType.StartOfCombat, 10, "Mana");
                        
                        _playerRelics.Add(relic);
                        _gameLog.AddMessage($"You obtained Relic: {relic.Name}!", Color.Gold);
                    }
                    else
                    {
                        _inventoryGrid.AddItem(selectedItem);
                        _gameLog.AddMessage($"You picked: {selectedItem.Name}", Color.Green);
                    }
                    
                    GenerateRoomSelection();
                    SaveGame(); 
                    break;
                }
            }
        }
    }

    private void UpdatePlaying(GameTime gameTime, Vector2 mouseVirtualPos, bool isLeftPressed, MouseState mouseState, KeyboardState keyboardState)
    {
        bool isRightClicked = mouseState.RightButton == ButtonState.Pressed && 
                              _prevMouseState.RightButton == ButtonState.Released;

        _inventoryGrid.Update(gameTime, mouseVirtualPos, isLeftPressed, isRightClicked);
        
        if (_skillButtons != null)
        {
            foreach (var btn in _skillButtons) btn.Update(mouseVirtualPos, isLeftPressed);
        }
        
        _currentStr = _baseStr + _inventoryGrid.TotalStr;
        _currentDex = _baseDex + _inventoryGrid.TotalDex;
        _currentInt = _baseInt + _inventoryGrid.TotalInt;
        _currentLuck = _baseLuck + _inventoryGrid.TotalLuck;
        
        foreach (var relic in _playerRelics.Where(r => r.EffectType == RelicEffectType.StatBonus))
        {
            if (relic.StatTarget == "Str") _currentStr += relic.Value;
            else if (relic.StatTarget == "Dex") _currentDex += relic.Value;
            else if (relic.StatTarget == "Int") _currentInt += relic.Value;
            else if (relic.StatTarget == "Luck") _currentLuck += relic.Value;
        }
        
        _infoPanel.UpdateStats(_currentStr, _currentDex, _currentInt, _currentLuck);
        
        if (keyboardState.IsKeyDown(Keys.Space) && !_prevKeyboardState.IsKeyDown(Keys.Space))
        {
            if (!_diceManager.IsRolling && !_waitingForNextRoom && _currentEnemy != null)
            {
                _diceManager.ThrowDices(_inventoryGrid.Items, _selectedClass);
                _gameLog.AddMessage("Rolling dice...", Color.White);
            }
        }
        
        if (keyboardState.IsKeyDown(Keys.D) && !_prevKeyboardState.IsKeyDown(Keys.D))
        {
            if (!_diceManager.IsRolling)
            {
                _diceManager.DebugSpawnAllDice();
                _gameLog.AddMessage("DEBUG: Rolling ALL dice!", Color.Yellow);
            }
        }
        
        if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
        {
            if (_waitingForNextRoom)
            {
                GenerateRoomSelection();
                SaveGame();
            }
        }
        
        _diceManager.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black); 

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        DrawBackground();

        switch (_currentState)
        {
            case GameState.TitleScreen:
                DrawTitleScreen();
                break;
            case GameState.SlotSelection:
                DrawSlotSelection();
                break;
            case GameState.ClassSelection:
                DrawClassSelection();
                break;
            case GameState.Options:
                DrawOptions();
                break;
            case GameState.RoomSelection:
                DrawRoomSelection();
                break;
            case GameState.Shop:
                DrawShop();
                break;
            case GameState.Event:
                DrawEvent();
                break;
            case GameState.Playing:
            case GameState.GameOver:
                DrawPlaying(gameTime);
                break;
            case GameState.Loot:
                DrawPlaying(gameTime); 
                DrawLootOverlay();
                break;
        }

        var mouseState = Mouse.GetState();
        float scaleX = (float)VirtualWidth / _destinationRectangle.Width;
        float scaleY = (float)VirtualHeight / _destinationRectangle.Height;
        Vector2 mousePos = new Vector2(
            (mouseState.X - _destinationRectangle.X) * scaleX,
            (mouseState.Y - _destinationRectangle.Y) * scaleY
        );
        
        Texture2D cursorTex = (mouseState.LeftButton == ButtonState.Pressed) ? _texCursorClicked : _texCursorNormal;
        if (cursorTex != null)
        {
            float cursorScale = 0.15f;
            _spriteBatch.Draw(cursorTex, mousePos, null, Color.White, 0f, Vector2.Zero, cursorScale, SpriteEffects.None, 0f);
        }

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new Color(20, 20, 20)); 

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_renderTarget, _destinationRectangle, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    private void DrawRoomSelection()
    {
        DrawPlaying(null);
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.8f);
        
        if (_titleFont != null)
        {
            string title = "CHOOSE YOUR PATH";
            Vector2 size = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - size.X)/2, 100), Color.White);
        }

        foreach (var btn in _roomButtons) btn.Draw(_spriteBatch);
    }
    
    private void DrawShop()
    {
        DrawPlaying(null);
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.8f);
        
        if (_titleFont != null)
        {
            string title = "MERCHANT";
            Vector2 size = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - size.X)/2, 100), Color.Gold);
            
            string gold = $"Gold: {_playerGold}";
            _spriteBatch.DrawString(_font, gold, new Vector2(VirtualWidth/2 - 50, 200), Color.Yellow);
        }

        foreach (var btn in _shopButtons) btn.Draw(_spriteBatch);
    }
    
    private void DrawEvent()
    {
        DrawPlaying(null);
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.8f);
        
        if (_titleFont != null)
        {
            string title = "EVENT";
            Vector2 size = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - size.X)/2, 100), Color.Cyan);
            
            Vector2 textSize = _font.MeasureString(_eventText);
            _spriteBatch.DrawString(_font, _eventText, new Vector2((VirtualWidth - textSize.X)/2, 250), Color.White);
        }

        foreach (var btn in _eventButtons) btn.Draw(_spriteBatch);
    }

    private void DrawBackground()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.DarkSlateBlue * 0.2f);
        
        foreach (var p in _bgParticles)
        {
            _spriteBatch.Draw(_pixel, new Rectangle((int)p.Pos.X, (int)p.Pos.Y, (int)p.Size, (int)p.Size), p.Color);
        }
    }

    private void DrawLootOverlay()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.8f);
        
        if (_titleFont != null)
        {
            string title = "CHOOSE YOUR REWARD";
            Vector2 titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - titleSize.X)/2, 200), Color.White);
        }

        for (int i = 0; i < _lootOptions.Count; i++)
        {
            Item item = _lootOptions[i];
            Rectangle rect = _lootRects[i];
            
            _spriteBatch.Draw(_pixel, rect, Color.DarkSlateGray);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), item.GetRarityColor());
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom, rect.Width, 2), item.GetRarityColor());
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), item.GetRarityColor());
            _spriteBatch.Draw(_pixel, new Rectangle(rect.Right, rect.Y, 2, rect.Height + 2), item.GetRarityColor());

            if (_font != null)
            {
                string name = item.Name;
                if (name.Length > 15) name = name.Substring(0, 12) + "...";
                
                _spriteBatch.DrawString(_font, name, new Vector2(rect.X + 10, rect.Y + 20), item.GetRarityColor());
                
                string stats = "";
                if (item.BaseStr > 0) stats += $"+{item.BaseStr} STR\n";
                if (item.BaseDex > 0) stats += $"+{item.BaseDex} DEX\n";
                if (item.BaseInt > 0) stats += $"+{item.BaseInt} INT\n";
                if (item.DiceType != DiceType.None) stats += $"Dice: {item.DiceType}\n";
                
                _spriteBatch.DrawString(_font, stats, new Vector2(rect.X + 10, rect.Y + 60), Color.White);
                
                _spriteBatch.DrawString(_font, item.Rarity.ToString(), new Vector2(rect.X + 10, rect.Bottom - 30), item.GetRarityColor());
            }
        }
    }

    private void DrawTitleScreen()
    {
        if (_texLogo != null)
        {
            float logoScale = 0.3f; 
            int logoWidth = (int)(_texLogo.Width * logoScale);
            int logoHeight = (int)(_texLogo.Height * logoScale);
            int logoX = (VirtualWidth - logoWidth) / 2;
            
            _spriteBatch.Draw(_texLogo, new Rectangle(logoX, 50, logoWidth, logoHeight), Color.White);
        }
        else if (_titleFont != null)
        {
            string title = "PROJECT BACKPACK DUNGEON";
            Vector2 titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - titleSize.X)/2, 200), Color.Gold);
        }
        
        foreach (var btn in _titleButtons) btn.Draw(_spriteBatch);
    }

    private void DrawSlotSelection()
    {
        if (_titleFont != null)
        {
            string title = "SELECT SAVE SLOT";
            Vector2 titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - titleSize.X)/2, 100), Color.White);
        }

        foreach (var btn in _slotButtons) btn.Draw(_spriteBatch);
        foreach (var btn in _deleteButtons) btn.Draw(_spriteBatch);
        _backButton.Draw(_spriteBatch);
    }

    private void DrawClassSelection()
    {
        if (_titleFont != null)
        {
            string title = "CHOOSE YOUR CLASS";
            Vector2 titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - titleSize.X)/2, 100), Color.White);
        }

        foreach (var btn in _classButtons) btn.Draw(_spriteBatch);
        _backButton.Draw(_spriteBatch);
    }
    
    private void DrawOptions()
    {
        if (_titleFont != null)
        {
            string title = "OPTIONS";
            Vector2 titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title, new Vector2((VirtualWidth - titleSize.X)/2, 100), Color.White);
        }

        foreach (var btn in _optionButtons) btn.Draw(_spriteBatch);
        _backButton.Draw(_spriteBatch);
    }

    private void DrawPlaying(GameTime gameTime)
    {
        Matrix transform = Matrix.Identity;
        if (_effectManager != null) transform = _effectManager.GetShakeMatrix();

        _spriteBatch.End(); 
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transform);

        DrawLayout();
        
        _infoPanel.Draw(_spriteBatch); 
        if (_currentEnemy != null) _currentEnemy.Draw(_spriteBatch);
        _diceManager.Draw(_spriteBatch); 
        _hudOverlay.Draw(_spriteBatch); 
        _gameLog.Draw(_spriteBatch); 
        
        if (_skillButtons != null)
        {
            foreach (var btn in _skillButtons) btn.Draw(_spriteBatch);
        }
        
        if (_effectManager != null)
        {
            _effectManager.Draw(_spriteBatch);
            _effectManager.DrawFlash(_spriteBatch, new Rectangle(0, 0, VirtualWidth, VirtualHeight));
        }
        
        if (_currentState == GameState.GameOver)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Red * 0.3f);
            if (_titleFont != null)
            {
                string text = "GAME OVER";
                Vector2 size = _titleFont.MeasureString(text);
                _spriteBatch.DrawString(_titleFont, text, new Vector2((VirtualWidth - size.X)/2, (VirtualHeight - size.Y)/2), Color.White);
            }
        }

        _spriteBatch.End();
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var mouseState = Mouse.GetState();
        float scaleX = (float)VirtualWidth / _destinationRectangle.Width;
        float scaleY = (float)VirtualHeight / _destinationRectangle.Height;
        Vector2 mouseVirtualPos = new Vector2(
            (mouseState.X - _destinationRectangle.X) * scaleX,
            (mouseState.Y - _destinationRectangle.Y) * scaleY
        );
        _inventoryGrid.Draw(_spriteBatch, mouseVirtualPos); 
    }

    private void DrawLayout()
    {
        int sideWidth = (int)(VirtualWidth * 0.25f);
        int centerWidth = VirtualWidth - (sideWidth * 2);
        int gameHeight = (int)(VirtualHeight * 0.8f);
        
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, sideWidth, VirtualHeight), Color.DarkSlateGray);
        _spriteBatch.Draw(_pixel, new Rectangle(sideWidth, 0, centerWidth, gameHeight), Color.DarkSlateBlue);
        _spriteBatch.Draw(_pixel, new Rectangle(sideWidth, gameHeight, centerWidth, VirtualHeight - gameHeight), Color.Black);
        _spriteBatch.Draw(_pixel, new Rectangle(sideWidth + centerWidth, 0, sideWidth, VirtualHeight), Color.DarkOliveGreen);

        int borderSize = 4;
        _spriteBatch.Draw(_pixel, new Rectangle(sideWidth - (borderSize/2), 0, borderSize, VirtualHeight), Color.Black);
        _spriteBatch.Draw(_pixel, new Rectangle((sideWidth + centerWidth) - (borderSize/2), 0, borderSize, VirtualHeight), Color.Black);
        _spriteBatch.Draw(_pixel, new Rectangle(sideWidth, gameHeight - (borderSize/2), centerWidth, borderSize), Color.Gray);
        
        int frameSize = 10;
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, frameSize), Color.Black);
        _spriteBatch.Draw(_pixel, new Rectangle(0, VirtualHeight - frameSize, VirtualWidth, frameSize), Color.Black);
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, frameSize, VirtualHeight), Color.Black);
        _spriteBatch.Draw(_pixel, new Rectangle(VirtualWidth - frameSize, 0, frameSize, VirtualHeight), Color.Black);
    }
}