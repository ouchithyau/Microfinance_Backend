namespace Microfinance.Application.Features.Identity.Models;

public record AuthResponse(
    Guid Id,
    string Username,
    string FullName,
    string Role,
    Guid BranchId,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiry
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);