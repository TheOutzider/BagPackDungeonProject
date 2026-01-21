# **CANEVAS DE PRODUCTION :**  **ProjectBagPackDungeon**

## **1\. VISION TECHNIQUE & STACK**

**Objectif :** Créer un Roguelite hybride (Textuel/RPG/Gestion) robuste, modulaire et hautement testable.

### **Technologies**

* **Langage :** C\# (.NET 8\)  
* **Moteur :** MonoGame 3.8.1 (DesktopGL / Cross-Platform)  
* **Physique :** Moteur Custom "Fake 3D" (Cercles 2D \+ Hauteur Z simulée) pour les dés.  
* **Données :** System.Text.Json (Sauvegardes).  
* **UI :** Framework Custom (Boutons, Grilles, Tooltips).

## **2\. ARCHITECTURE DE L'INTERFACE (LAYOUT TRIPTYQUE)**

L'écran (1920x1080) est divisé en trois colonnes distinctes.  
**Ratio Actuel :** 25% / 50% / 25%.

### **Zone A : Informations (Gauche - 25%)**

* **Panneau Stats :** Affichage riche des attributs (Force, Dex, Int, Luck).  

### **Zone B : La Scène (Centre - 50%)**

* **Partie Haute (80%) : L'Arène**
  * Scène de combat (Ennemis, Dés).
  * HUD (Orbes Santé/Mana, Portrait).
  * Effets visuels (Particules, Textes flottants).
* **Partie Basse (20%) : Le Log**
  * Console textuelle déroulante.

### **Zone C : Gestion & Meta (Droite - 25%)**

* **Inventory Grid (Tetris) :**  
  * Matrice interactive (6x8) pour placer les objets.  
  * Gestion du Drag & Drop et Rotation (Clic Droit).  
  * Utilisation des consommables (Clic Droit).
  * Tooltips dynamiques.

## **3\. CORE SYSTEMS (MÉCANIQUES)**

### **3.1. Inventory System (The Grid)**

* **Synergies :** Les objets (Gemmes) boostent leurs voisins (Adjacency Bonus).  
* **Stats RPG :** L'équipement définit les stats (Str, Dex, Int) et le Dice Pool.

### **3.2. Dice System (The Engine)**

* **Physique :** Collision 2D, Frottement élevé pour tours rapides.
* **Résolution :** Somme des valeurs = Dégâts bruts.

### **3.3. Loot System (Diablo-like)**

* **Génération Procédurale :** Items avec Rareté (Common -> Unique) et Affixes.
* **Choix :** Sélection de récompense après chaque combat.

### **3.4. Combat System**

* **Tour par Tour :** Joueur (Attaque) -> Ennemi (Riposte).
* **Calculs :** Dégâts = (Dés + Str) - (Dex Ennemi).

### **3.5. Persistance**

* **Sauvegarde :** 3 Slots. JSON.
* **Auto-Save :** À chaque étape clé.
* **Permadeath :** Suppression à la mort.

## **4\. DONNÉES & PROCÉDURAL**

* **ItemGenerator :** Génère des items infinis basés sur des templates.
* **FloorManager :** Génère une suite de salles (Combat, Repos, Boss).

## **5\. PLAN DE DÉVELOPPEMENT (ROADMAP)**

### **PHASE 1 : LE PROTOTYPE (GREY BOX) - TERMINÉ**

*   [x] Layout & UI.
*   [x] Inventaire complet.
*   [x] Combat & Dés.
*   [x] Loot & Progression.
*   [x] Sauvegardes.

### **PHASE 2 : LE VISUEL & RENDU (EN COURS)**

*   [x] Écran Titre Graphique.
*   [x] Feedback Visuel (Juice).
*   [ ] Intégration des sprites In-Game.

### **PHASE 3 : CONTENU & POLISH**

*   [ ] Audio (SFX, Musique).
*   [ ] Map FTL (Arborescence).
*   [ ] Plus d'ennemis et d'items.