namespace TrueCode.SharedKernel.Results;

public class Result<T>
{
    public bool IsSuccess => Errors.Count == 0;

    public List<string> Errors { get; private set; } = new();

    public T? Data { get; set; }

    public void Merge(Result<T> result)
    {
        if (result.Data != null)
        {
            Data = result.Data;
        }

        Errors.AddRange(result.Errors);
    }

    public string GetErrorsString()
    {
        return string.Join(", ", Errors);
    }

    public Result<T> AddError(string err)
    {
        Errors.Add(err);
        return this;
    }
}
