using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt {  get; set; }

        public DateTime? RevokedAt { get; set; }

        public User User { get; set; } = null!;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
