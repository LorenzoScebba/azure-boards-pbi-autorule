namespace azure_boards_pbi_autorule.Models
{
    public class Result<T, TE>
    {
        public bool HasError { get; set; }
        public T Data { get; set; }
        public TE Error { get; set; }

        public static Result<T, TE> Ok(T data)
        {
            return new Result<T, TE>
            {
                Data = data,
                Error = default,
                HasError = false
            };
        }

        public static Result<T, TE> Fail(TE error)
        {
            return new Result<T, TE>
            {
                Data = default,
                Error = error,
                HasError = true
            };
        }
    }
}