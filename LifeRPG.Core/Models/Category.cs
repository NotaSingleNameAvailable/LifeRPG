using System;
using System.Collections.Generic;

namespace LifeRPG.Core.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;

        // Navigation property - matches the WithMany configuration
        public virtual ICollection<RpgTask> Tasks { get; set; } = new List<RpgTask>();
    }
}