namespace Mantaras.Juridico.Application.Common.Pagination;

public abstract class PagedRequest
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1)
            {
                _pageSize = DefaultPageSize;
                return;
            }

            _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
