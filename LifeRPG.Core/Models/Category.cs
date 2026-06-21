using System;
using System.Collections.Generic;

namespace LifeRPG.Core.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ColorCode { get; set; }

        // Navigation property - matches the WithMany configuration
        public virtual ICollection<RpgTask> Tasks { get; set; } = new List<RpgTask>();
    }
}