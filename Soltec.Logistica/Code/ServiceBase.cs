namespace Soltec.Logistica.Service
{
    public interface IServiceBase 
    {
        public string baseUrl { get; set; }
        public string ApiKey { get; set; }
    }
    public class ServiceBase:IServiceBase
    {
        public string baseUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
