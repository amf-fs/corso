
using System.Linq.Expressions;

namespace CorsoApi.Infrastructure;

public class CsvParser
{
    public async Task<ValidationResult> ValidateAsync<T>(Stream stream, params Expression<Func<T, object>>[] doNotValidate)
    {
        if(!stream.CanSeek)
        {
            throw new InvalidOperationException("Csv parser only support seekable streams!");
        }

        try
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
            var doNotValidateNames = GetNames(doNotValidate);

            foreach (var prop in propertiesFromGivenType)
            {
                if(doNotValidateNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

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
        finally
        {
            //Always set stream to the begin for re-read.
            stream.Seek(0, SeekOrigin.Begin);
        }
    }

    public async Task<IEnumerable<T>> ParseAsync<T>(Stream stream, params Expression<Func<T, object>>[] excludeFromParsing)
    {
        var reader = new StreamReader(stream, leaveOpen: true);
        
        var headerLine = await reader.ReadLineAsync()
            ?? throw new InvalidOperationException("File header cannot be empty!");
        
        var excludedPropNames = GetNames(excludeFromParsing);
        var indexesFromType = MapIndexesFromType<T>(headerLine, excludedPropNames);
        List<T> parsedItems = []; 
        //starts at 2 because of header.
        var lineNumber = 2;
        while(!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync()
                ?? throw new InvalidOperationException($"Csv line {lineNumber} is empty.");

            var parsed = ParseItem<T>(indexesFromType, line, lineNumber);
            parsedItems.Add(parsed);
            lineNumber++;
        }

        return parsedItems;
    }

    /// <summary>
    /// It finds the property name on given header line and maps the index
    /// With this technique it is possible to access the values by position.
    /// </summary>
    private static Dictionary<string, int> MapIndexesFromType<T>(string headerLine, IEnumerable<string>? excludedPropNames = null)
    {
        var headerFields = headerLine.Split(",", StringSplitOptions.TrimEntries);
        var givenType = typeof(T);
        var headerIndexes = new Dictionary<string, int>();
        
        foreach(var prop in givenType.GetProperties())
        {
            if(excludedPropNames is not null && excludedPropNames.Contains(prop.Name))
            {
                continue;
            }

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

    private static T ParseItem<T>(Dictionary<string, int> indexValuePairs, string csvLine, int lineNumber)
    {
        var fields = csvLine?.Split(",", StringSplitOptions.TrimEntries);

        if (fields is null || fields.Length == 0)
        {
            throw new InvalidOperationException($"Csv line {lineNumber} is empty.");
        }

        var propValuesMap = new Dictionary<string, object?>();

        foreach (var indexValuePair in indexValuePairs)
        {
            var value = fields.ElementAtOrDefault(indexValuePair.Value)
                ?? throw new InvalidOperationException($"Csv line {lineNumber} is bad formatted, could not find {indexValuePair.Key} value.");

            propValuesMap.Add(indexValuePair.Key, value);
        }

        var item = Activator.CreateInstance<T>();

        foreach (var prop in typeof(T).GetProperties())
        {
            //We skip values not present in dictionary, because they were excluded.
            if(!propValuesMap.ContainsKey(prop.Name))
            {
                continue;
            }

            //Since we use Split, everything is a string already, so we only need convert types like int double and so on...
            var value = prop.PropertyType != typeof(string) ?
                Convert.ChangeType(propValuesMap[prop.Name], prop.PropertyType) :
                propValuesMap[prop.Name];

            prop.SetValue(item, value);
        }

        return item;
    }

    private static IEnumerable<string> GetNames<T>(Expression<Func<T, object>>[] expressions)
    {
        foreach(var expression in expressions)
        {
            if(expression.Body is MemberExpression memberExpression)
            {
                yield return memberExpression.Member.Name;
            }
            else if(expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExpressionFromOperand)
            {
                yield return memberExpressionFromOperand.Member.Name;
            }
            else
            {
                throw new ArgumentException("The expression must be a simple member expression, e.g p => p.Name, p => p.Age, p => p.Gender");
            }
        }
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

