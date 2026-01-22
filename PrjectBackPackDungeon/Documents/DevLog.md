# **DEVLOG : ProjectBagPackDungeon**

Ce document suit l'évolution du développement, les choix techniques effectués et les tâches restantes.

---

## **ÉTAT DU PROJET : BÊTA 1.5 (Procedural & UI Overhaul)**
**Date de dernière mise à jour :** 21/01/2026
**Version :** 1.5

Le jeu a franchi une étape majeure dans sa profondeur de gameplay et son identité visuelle. Le système de combat est désormais tactique grâce aux effets de statut et aux capacités ennemies.

---

## **1. FONCTIONNALITÉS IMPLÉMENTÉES**

### **A. Architecture & Moteur**
*   **Résolution Virtuelle :** 1920x1080 avec Letterboxing adaptatif.
*   **Game States :** Machine à états complète (`TitleScreen`, `SlotSelection`, `ClassSelection`, `Playing`, `Loot`, `GameOver`, `Options`, `RoomSelection`, `Shop`, `Event`).
*   **Nettoyage Automatique :** Les dés sont désormais systématiquement effacés après chaque action de combat pour maintenir une arène propre.

### **B. Système d'Inventaire & Items (The Grid)**
*   **Générateur Procédural d'Items :** Plus de 20 bases d'objets avec préfixes/suffixes dynamiques influençant les stats et les effets.
*   **Effets d'Items :** Les objets peuvent désormais appliquer des effets (Poison, Shield, Regen, Vulnerable, Weak) lors du lancer de dés.
*   **Synergies :** Les Gemmes boostent les stats des objets adjacents.
*   **Tooltips Premium :** Infobulles décalées du curseur, avec header coloré selon la rareté et bordures dynamiques.

### **C. Bestiaire & Combat (Procedural Enemies)**
*   **Générateur d'Ennemis :** Système de catégories (Beast, Undead, Construct, Demon, Ooze) avec modificateurs (Giant, Swift, Enraged, etc.).
*   **Éléments :** Monstres élémentaires (Fiery, Frozen, Venomous, etc.) avec teintes visuelles et effets de statut passifs.
*   **Capacités Ennemies (AI) :** Les monstres ne se contentent plus d'attaquer. Ils peuvent se soigner, se buffer (Shield), voler du Mana/Or ou infliger des debuffs.
*   **Status Effects :** Implémentation complète du Poison, Bleed, Weak, Vulnerable, Shield et Regen.

### **D. Interface Utilisateur (UI/UX Overhaul)**
*   **Orbes de Vie/Mana :** Sphères de liquide animées avec **vague sinusoïdale** fluide, situées dans les coins inférieurs pour libérer la zone de jeu.
*   **Portrait Joueur :** Portrait central abaissé sur le calque le plus haut, chevauchant le journal de combat pour un style "RPG classique".
*   **Cartes de Sélection (3D-ish) :** Les salles et le butin sont présentés sous forme de cartes élégantes qui grossissent (Hover) et s'illuminent au survol.
*   **Bouton d'Action :** Ajout d'un bouton "LANCER LES DES" pulsant dans l'arène pour guider le joueur.
*   **Événements Intégrés :** Les événements et le magasin s'affichent désormais directement dans l'arène centrale avec des visuels dédiés.

### **E. Progression & Map**
*   **Système de Biomes :** Descriptions et ambiances changeant selon l'étage (Caves, Ruins, Depths).
*   **Progression d'Étage :** Passage au niveau supérieur après la défaite d'un Boss à la salle 10.
*   **Salles Spéciales :** Ajout de salles "Treasure" cachées.

---

## **2. MODIFICATIONS RÉCENTES**

*   **UI Polish :** Déplacement des Skills dans le panneau de gauche sous les stats.
*   **Vague Sinusoïdale :** Rendu du liquide des orbes par bandes verticales pour un effet ondulant réaliste.
*   **Correction Tooltips :** Décalage des infobulles pour éviter qu'elles ne passent sous le curseur graphique.
*   **Stabilité Texte :** Suppression des accents et symboles spéciaux pour compatibilité SpriteFont.

---

## **3. TODO LIST (Reste à faire pour la Release)**

### **Priorité Haute (Polish)**
*   [ ] **Assets Graphiques :** Remplacer les placeholders par du Pixel Art (Monstres, Items, Décors).
*   [ ] **Audio :** Ajouter SFX (Impact, Dice Roll, UI) et Musique d'ambiance.

### **Priorité Moyenne (Contenu)**
*   [ ] **Reliques Passives :** Ajouter plus de reliques avec des effets uniques (ex: "Chaque 6 lancé donne 1 Shield").
*   [ ] **Arbre de Compétences :** Permettre au joueur de choisir de nouvelles compétences lors de la montée d'étage.

---

## **4. NOTES TECHNIQUES**

*   **Rendu Orbes :** Utilisation de `Math.Sin` avec découpage de texture en bandes verticales (step de 2px).
*   **Z-Order :** Le HUD est dessiné en dernier dans le SpriteBatch pour garantir la priorité visuelle.
*   **Procedural Logic :** Utilisation de dictionnaires de bases et de listes de modificateurs avec pondération aléatoire.
