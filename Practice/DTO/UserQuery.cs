namespace Practice.DTO
{
    public class UserQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? Search { get; set; }
    }
}
