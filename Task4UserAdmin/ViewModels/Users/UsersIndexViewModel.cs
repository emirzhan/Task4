namespace Task4UserAdmin.ViewModels.Users;

public class UsersIndexViewModel
{
    public IReadOnlyList<UserListItemViewModel> Users { get; init; } = Array.Empty<UserListItemViewModel>();
}
