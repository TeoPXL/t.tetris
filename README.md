# t.tetris
> no one knows that the t stands for

A Tetris implementation in Unity, focusing on clean architecture and data-driven design.

## Features

- **Classic Gameplay**: Complete implementation of standard Tetris mechanics including:
  - **Hold Piece**: Swap your current piece with a held one for strategic play.
  - **Next Queue**: Preview the next 3 incoming pieces.
  - **Scoring**: Traditional scoring system (100/300/500/800 points for 1-4 lines).
  - **Progressive Difficulty**: Game speed increases every minute of survival.
- **Visuals**: 
  - Dynamic camera fitting system to support various aspect ratios.
  - Smooth grid rendering.
- **Audio**: Music, Sound effects for movement and line clearing.

## Technical Implementation

This project leverages **ScriptableObjects** to maintain a flexible and data-driven architecture:

- **Block Data**: Each tetromino shape is defined as a `BlockData` ScriptableObject, allowing for easy editing of shapes and colors without touching code.
- **Localization**: A custom `LanguageData` ScriptableObject system handles translations, making it trivial to add new languages.
- **Custom Blocks**: The system supports loading custom block definitions from PlayerPrefs (JSON), demonstrating a hybrid approach of static (ScriptableObject) and dynamic (JSON) data loading.

## Development

- **Engine**: Built with Unity
- **Architecture**: Uses a centralized `GameManager` for state (Menu, Playing, Paused, GameOver) and a discrete `Board` controller for grid logic.
