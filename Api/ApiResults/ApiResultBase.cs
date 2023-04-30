namespace Api.ApiResults
{
    public abstract class ApiResultBase
    {
        public string? Code { get; set; }
        public bool Success { get; set; }
        public IEnumerable<ValidationError> Errors { get; set; }
        public string? Message { get; set; }
        public ApiResultBase()
        {
            Success = true;
            Errors = new List<ValidationError>();
        }
        public static ApiResult Error(string message = "Oops! Something went wrong. Please try again later.")
        {
            return new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.Error,
                Message = message
            };
        }
        public static ApiResult InvalidJson(string message = "The request body could not be decoded as JSON")
        {
            return new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.InvalidJson,
                Message = message
            };
        }
        public static ApiResult InvalidRequest(string message = "This request if not supported.")
        {
            return new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.InvalidRequest,
                Message = message
            };
        }

        public static ApiResult Unauthorized(string message = "Access is denied due to invalid credential.")
        {
            return new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.Unauthorized,
                Message = message
            };
        }
        public static ApiResult ValidationError(string message = "Your input data is invalid.", List<ValidationError>? errors = null)
        {
            var result = new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.ValidationError,
                Message = message
            };
            if (errors != null)
            {
                result.Errors = errors;
            }
            return result;
        }
        public static ApiResult ValidationError(List<ValidationError>? errors)
        {
            var result = new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.ValidationError,
                Message = "Your input data is invalid."
            };
            if (errors != null)
            {
                result.Errors = errors;
            }
            return result;
        }
        public static ApiResult RestrictedResource(string message = "Access Denied! Please contact your administrator to request access.")
        {
            return new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.RestrictedResource,
                Message = message
            };
        }
        public static ApiResult NotFound(string message = "Not Found.")
        {
            return new ApiResult
            {
                Success = false,
                Code = ApiResultCodes.ObjectNotFound,
                Message = message
            };
        }
    }
}
