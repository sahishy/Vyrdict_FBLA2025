# **Vyrdict - Official Documentation**

## **Table of Contents**
- [Introduction](#introduction)
- [Installation & Setup](#installation--setup)
  - [System Requirements](#system-requirements)
  - [Downloading & Running the Game](#downloading--running-the-game)
- [Gameplay Overview](#gameplay-overview)
  - [Objective](#objective)
  - [Key Features](#key-features)
  - [Game Loop](#game-loop)
- [Game Mechanics](#game-mechanics)
  - [Fate Timer](#fate-timer)
  - [Resources](#resources)
  - [Cards System](#cards-system)
  - [AI-Generated Ethical Dilemmas](#ai-generated-ethical-dilemmas)
  - [Multiplayer Mode](#multiplayer-mode)
- [Controls](#controls)
- [Technical Implementation](#technical-implementation)
  - [Procedural Island Generation](#procedural-island-generation)
  - [AI System](#ai-system)
  - [Multiplayer with Photon](#multiplayer-with-photon)
- [Graphics & UI Design](#graphics--ui-design)
- [Troubleshooting & FAQ](#troubleshooting--faq)
- [Credits & Acknowledgements](#credits--acknowledgements)

---

## **Introduction**  
Vyrdict is a **medieval, island-based ethical strategy game** that challenges players to make **morally complex decisions** while managing an isolated community. Every week, players must **balance limited resources**, respond to **AI-generated ethical dilemmas**, and **compete against time** before their community’s inevitable fate. With a **unique card-based gameplay system**, procedurally generated maps, and a competitive multiplayer mode, Vyrdict offers a **dynamic, thought-provoking experience** unlike any other strategy game.

---

## **Installation & Setup**

### **System Requirements**
| Component  | Minimum Requirements  | Recommended Requirements  |
|------------|----------------------|--------------------------|
| **OS**     | Windows 10/macOS  | Windows 11/macOS Monterey+ |
| **Processor** | Intel Core i5 / AMD Ryzen 5 | Intel Core i7 / AMD Ryzen 7 |
| **RAM**    | 8GB  | 16GB  |
| **Graphics** | Integrated GPU | NVIDIA GTX 1050 / AMD RX 560+  |
| **Storage**  | 2GB  | 5GB  |

### **Downloading & Running the Game**
1. **Download the game** from the official GitHub repository or competition-provided ZIP file.
2. **Extract the files** into a dedicated folder.
3. Run **`Vyrdict.exe`** (Windows) or **`Vyrdict.app`** (Mac).
4. (Optional) Adjust **settings** such as graphics, resolution, and volume before starting gameplay.

---

## **Gameplay Overview**

### **Objective**
- **Survive as many days as possible** by effectively managing resources and making ethical decisions.
- Each week, **players must draw, choose, and play cards** to impact their island community.
- Every decision affects **supplies, food, and gold**, requiring **long-term strategic planning**.
- In **multiplayer mode**, players compete to survive longer than their opponent.

### **Key Features**
 **AI-generated ethical dilemmas** that adapt to gameplay.  
 **Card-based decision-making system** balancing risk and reward.  
 **Procedural island generation** ensuring a fresh experience every game.  
 **Fate Timer** that determines how long the player has until inevitable failure.  
 **Multiplayer mode** for competitive survival.  

### **Game Loop**
1. **Card Draw Phase** - Players draft 5 out of 7 randomly drawn cards.
2. **Main Gameplay Phase** - Players play cards that impact resources and structures.
3. **End of Week Phase** - An **AI-generated ethical dilemma** presents a choice with consequences.
4. **Repeat Until Fate Timer Reaches 0** - Game ends when time runs out.

---

## **Game Mechanics**

### **Fate Timer**
- Starts at **14 days**, decreasing by **1 per in-game day**.
- **Negative stats accelerate** the countdown.
- Some **choices can extend or reduce** the timer.

### **Resources**
- **Supplies** - Needed for construction and maintaining buildings.
- **Food** - Feeds the population and prevents hunger-related dilemmas.
- **Gold** - Used for economic exchanges and securing supplies.

### **Cards System**
- **Buildable Cards** - Place new structures.
- **Upgrade Cards** - Improve existing buildings.
- **Convert Cards** - Exchange one resource for another.
- **Boost Cards** - Temporarily increase resources.
- **Crisis Cards** - Trigger emergency situations requiring immediate action.

### **AI-Generated Ethical Dilemmas**
- AI **analyzes player’s situation** and generates **a contextually appropriate scenario**.
- Players **choose between three options**, each impacting different resources.
- Some choices **affect the Fate Timer**, speeding up or delaying the inevitable.

### **Multiplayer Mode**
- Players enter **a room-based system** (Host & Opponent).
- Each player plays **separate games** but sees their opponent’s **survival progress**.
- **Goal:** Outlast the other player by making better decisions.

---

## **Controls**
 **Mouse** - Click to select & play cards.  
 **WASD/Arrow Keys** - Move camera.  
 **ESC** - Open menu.  

---

## **Technical Implementation**

### **Procedural Island Generation**
- Uses **Perlin Noise** to generate a **hexagonal map** each game.
- **Dynamic terrain placement** ensures unique gameplay every session.

### **AI System**
- AI **receives real-time game data** including:
  - Resource levels
  - Buildings
  - Fate Timer status
- Uses **GPT-4o-mini** to generate dilemmas tailored to the player’s situation.

### **Multiplayer with Photon**
- **Room-based system** with a host and a player.
- **Real-time survival tracking** updates between clients.
- **Automatic matchmaking & reconnection**.

---

## **Graphics & UI Design**
- **Medieval aesthetic** with **low-poly 3D models**.
- **Natural color palette** reflecting **earthy, survivalist tones**.
- **Simple UI** ensuring clarity and accessibility.
- **Accessibility options**: Color-blind mode, sound & UI scaling.

---

## **Troubleshooting & FAQ**
### **Q: The game crashes on startup.**
Ensure **DirectX 11+** is installed.  
Run the game as **Administrator**.  

### **Q: Multiplayer isn’t working.**
Check **internet connection**.  
**Disable VPN** if using one.  

### **Q: AI dilemmas aren’t appearing.**
Ensure the **Flask server** is running in the background.  

---

## **Credits & Acknowledgements**
### **Developers**
- **Sahish Durgam** - Lead Developer, AI Integration  
- **Nael Almutairi** - Multiplayer & Systems Developer  
- **Ryad Alharbi** - UI/UX, Graphics & Asset Design  

Thank you for playing **Vyrdict**!
