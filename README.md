# Non-Transitive Dice Game

A console-based implementation of a generalized non-transitive dice game, where the outcome of dice rolls defies traditional transitive logic. This creates a rock-paper-scissors-like dynamic where dice A might beat dice B, dice B might beat dice C, but dice C might beat dice A.

## Overview

This application allows you to play against the computer using custom, non-transitive dice configurations. The game features:

- Support for arbitrary dice values and multiple dice configurations
- Provably fair first-move determination and dice rolling
- Interactive console interface with comprehensive help
- Statistical probability analysis for win rates between different dice
- Visual console tables with win probabilities for strategic decision-making

## Demo

https://drive.google.com/file/d/1aLHhh-MO_MPUVjJE6Ohs6fzWxoc68Biw/view?usp=sharing

The demo video demonstrates:
1. Starting the game with custom dice configurations
2. The process of determining who makes the first move
3. Selecting dice for player and computer
4. The fair roll mechanism for determining dice outcomes
5. Accessing the help menu with probability tables
6. Full gameplay experience from start to finish

## Requirements

- .NET SDK (6.0 or newer recommended)
- ConsoleTables NuGet package

## Installation

1. Clone the repository or download the source code
2. Install the required NuGet package:
   ```
   dotnet add package ConsoleTables
   ```
3. Build the application:
   ```
   dotnet build
   ```

## Usage

Run the application from the command line, providing at least three dice configurations as command-line arguments. Each dice configuration should be a string of 6 comma-separated integers:

```
dotnet run 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7
```

Or if using the compiled executable:

```
NonTransitiveDiceGame.exe 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7
```

### Game Controls

Throughout the game, you can use these controls:
- Numeric keys (0-9): Make selections from the menu
- `?`: Display help, including game rules and probability tables
- `X`: Exit the current stage or the entire game

## How to Play

1. **First Move Determination**:
   - The computer selects a random bit (0 or 1) and provides an HMAC as proof
   - You guess the bit
   - If you guess correctly, you go first; otherwise, the computer goes first

2. **Dice Selection**:
   - The first player selects a dice from the available configurations
   - The second player selects a dice from the remaining configurations

3. **Dice Rolling**:
   - Each player rolls their selected dice using a provably fair mechanism
   - The computer selects a random value and provides an HMAC as proof
   - You contribute additional randomness by selecting a value
   - The final value is determined by combining both inputs
   - The player who rolls the higher number wins

## Understanding Non-Transitive Dice

The help menu (accessed by typing `?` during gameplay) displays a probability table showing the likelihood of winning when using each dice against others. This non-transitive property creates an interesting strategic element, as no single dice is universally superior.

In the example dice set (`2,2,4,4,9,9`, `1,1,6,6,8,8`, `3,3,5,5,7,7`):
- The first dice tends to beat the second
- The second dice tends to beat the third
- The third dice tends to beat the first

This creates a circular relationship similar to rock-paper-scissors.

## Provably Fair Mechanism

The game uses cryptographic techniques to ensure fair play:
- Random selections are committed using HMAC-SHA256
- The computer reveals the cryptographic key after selections are made
- This allows verification that the computer didn't change its choice after seeing yours

