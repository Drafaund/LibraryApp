namespace LibraryApp.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, string.Empty);

    public static Result Fail(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value {get;}

    private Result(bool isSuccess, T value, string error) : base(isSuccess, error)
    {
        Value = value;
    }

    //gak butuh new soalnya overload sehingga pas compile gak ada potensi bentrok
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    //butuh new soalnya ini sama persis dgn parent agar compiler gak bingung n gak ada potensi bentrok
    public new static Result<T> Fail(string error) => new(false, default!, error); 
}

