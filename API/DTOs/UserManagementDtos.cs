namespace API.DTOs;

public class CreateHashedUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class CreateDirectUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class VerifyPasswordDto
{
    public int Id { get; set; }
    public string CandidatePassword { get; set; } = string.Empty;
}

public class VerifyPasswordResultDto
{
    public bool IsMatch { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
