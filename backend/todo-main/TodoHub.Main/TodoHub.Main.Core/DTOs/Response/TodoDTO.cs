﻿

namespace TodoHub.Main.Core.DTOs.Response
{
    public class TodoDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsCompleted { get; set; }
        public Guid OwnerId { get; set; }
    }
}
