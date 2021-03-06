# Jens.InversionOfControl
This is a simple IoCContainer max depth is one, no life cycle handling (not planned). Next milestone will be Interception capability for interfaces. Dependencies will be auto solved always starting with the largest constructor (arguments) if no suitable constructor is found an exception is thrown. For a code sample please take a look into the Jens.InversionOfControl.Test. Happy Coding.


# Jens.RestServer

This started as a side project of my MagicMirror, as I wanted to have a lightweight restful webserver to handle my configuration web page.

This would be the logic for the server to startup including a file route handler. However you are able to create own file systems if you want to, just make use of the provided interfaces. 
```C#
class Program
{
    private static IOContainer _container;
    private static RestServer _restServer;

    static void Main(string[] args)
    {
        Log.DefaultLogger = new ConsoleLogger();
        _container = new IOContainer();
        var config = _container.GetDependency(typeof(Configuration)) as Configuration;
        _restServer = new RestServer(new IPEndPoint(config.IPAddress, config.Port), _container,
            System.Reflection.Assembly.GetExecutingAssembly());
        _restServer.RegisterRouteHandler(new RestServerServiceFileRouteHandler("InetPub"));


        _restServer.Start();

        while (_restServer.IsRunning)
        {
            System.Threading.Thread.Sleep(10);
        }
    }
}
```

And here you have a Sample Rest Api Service. You are able to defince the instance type for your service class as singleton lazy, singelton strict or instance. Also dependency injection is possible via contructor. The base class always contains the current httpcontext when a api call is executed. This is all pretty straight forward. 
```C#
internal class SampleApiFullBodyResponse
{
    public string Body { get; set; }
}
internal class WildcardTest
{
    public int WildcardInt { get; set; }
    public string WildcardString { get; set; }
}

[RestServerServiceInstance(RestServerServiceInstanceType.Instance)]
internal class SampleApiService : RestServerService
{
    private readonly Configuration _config;

    public SampleApiService(Configuration configuration)
    {
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [RestServerServiceCall("/sampleapi/getconfiguration")]
    public Configuration GetConfiguration()
    {
        return _config;
    }

    [RestServerServiceCall("/sampleapi/fullbody")]
    public SampleApiFullBodyResponse FullBody(string body)
    {
        return new SampleApiFullBodyResponse() { Body = body };
    }

    [RestServerServiceCall("/sampleapi/wildcard/{WildcardInt}/{WildcardString}")]
    public WildcardTest FullBody(WildcardTest wildcardTest)
    {
        return wildcardTest;
    }
}
```
Wildcards are used to fill properties of the input object directly from url. 
