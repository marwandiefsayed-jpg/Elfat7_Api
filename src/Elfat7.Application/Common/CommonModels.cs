namespace Elfat7.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

public class PagedFilterRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class Result<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result<T> Success(T data, string message = "تمت العملية بنجاح")
        => new() { Succeeded = true, Data = data, Message = message };

    public static Result<T> Failure(string message, List<string>? errors = null)
        => new() { Succeeded = false, Message = message, Errors = errors ?? new() };
}

public class Result
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static Result Success(string message = "تمت العملية بنجاح")
        => new() { Succeeded = true, Message = message };

    public static Result Failure(string message, List<string>? errors = null)
        => new() { Succeeded = false, Message = message, Errors = errors ?? new() };
}
