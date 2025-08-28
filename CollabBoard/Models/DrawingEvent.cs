using System;
using System.ComponentModel.DataAnnotations;

namespace CollabBoard.Models
{
    public class DrawingEvent
    {
        public string Type { get; set; } = string.Empty; // "startDrawing", "drawing", "stopDrawing"
        public float X { get; set; }
        public float Y { get; set; }
        public string Color { get; set; } = "#000000";
        public int LineWidth { get; set; } = 2;
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
