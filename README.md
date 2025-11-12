# 🕹️ POE Game Project

> A connected game ecosystem demo — Unity 3D game + .NET backend + React website that syncs player progress in real-time.

![Unity](https://img.shields.io/badge/Game-Unity-000000?logo=unity)
![.NET](https://img.shields.io/badge/Backend-.NET-blueviolet?logo=dotnet)
![React](https://img.shields.io/badge/Frontend-React-61DAFB?logo=react)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)

---

## 🎥 Demo

> 📽️ *Coming soon — video walkthrough of the project showcasing the game, website, and backend integration.*

---

## 💡 Overview

This project is a **Proof of Concept (POE)** showcasing the integration of a **Unity 3D game**, a **C# .NET API backend**, and a **React TypeScript frontend**.  

The goal is to demonstrate how a player’s in-game progress can be linked to a companion website, creating a unified experience between gameplay and online interaction.

It also highlights how different technologies can work together and serves as an example of adaptable, full-stack development using modern tools.

---

## 🔗 Related Repositories

- 🎮 [Unity Game](https://github.com/Kaminari-Mizu/UnityGameProject1)  
- 🌐 [Frontend (Website)](https://github.com/Kaminari-Mizu/Game-Site)  
- ⚙️ [Backend (API)](https://github.com/Kaminari-Mizu/HorrorArpgBackend)

---

## 🧰 Tech Stack

- **Game:** Unity (3D engine)  
- **Backend:** C# and .NET API  
- **Database:** SQL Server  
- **Website:** React + TypeScript  
- **Version Control:** Git & GitHub  

---

## ⚙️ Prerequisites

Before running the project, make sure you have the following installed:

- [Node.js](https://nodejs.org/) (v18+)
- [.NET SDK](https://dotnet.microsoft.com/) (v8.0 or later)
- [SQL Server](https://www.microsoft.com/sql-server/)
- [Unity Hub](https://unity.com/download) (Unity 2022 or later)
- [Git](https://git-scm.com/)

---

## 🚀 How to Run the Project

### 1️⃣ Clone the Repositories

# Frontend
git clone https://github.com/Kaminari-Mizu/Game-Site

# Unity Game
git clone https://github.com/Kaminari-Mizu/UnityGameProject1

# Backend
git clone https://github.com/Kaminari-Mizu/HorrorArpgBackend
2️⃣ Backend Setup
Open the backend project (HorrorArpgBackend) in Visual Studio or VS Code.

Restore dependencies:

bash
Copy code
dotnet restore
Open appsettings.json and replace the "DefaultConnection" string with your own SQL Server connection string.

Apply migrations to set up the database:

bash
Copy code
dotnet ef database update --project horrorarpg-backend --startup-project HorrorArpg
Run the backend (make sure HorrorArpg is selected as the startup project):

bash
Copy code
dotnet run --project HorrorArpg
3️⃣ Frontend Setup
Open the Game-Site project in your editor (e.g. VS Code).

Install dependencies:

bash
Copy code
npm install
Start the development server:

bash
Copy code
npm run dev
The site should now be available at the URL printed in your terminal (e.g. http://localhost:5173).

4️⃣ Unity Game Setup
Open Unity Hub, then Add Project from Disk and select the cloned Unity project folder.

Once loaded, open Assets/Scenes/MainMenu.unity.

Make sure both the backend and frontend are running.

Press the Play button in Unity to start the game.

🕹️ How to Use the Project
Register on the Website

Create an account on the frontend website.

Log in through Unity

Use the same account credentials from the main menu.

The backend verifies your login details before allowing access.

Play the Game

Move, attack, swim, and jump in the environment.

Press Esc → Save to create a save file.

Load or delete saves from the main menu.

View Progress Online

After saving, go back to the website and refresh the page.

Your latest game data (scene, position, health, mana) will appear under Progress.

🧩 Project Purpose
This project demonstrates how Unity, .NET, and web technologies can work together to deliver a connected gameplay experience.
It was built as part of a Proof of Concept to explore:

Cross-platform data synchronization between a game and web app

Backend-driven player authentication and save management

Full-stack architecture using modern development practices

🔧 Troubleshooting
Issue	Possible Fix
Database connection fails	Ensure SQL Server is running and your appsettings.json connection string is correct.
Unity login not working	Confirm the backend API is running and the login URL in Unity is correct.
Website not showing save data	Check CORS settings in the backend and confirm the frontend is using the right API base URL.

💭 Future Improvements
Add refresh tokens for persistent authentication

Add player avatars and profile customization

Implement cloud saves instead of local database storage

Expand the Unity world with more gameplay interactions

🧑‍💻 Author & License
Author: Jason Saal
License: MIT
