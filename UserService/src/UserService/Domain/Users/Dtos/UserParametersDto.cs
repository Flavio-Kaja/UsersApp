namespace UserService.Domain.Users.Dtos;

using UserService.Dtos;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? MinDailyGoal { get; set; }
    public int? MaxDailyGoal { get; set; }
    public string SortBy { get; set; }
    public bool Descending { get; set; }
}
