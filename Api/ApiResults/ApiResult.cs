namespace Api.ApiResults
{
    
    public class ApiResult<T>:ApiResultBase, IApiResult<T>
    {
        public T? Data { get; set; }
        public ApiResult(): base()
        { }
        public static ApiResult<T> Ok(T data)
        {
            return new ApiResult<T>() { Data = data };
        }
    }

    public class ApiResult : ApiResult<object>
    {

    }
}


