namespace CorsoApi.Infrastructure;

public class CsvParser
{
    internal IEnumerable<T> Parse<T>(Stream stream)
    {
        throw new NotImplementedException();
    }

    internal CsvValidationResult Validate<T>(Stream stream)
    {
        return new CsvValidationResult
        {
            Succeeded = false,
            ErrorMessages =
            [
                "error1",
                "error2"
            ]
        };
    }

    public class CsvValidationResult
    {
        public bool Succeeded { get; internal set; }
        public IEnumerable<string>? ErrorMessages { get; internal set; }
    }
}

