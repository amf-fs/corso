using System.Text;
using CorsoApi.Infrastructure;

namespace CorsoApiTests;

public class CsvParserTests
{
    private readonly CsvParser _csvParser;

    public CsvParserTests()
    {
        _csvParser = new CsvParser();
    }
    

    [Fact(DisplayName = "When validate it should return a error message when header is empty")]
    public async Task EmptyHeader()
    {
        //Arrange
        var emptyCsv = string.Empty;

        //Act
        var actual = await _csvParser.ValidateAsync<Poco>(emptyCsv.ToMemoryStream());

        //Assert
        Assert.False(actual.Succeeded);
        Assert.Equal(CsvParser.ErrorTypes.EmptyHeader, actual.Error?.Type);
    }

    [Fact(DisplayName = "When validate it should return a error message when header does not match POCO")]
    public async Task HeaderDoesNotMatchPoco()
    {
        //Arrange
        var badCsvHeader = @"badName, badQuantity
                           testName, 20";

        //Act
        var actual = await _csvParser.ValidateAsync<Poco>(badCsvHeader.ToMemoryStream());

        //Assert
        Assert.False(actual.Succeeded);
        Assert.Equal(CsvParser.ErrorTypes.HeaderDoesNotMatch, actual.Error?.Type);
    }

    [Fact(DisplayName = "When validate it should return success when header matches POCO")]
    public async Task MatchingHeader()
    {
        //Arrange
        var csvContent = @"title, quantity, username
                          titleName,testName, 20";

        //Act
        var actual = await _csvParser.ValidateAsync<Poco>(csvContent.ToMemoryStream());

        //Assert
        Assert.True(actual.Succeeded);
        Assert.Null(actual.Error);
    }

    [Fact(DisplayName = "after validate stream it should be able to read again")]
    public async Task CanReadStreamAfterValidation()
    {
        //Arrange
        var someStream = new MemoryStream([1, 2, 3]);
        
        //Act
        await _csvParser.ValidateAsync<Poco>(someStream);
        
        //Assert
        Assert.True(someStream.CanRead);
    }

    private class Poco
    {
        public required string Title {get; set;}
        public int Quantity {get; set;}
        public required string Username {get; set;} 
    }
}
