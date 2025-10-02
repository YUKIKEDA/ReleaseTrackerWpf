using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService7
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException("Name is required");
        }
        
        public virtual async Task<bool> ProcessAsync()
        {
            await Task.Delay(100);
            return true;
        }
    }
}
