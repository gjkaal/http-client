namespace N2.Http
{
    public class BaseResult
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public ResponseCode ResponseCode { get; set; }
    }

}
