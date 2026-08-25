using System.ComponentModel.DataAnnotations;

namespace UAT_System_API.Models
{
    public class Users
    {
        [Key]
        public int id { get; set; }
        public string? full_name { get; set; }
        public string? email { get; set; }
        public string? password_hash { get; set; }
        public string? role { get; set; }
        public string? department { get; set; }
        public string? phone { get; set; }
        public string? location { get; set; }
        public string? bio { get; set; }
        public bool dark_mode { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
    }
}