# Property Tycoon - Unity Game

A Monopoly-style board game developed in Unity, featuring both human and AI players.

## 📖 Project Overview

Property Tycoon is a digital board game similar to Monopoly, where players move around the board, buy properties, collect rent, and aim to become the wealthiest player.

## 🎮 Features

- **Player Support**: Supports both human and AI players
- **Property System**: Buy, sell, mortgage properties with houses and hotels
- **Card System**: 
  - **Opportunity Knock Cards**: Special event cards (16 cards)
  - **Potluck Cards**: Random chance cards (17 cards)
- **Board Elements**:
  - Properties with different color sets
  - Utilities and Stations
  - Tax spaces
  - Jail system
  - Free Parking (fine pool)
  - Go (collect £200)

## 📁 Project Structure

```
├── Property Tycoon.exe        # Game executable
├── Scripts.zip                # Source code archive
└── Scripts/                   # Unity C# scripts
    ├── GameManager.cs         # Main game logic and turn management
    ├── Player.cs              # Player class (human/AI)
    ├── MonopolyBoard.cs       # Board layout and movement
    ├── MonopolyNode.cs        # Board space properties
    ├── PlayerInfo.cs          # Player information display
    ├── MessageSystem.cs       # In-game messaging
    ├── UiShowPanel.cs         # UI panel management
    ├── UiShowProperty.cs      # Property display UI
    ├── UiShowStation.cs       # Station display UI
    ├── UiShowUtility.cs       # Utility display UI
    ├── Editor/                # Unity Editor scripts
    │   └── NodeSetEditor.cs   # Custom editor for node sets
    ├── Opportunity Knock Cards/  # Opportunity cards and scripts
    │   ├── OpKnockField.cs
    │   ├── Script_OpKnockCard.cs
    │   └── *.asset            # Card data files
    └── Potluck Cards/         # Potluck cards and scripts
        ├── Potluck.cs
        ├── Script_PotluckCard.cs
        └── *.asset            # Card data files
```

## 🎯 Game Rules

1. Players start with £1500
2. Roll dice to move around the board
3. Buy unowned properties you land on
4. Pay rent when landing on opponent's properties
5. Build houses and hotels to increase rent
6. Draw cards from Opportunity Knock or Potluck piles
7. Go to Jail for rolling 3 doubles or landing on "Go To Jail"
8. Pass GO to collect £200
9. Mortgage properties when low on cash
10. Last player standing wins!

## 🔧 Development

- **Engine**: Unity
- **Language**: C#
- **UI Framework**: TextMeshPro (TMP)

## 🚀 How to Play

Run `Property Tycoon.exe` to start the game.

## 📝 License

This project was developed for educational purposes.