using TesteFeriados.Api.Contracts;

namespace TesteFeriados.UnitTests.Contracts;

public class PagedResultTests
{
    private static PagedResult<string> Build(int totalCount, int pageSize)
        => new(Items: Array.Empty<string>(), Page: 1, PageSize: pageSize, TotalCount: totalCount);

    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(1, 5, 1)]
    [InlineData(5, 5, 1)]
    [InlineData(9, 5, 2)]
    [InlineData(10, 5, 2)]
    [InlineData(11, 5, 3)]
    public void TotalPages_CalculaCorretamente(int totalCount, int pageSize, int esperado)
    {
        var result = Build(totalCount, pageSize);

        Assert.Equal(esperado, result.TotalPages);
    }

    [Fact]
    public void TotalPages_PageSizeZero_NaoDividePorZero()
    {
        var result = Build(totalCount: 10, pageSize: 0);

        Assert.Equal(0, result.TotalPages);
    }
}
