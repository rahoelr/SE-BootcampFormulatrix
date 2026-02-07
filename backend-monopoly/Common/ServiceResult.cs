namespace MonopolyBackend.Common
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public ServiceError? Error { get; }

        private ServiceResult(T data)
        {
            IsSuccess = true;
            Data = data;
        }

        private ServiceResult(ServiceError error)
        {
            IsSuccess = false;
            Error = error;
        }

        public static ServiceResult<T> Success(T data) => new(data);
        public static ServiceResult<T> Fail(ServiceError error) => new(error);
    }

}