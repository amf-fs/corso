using System.Text;
using CorsoApi.Infrastructure;

namespace CorsoApiTests;

public class CsvParserTests
{
    [Fact(DisplayName = "When validate it should return a error message when header is empty")]
    public void EmptyHeader()
    {
        //Arrange
        var badCsvHeader = string.Empty;
        var parser = new CsvParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(badCsvHeader));

        //Act
        var actual = parser.Validate<Poco>(stream);

        //Assert
        Assert.False(actual.Succeeded);
        Assert.Equal(CsvParser.ErrorTypes.EmptyHeader, actual.Error?.Type);
    }

    [Fact(DisplayName = "When validate it should return a error message when header does not match POCO")]
    public void HeaderDoesNotMatchPoco()
    {
        //Arrange
        var badCsvHeader = @"badName, badQuantity
                           testName, 20";
        var parser = new CsvParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(badCsvHeader));

        //Act
        var actual = parser.Validate<Poco>(stream);

        //Assert
        Assert.False(actual.Succeeded);
        Assert.Equal(CsvParser.ErrorTypes.HeaderDoesNotMatch, actual.Error?.Type);
    }

    [Fact(DisplayName = "When validate it should return success when header matches POCO")]
    public void MatchingHeader()
    {
        //Arrange
        var csvHeader = @"title, quantity, username
                          titleName,testName, 20";
        var parser = new CsvParser();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvHeader));

        //Act
        var actual = parser.Validate<Poco>(stream);

        //Assert
        Assert.True(actual.Succeeded);
        Assert.Null(actual.Error);
    }

    [Fact(DisplayName = "after validate stream it should be able to read again")]
    public void CanReadStreamAfterValidation()
    {
        //Arrange
        var stream = new MemoryStream([1, 2, 3]);
        var parser = new CsvParser();
        
        //Act
        parser.Validate<Poco>(stream);
        
        //Assert
        Assert.True(stream.CanRead);
    }

    private class Poco
    {
        public required string Title {get; set;}
        public int Quantity {get; set;}
        public required string Username {get; set;} 
    }
}
