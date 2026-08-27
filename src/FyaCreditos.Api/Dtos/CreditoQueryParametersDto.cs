namespace FyaCreditos.Api.Dtos;

public enum CreditoSortBy
{
    Fecha,
    Valor
}

public enum SortOrder
{
    Asc,
    Desc
}

/// <summary>
/// Parámetros aceptados por GET /api/creditos:
/// ?nombre=&cedula=&comercial=&sortBy=fecha|valor&order=asc|desc&page=&pageSize=
/// </summary>
public class CreditoQueryParametersDto
{
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string? Comercial { get; set; }
    public CreditoSortBy SortBy { get; set; } = CreditoSortBy.Fecha;
    public SortOrder Order { get; set; } = SortOrder.Desc;

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => value
        };
    }
}
