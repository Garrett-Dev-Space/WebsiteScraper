public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        this.response = response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(response);
    }
}