using Microsoft.Extensions.Options;
using Pretech.Ihale.Mobile.Gateway.Settings;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Pretech.Ihale.Mobile.Gateway.DelegateHandlers
{
    public class FailoverDelegateHandler : DelegatingHandler
    {
        private readonly LBSettings _envanters;
        public FailoverDelegateHandler(IOptions<LBSettings> envanters)
        {
            _envanters = envanters?.Value;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                CancellationTokenSource requestCancellation = new CancellationTokenSource(_envanters.HttpRequestSettings.RequestTimeout);
                try
                {

                    var response = await base.SendAsync(request, requestCancellation.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                    else
                    {
                        var changeState = RequestChange(request, new UriBuilder(request.RequestUri));
                        if (!changeState)
                        {
                            return response;
                        }
                        else continue;
                    }
                }
                catch (Exception ex)
                {
                    if ((ex.InnerException is SocketException) | (requestCancellation.IsCancellationRequested))
                    {
                        RequestChange(request, new UriBuilder(request.RequestUri));
                    }
                }
            }
            return await base.SendAsync(request, cancellationToken); ;

        }
        private bool RequestChange(HttpRequestMessage request, UriBuilder errorHost)
        {
            var firstHost = _envanters.Envanters.SkipWhile(q => q.Host == errorHost.Host).FirstOrDefault();
            if (firstHost == null) return false;
            else
            {
                errorHost.Host = firstHost.Host;
                errorHost.Port = firstHost.Port;
                request.RequestUri = errorHost.Uri;
                return true;
            }
        }
    }
}
