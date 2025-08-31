🎨 CollaboBoard
A real-time collaborative drawing application built with ASP.NET Core and SignalR. Multiple users can draw together on a shared whiteboard by creating or joining rooms with unique room codes.
✨ Features
🏠 Room Management

Create Rooms: Generate unique 6-character room codes (e.g., ABC123)
Join Rooms: Enter room code and your name to join existing sessions
Real-time Sync: All drawing actions are instantly visible to everyone in the room

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

🎨 Drawing Tools

Pen Tool ✏️: Standard drawing with customizable colors and brush sizes
Eraser Tool 🧽: Remove existing drawings with larger brush size
Shape Tools 📐:
  Rectangle ⬜: Click and drag to create rectangles
  Circle ⭕: Draw circles from center point
  Line 📏: Create straight lines between two points

Live Preview: See dashed preview when drawing shapes
Touch Support: Full mobile device compatibility

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

👥 User Management

User Names: Required when joining rooms
Drawing Status: See who is currently drawing in real-time
Tool Indicators: Visual indicators showing what tool each user is using
User List: Live list of connected users with drawing status

📱 User Interface

Modern Design: Beautiful gradient UI with smooth animations
Responsive Layout: Works on desktop, tablet, and mobile
Activity Log: Comprehensive logging of all user actions
Real-time Notifications: Join/leave notifications and drawing status updates

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

🏗️ Architecture
Backend (ASP.NET Core)
├── Controllers/
│   └── RoomController.cs      # REST API endpoints
├── Hubs/
│   └── DrawingHub.cs          # SignalR hub for real-time communication
├── Models/
│   ├── DrawingEvent.cs        # Drawing data model
│   ├── Room.cs                # Room data model
│   └── ApiModels.cs           # API request/response models
├── Services/
│   ├── IRoomService.cs        # Room service interface
│   └── RoomService.cs         # Room management implementation
└── Program.cs                 # Application configuration

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

Frontend (HTML + JavaScript) - 
Modern HTML5 Canvas for drawing
SignalR JavaScript client for real-time communication
Responsive CSS with modern design
Touch-friendly controls for mobile devices




Made with ❤️ for collaborative creativity
