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
        Assert.Equal(0, someStream.Position);
    }

    [Fact(DisplayName = "it should parse valid csv file to POCO")]
    public async Task ParseValidCsvFile()
    {
        //Arrange
        var validCsv = @"title,quantity,username
                        some title,20,someUser
                        some title2,40,someUser2";
        
        //Act
        var actual = await _csvParser.ParseAsync<Poco>(validCsv.ToMemoryStream());

        //Assert
        var expected = new List<Poco>()
        {
            new()
            {
                Title = "some title",
                Quantity = 20,
                Username = "someUser"      
            },
            new()
            {
                Title = "some title2",
                Quantity = 40,
                Username = "someUser2"
            }
        };

        Assert.Equivalent(expected, actual);
    }

    [Fact(DisplayName = "it should parse valid csv that contains extra columns")]
    public async Task ParseValidCsvWithExtraColumns()
    {
        //Arrange
        var validCsvWithExtraColumns = @"extra1,title,quantity,extra2,username
                        extraData,some title,20,extraData2,someUser
                        extraData,some title2,40,extraData2,someUser2";
        
        //Act
        var actual = await _csvParser.ParseAsync<Poco>(validCsvWithExtraColumns.ToMemoryStream());

        //Assert
        var expected = new List<Poco>()
        {
            new()
            {
                Title = "some title",
                Quantity = 20,
                Username = "someUser"      
            },
            new()
            {
                Title = "some title2",
                Quantity = 40,
                Username = "someUser2"
            }
        };

        Assert.Equivalent(expected, actual);
    }

    [Fact(DisplayName="it should throw exception when parse and csv is missing poco property")]
    public async Task ShouldThrowIfMissingProperty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var csvWithMissingTitle = @"username, quantity
                                        aName, 20";
        
            return _csvParser.ParseAsync<Poco>(csvWithMissingTitle.ToMemoryStream());    
        });
    }

    [Fact(DisplayName="it should throw exception when parse and csv is bad formatted")]
    public async Task ShouldThrowIfBadFormat()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var csvWithMissingTitleValue = @"username, quantity,title
                                        aName, 20";
        
           return _csvParser.ParseAsync<Poco>(csvWithMissingTitleValue.ToMemoryStream());    
        });
    }

    [Fact(DisplayName = "it should not validate POCO properties when specified")]
    public async Task ExcludePropertiesFromValidation()
    {
        //Arrange
        var csvContentWithoutQuantity = @"title,username
                          titleName,testName";

        //Act
        var result = await _csvParser.ValidateAsync<Poco>(csvContentWithoutQuantity.ToMemoryStream(), poco => poco.Quantity);

        //Assert
        Assert.True(result.Succeeded);
    }

    [Fact(DisplayName = "it should not set POCO properties when specified on parsing")]
    public async Task DoNotSetPropertiesOnParsing()
    {
        //Arrange
        var csvContentWithoutQuantity = @"title,username
                          titleName,testName";

        //Act
        var actual = await _csvParser.ParseAsync<Poco>(csvContentWithoutQuantity.ToMemoryStream(), poco => poco.Quantity);

        //Arrange
        var parsed = actual.First();
        Assert.Equal(0, parsed.Quantity);
        Assert.Equal("titleName", parsed.Title);
        Assert.Equal("testName", parsed.Username);
    }

    private class Poco
    {
        public required string Title {get; set;}
        public int Quantity {get; set;}
        public required string Username {get; set;} 
    }
}
