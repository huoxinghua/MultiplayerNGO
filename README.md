# MultiplayerNGO - Unity Multiplayer Game Project

## Project Overview

MultiplayerNGO is a Unity-based multiplayer network game project that utilizes Unity Netcode for GameObjects (NGO) and Steam networking services to implement multiplayer functionality. The game combines first-person exploration, NPC interactions, item collection, and voice chat elements.

## Key Features

### 🌐 Multiplayer Networking
- **Steam Integration**: P2P connections using Steam networking services
- **Voice Chat**: Distance-based 3D spatial voice chat system
- **Lobby System**: Create and join Steam lobbies
- **Network Synchronization**: Real-time synchronization of player states, items, and NPC behaviors

### 🎮 Gameplay Features
- **First-Person Controller**: Complete FPS controller with movement, jumping, crouching, and sprinting
- **NPC System**: 
  - **Tranquil NPCs**: Peaceful Beetle NPCs that follow players or flee
  - **Violent NPCs**: Hostile Brute NPCs with attack and tracking behaviors
- **Item System**: Pickup, use, and trade items (flashlights, baseball bats, etc.)
- **State Machine**: Both players and NPCs use state machine systems for behavior management

### 🗺️ Level Design
- **Procedural Dungeons**: Random dungeon generation using DunGen plugin
- **Multiple Scenes**: Includes test scenes, showcase scenes, and official levels
- **Terrain System**: Unity Terrain system support

## Technology Stack

### Core Frameworks
- **Unity 2023.x**: Game engine
- **Unity Netcode for GameObjects 2.5.0**: Multiplayer networking framework
- **Steamworks.NET**: Steam API integration
- **Unity Input System**: Input management
- **Universal Render Pipeline (URP)**: Rendering pipeline

### Third-Party Plugins
- **DunGen**: Procedural dungeon generation
- **ProximityChat**: Voice chat system
- **QuickOutline**: Item highlighting effects
- **FMOD**: Audio system

## Project Structure

```
Assets/
├── _Project/                    # Main game code
│   ├── Code/
│   │   ├── Gameplay/
│   │   │   ├── Player/          # Player control system
│   │   │   ├── NPC/             # NPC system
│   │   │   └── NewItemSystem/   # Item system
│   │   └── Utilities/           # Utility classes
│   ├── Prefabs/                 # Prefabs
│   ├── Scenes/                  # Game scenes
│   └── ScriptableObjects/       # Scriptable objects
├── Network/                     # Network-related code
│   └── Scripts/
│       ├── SteamWork/          # Steam integration
│       └── ProximityChat/      # Voice chat
├── DunGen/                     # Dungeon generation plugin
└── Plugins/                    # Third-party plugins
```

## Installation and Setup

### Requirements
- Unity 2023.x or higher
- Steam client
- Windows 10/11

### Installation Steps

1. **Clone the Project**
   ```bash
   git clone [project-url]
   cd MultiplayerNGO
   ```

2. **Open Unity Project**
   - Open the project folder using Unity Hub
   - Wait for Unity to import all assets

3. **Steam Configuration**
   - Ensure Steam client is running
   - Project root contains `steam_appid.txt` file

4. **Run the Game**
   - Open the main menu scene in Unity editor
   - Click play button to start the game

### Multiplayer Setup

1. **Create Lobby**
   - Click "Host Public" to create a public lobby
   - Or click "Host Friends Only" to create a friends-only lobby

2. **Join Game**
   - Click "Join Game" to browse available lobbies
   - Select a lobby to join

3. **Invite Friends**
   - Click "Invite Friends" button in lobby
   - Invite through Steam friends system

## Game Controls

### Basic Movement
- **WASD**: Move
- **Space**: Jump
- **Left Shift**: Sprint
- **Left Ctrl**: Crouch
- **Mouse**: Look around

### Interactions
- **E**: Pick up items
- **Left Mouse**: Use items
- **Tab**: Open menu

### Voice Chat
- **V**: Hold to talk (Push-to-Talk)
- Voice range is distance-based, other players closer to you will hear you more clearly

## Development Notes

### Network Architecture
- Client-server architecture using Unity Netcode for GameObjects
- Steam Networking Sockets as transport layer
- Server-authoritative game state management

### NPC System
- State machine-based AI behavior
- Network-synchronized NPC states
- Support for different NPC behavior patterns

### Item System
- Interface-based item system design
- Support for pickup, use, equip, and trade
- Network-synchronized item states

## Contributing

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Contact

For questions or suggestions, please contact:
- Create an Issue
- Send email to [contact-email]

## Acknowledgments

- Unity Technologies - Game engine
- Steam - Networking services
- DunGen - Dungeon generation tool
- All contributors and testers

---

**Note**: This is a work-in-progress project. Some features may still be under development. Please test all functionality in release builds.