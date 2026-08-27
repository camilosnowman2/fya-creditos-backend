using FyaCreditos.Api.Dtos;
using Xunit;

namespace FyaCreditos.Api.Tests;

public class CreditoQueryParametersDtoTests
{
    [Fact]
    public void Page_menor_a_uno_se_normaliza_a_uno()
    {
        var query = new CreditoQueryParametersDto { Page = 0 };
        Assert.Equal(1, query.Page);

        query.Page = -5;
        Assert.Equal(1, query.Page);
    }

    [Fact]
    public void PageSize_fuera_de_rango_se_normaliza()
    {
        var query = new CreditoQueryParametersDto { PageSize = 0 };
        Assert.Equal(20, query.PageSize);

        query.PageSize = 1000;
        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void Valores_por_defecto_son_fecha_descendente()
    {
        var query = new CreditoQueryParametersDto();
        Assert.Equal(CreditoSortBy.Fecha, query.SortBy);
        Assert.Equal(SortOrder.Desc, query.Order);
    }
}
