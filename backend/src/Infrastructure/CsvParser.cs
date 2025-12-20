
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

        var fields = header.Split(",", StringSplitOptions.TrimEntries);
        var givenType = typeof(T);
        var propertiesFromGivenType = givenType.GetProperties();
        var missingFields = new List<string>();

        foreach (var prop in propertiesFromGivenType)
        {
            if (!fields.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
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
        
        var header = await reader.ReadLineAsync()
            ?? throw new InvalidOperationException("File header cannot be empty!");
        
        var headerFields = header.Split(",", StringSplitOptions.TrimEntries);
        var givenType = typeof(T);
        var propIndexes = new Dictionary<string, int>();
        List<T> parsedItems = []; 

        foreach(var prop in givenType.GetProperties())
        {
            var propIndexOnHeader = Array.FindIndex(headerFields,
                 _ => _.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));

            propIndexes.Add(prop.Name, propIndexOnHeader);
        }

        while(!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var fields = line?.Split(",", StringSplitOptions.TrimEntries);

            if(fields is not null && fields.Length > 0)
            {
                var propValuesMap = new Dictionary<string, object?>();

                foreach(var propIndexItem in propIndexes)
                {
                    var value = fields.ElementAtOrDefault(propIndexItem.Value)
                        ?? throw new InvalidOperationException($"Csv line is bad formatted, could not find {propIndexItem.Key}");
                    
                    propValuesMap.Add(propIndexItem.Key, value);
                }
                
                T parsed = Activator.CreateInstance<T>();

                foreach(var prop in givenType.GetProperties())
                {
                    //Since we use Split, everything is a string already, so we only need convert types like int double and so on...
                    var value = prop.PropertyType != typeof(string) ? 
                        Convert.ChangeType(propValuesMap[prop.Name], prop.PropertyType) :
                        propValuesMap[prop.Name]; 

                    prop.SetValue(parsed, value);
                }

                parsedItems.Add(parsed);
            }
        }

        return parsedItems;
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

