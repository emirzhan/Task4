namespace Task4UserAdmin.ViewModels.Users;

public class UserListItemViewModel
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public string StatusCssClass { get; init; } = string.Empty;
    public string RegisteredAtText { get; init; } = string.Empty;
    public string LastLoginText { get; init; } = string.Empty;
    public string LastLoginTooltip { get; init; } = string.Empty;
    public bool IsCurrentUser { get; init; }
}
