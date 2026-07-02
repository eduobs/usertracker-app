using System;

namespace UserTracker.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string GoogleSubjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.Common;
    public bool IsApproved { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
