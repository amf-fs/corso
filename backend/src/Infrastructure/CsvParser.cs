
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

    internal async Task<T> ParseAsync<T>(Stream stream)
    {
        throw new NotImplementedException();
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

