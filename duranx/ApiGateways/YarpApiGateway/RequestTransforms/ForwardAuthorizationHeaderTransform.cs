using Yarp.ReverseProxy.Transforms;

namespace YarpApiGateway.RequestTransforms
{
    public class ForwardAuthorizationHeaderTransform : RequestTransform
    {
        public override async ValueTask ApplyAsync(RequestTransformContext context)
        {
            //if (context.ProxyRequest.Headers.TryGetValues("Authorization", out var values))
            //{
            //    context.ProxyRequest.Headers.Remove("Authorization");
            //    context.ProxyRequest.Headers.Add("Authorization", values.First());
            //}
            return;
        }
    }
}
