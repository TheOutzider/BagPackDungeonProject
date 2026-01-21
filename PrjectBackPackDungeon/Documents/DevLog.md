# **DEVLOG : ProjectBagPackDungeon**

Ce document suit l'évolution du développement, les choix techniques effectués et les tâches restantes.

---

## **ÉTAT DU PROJET : BÊTA 1.2 (BepuPhysics Integration)**
**Date de dernière mise à jour :** 20/01/2026
**Version :** 1.2

Le jeu est une Vertical Slice complète et jouable à l'infini.
Toutes les mécaniques principales (Combat, Inventaire, Map, Shop, Event, Progression) sont implémentées et fonctionnelles.

---

## **1. FONCTIONNALITÉS IMPLÉMENTÉES**

### **A. Architecture & Moteur**
*   **Résolution Virtuelle :** 1920x1080 avec Letterboxing adaptatif.
*   **Game States :** Machine à états complète (`TitleScreen`, `SlotSelection`, `ClassSelection`, `Playing`, `Loot`, `GameOver`, `Options`, `RoomSelection`, `Shop`, `Event`).
*   **Options :** Gestion persistante du Volume et du Plein Écran (`SettingsManager`).

### **B. Système d'Inventaire (The Grid)**
*   **Grille Tetris :** 6x8 cases. Drag & Drop, Rotation (Clic Droit).
*   **Synergies :** Les Gemmes boostent les stats des objets adjacents.
*   **Consommables :** Potions utilisables en combat.
*   **Reliques :** Objets passifs puissants (Bonus Stats, Mana, Soin) obtenus via Events ou Loot rare.

### **C. Système de Dés 3D (Physics & Rendering)**
*   **Rendu 3D :** Génération procédurale de meshes (Cube D6, Icosaèdre D20, Tétraèdre D4, Octaèdre D8, D10, D12) avec texture dynamique.
*   **Physique BepuPhysics v2 (v1.2) :** 
    *   Intégration complète du moteur physique BepuPhysics.
    *   Simulation réaliste des collisions (Rigid Body Dynamics).
    *   Gestion précise de la gravité, friction et restitution.
    *   Formes physiques adaptées (Box pour D6, Sphères pour les autres pour l'instant).
*   **Animation :** Synchronisation temps réel entre la simulation physique et le rendu visuel.
*   **Ombres :** Ombres portées dynamiques basées sur la hauteur réelle (Z) du dé.

### **D. Combat & Skills**
*   **Tour par Tour :** Joueur (Dés + Bonus Str) vs Ennemi.
*   **Compétences (Mana) :** 
    *   **Warrior :** Bash, Heal.
    *   **Mage :** Fireball, Heal, Reroll.
    *   **Rogue :** Stab, Reroll.
*   **Feedback :** Screen Shake (Impacts murs/ennemis), Flash Rouge, Textes Flottants, Particules.

### **E. Progression & Map**
*   **Room Selection :** Choix entre 3 salles après chaque victoire (Combat, Elite, Shop, Event, Rest).
*   **Économie :** Gain d'Or, Achat d'objets/soin au Shop.
*   **Événements :** Choix narratifs à risque (Prier, Voler, Ignorer).
*   **Boucle Infinie :** Dungeon Level infini avec scaling de difficulté (HP/Dégâts ennemis).

### **F. Persistance (Save System)**
*   **Multi-Slots :** 3 emplacements.
*   **Données Complètes :** Sauvegarde de l'inventaire, des stats, de la position, de la seed, de l'or et des reliques.

---

## **2. MODIFICATIONS RÉCENTES**

*   **BepuPhysics :** Remplacement de la physique "maison" par BepuPhysics v2 pour une simulation stable et réaliste.
*   **Physique 2.0 :** Refonte complète du moteur physique des dés. Ajout de collisions entre dés, gestion de masse, friction réaliste et lancers en cône.
*   **Dés Spéciaux :** Ajout des types D4, D8, D10, D12 avec couleurs et plages de valeurs spécifiques.
*   **Dés 3D :** Remplacement des sprites 2D par un moteur 3D custom intégré.
*   **Map Simplifiée :** Remplacement du graphe complexe par un choix de 3 portes ("Slay the Spire" -> "Hand of Fate").

---

## **3. TODO LIST (Reste à faire pour la Release)**

### **Priorité Haute (Polish)**
*   [ ] **Assets Graphiques :** Remplacer les placeholders (Carrés de couleur) par du Pixel Art.
*   [ ] **Audio :** Ajouter SFX (Impact, Dice Roll, UI) et Musique d'ambiance.

### **Priorité Moyenne (Contenu)**
*   [ ] **Plus d'Items :** Ajouter des armes légendaires et des sets d'armure.
*   [ ] **Bestiaire :** Diversifier les comportements ennemis (Debuffs, Soin, Invocations).
*   [x] **Dés Spéciaux :** Ajouter D4, D8, D10, D12.

---

## **4. NOTES TECHNIQUES**

*   **3D Rendering :** Utilisation de `BasicEffect` avec `VertexPositionNormalTexture`.
*   **Texture Generation :** Utilisation de `RenderTarget2D` pour créer les textures de dés à la volée.
*   **Physics :** BepuPhysics v2. Simulation Rigid Body complète. Conversion Vector3 (System.Numerics) <-> Vector3 (XNA).