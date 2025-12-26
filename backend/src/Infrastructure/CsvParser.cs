
namespace CorsoApi.Infrastructure;

public class CsvParser
{
    public async Task<ValidationResult> ValidateAsync<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var header = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(header))
        {
            return new ValidationResult()
            {
                Error = new Error
                {
                    Type = ErrorTypes.EmptyHeader,
                    Message = $"The header is empty."
                }
            };
        }

        var fieldsFromHeader = header.Split(",", StringSplitOptions.TrimEntries);
        var givenType = typeof(T);
        var propertiesFromGivenType = givenType.GetProperties();
        var missingFields = new List<string>();

        foreach (var prop in propertiesFromGivenType)
        {
            if (!fieldsFromHeader.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
            {
                missingFields.Add(prop.Name.ToLower());
            }
        }

        if(missingFields.Count > 0)
        {
            return new ValidationResult()
            {
                Error = new Error
                {
                    Type = ErrorTypes.HeaderDoesNotMatch,
                    Message = $"The header does not match with provided type: {givenType.Name}, missing fields: {string.Join(", ", missingFields)}."
                }
            };
        }

        return new ValidationResult()
        {
            Succeeded = true
        };
    }

    public async Task<IEnumerable<T>> ParseAsync<T>(Stream stream)
    {
        var reader = new StreamReader(stream, leaveOpen: true);
        
        var headerLine = await reader.ReadLineAsync()
            ?? throw new InvalidOperationException("File header cannot be empty!");
        
        var indexesFromType = MapIndexesFromType<T>(headerLine);

        //starts at 2 because of header.
        List<T> parsedItems = []; 
        var lineNumber = 2;
        while(!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync()
                ?? throw new InvalidOperationException($"Csv line {lineNumber} is empty.");

            T parsed = ParseItem<T>(indexesFromType, line, lineNumber);
            parsedItems.Add(parsed);
            lineNumber++;
        }

        return parsedItems;
    }

    /// <summary>
    /// It finds the property name on given header line and maps the index
    /// With this technique it is possible to access the values by position.
    /// </summary>
    private static Dictionary<string, int> MapIndexesFromType<T>(string headerLine)
    {
        var headerFields = headerLine.Split(",", StringSplitOptions.TrimEntries);
        var givenType = typeof(T);
        var headerIndexes = new Dictionary<string, int>();
        List<T> parsedItems = []; 

        foreach(var prop in givenType.GetProperties())
        {
            var propIndexOnHeader = Array.FindIndex(headerFields,
                 _ => _.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));

            if(propIndexOnHeader == -1)
            {
                throw new InvalidOperationException($"Required header {prop.Name} is missing at csv file.");
            }

            headerIndexes.Add(prop.Name, propIndexOnHeader);
        }

        return headerIndexes;
    }

    private static T ParseItem<T>(Dictionary<string, int> indexesOfValues, string csvLine, int lineNumber)
    {
        var fields = csvLine?.Split(",", StringSplitOptions.TrimEntries);

        if (fields is null || fields.Length == 0)
        {
            throw new InvalidOperationException($"Csv line {lineNumber} is empty.");
        }

        var propValuesMap = new Dictionary<string, object?>();

        foreach (var propIndexItem in indexesOfValues)
        {
            var value = fields.ElementAtOrDefault(propIndexItem.Value)
                ?? throw new InvalidOperationException($"Csv line {lineNumber} is bad formatted, could not find {propIndexItem.Key} value.");

            propValuesMap.Add(propIndexItem.Key, value);
        }

        T item = Activator.CreateInstance<T>();

        foreach (var prop in typeof(T).GetProperties())
        {
            //Since we use Split, everything is a string already, so we only need convert types like int double and so on...
            var value = prop.PropertyType != typeof(string) ?
                Convert.ChangeType(propValuesMap[prop.Name], prop.PropertyType) :
                propValuesMap[prop.Name];

            prop.SetValue(item, value);
        }

        return item;
    }

    public class ValidationResult
    {
        public bool Succeeded { get; set; }
        public Error? Error {get; set;}
    }

    public class Error
    {
        public ErrorTypes Type {get; set;}
        public required string Message {get; set;}
    }

    public enum ErrorTypes
    {
        EmptyHeader,
        HeaderDoesNotMatch
    }
}

