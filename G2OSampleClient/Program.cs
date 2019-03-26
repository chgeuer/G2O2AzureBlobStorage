namespace G2OSampleClient
{
    using G2O;
    using G2OAzureStorage;
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using WhatIsMyIP;

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "G2O Client";
            Console.Write("Press <return>"); Console.ReadLine();

            Func<string,string> getCfg = _ => ConfigurationManager.AppSettings[_];
            var config = new G2OProxyConfiguration() { GetConfiguration = getCfg };

            var version = 3;
            var nonce = config.Keys.First().Key;
            var g2oClientHandler = new G2OHttpClientHandler(
                version: version,
                edgeIP: ExternalIPFetcher.GetAddress().IPAddress.ToString(),
                clientIP: "1.2.3.4", // 
                nonce: nonce,
                nonceValue: config.Keys[nonce]);

            var files = new[] 
            { 
                // http://127.0.0.1:10000/private/hello-akamai.jpg
                "localdevstorage/private/hello-akamai.jpg",

                // https://cdndatastore01.blob.core.windows.net/private1/someLockedDownImage.jpg
                "cdndatastore01/private1/someLockedDownImage.jpg",

                // https://cdndatastore01.blob.core.windows.net/public/data/somePublicImage.jpg
                "cdndatastore01/public/data/somePublicImage.jpg",
            };

            files.ToList().ForEach(file =>
            {
                var localSelfHost = "localhost:82";
                var localWorkerRole = "localhost:81";

                var url = "http://" + localWorkerRole + "/" + file;

                var response = new HttpClient(g2oClientHandler).SendAsync(new HttpRequestMessage(HttpMethod.Get, url)).Result;

                Console.WriteLine(response.StatusCode + " " + url + " " + response.Content.Headers.ContentLength);

                // Console.WriteLine(response.Content.ReadAsStringAsync().Result);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Console.WriteLine("{0} has length {1} ({2})", file, response.Content.ReadAsByteArrayAsync().Result.Length, url);
                }
                else
                {
                    // Console.WriteLine("{0} was {1} ({2})", file, response.StatusCode, url);
                }
            });

            Console.ReadLine();
        }
    }
}