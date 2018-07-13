using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ManualConnectedCiotola
{
    static class Program
    {
        private static int dosatore = 0;
        private static string image = "";
        static async Task Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var deviceId = configuration["deviceId"];
            var authenticationMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, configuration["deviceKey"]);
            var transportType = TransportType.Mqtt;
            if (!string.IsNullOrWhiteSpace(configuration["transportType"]))
            {
                transportType = (TransportType)Enum.Parse(typeof(TransportType), configuration["transportType"], true);
            }
            var client = DeviceClient.Create(configuration["hostName"], authenticationMethod, transportType);
            var connectionString = configuration["StorageConnectionString"];

           
            var twin = await client.GetTwinAsync();

            while (true)
            {
                var message = await client.ReceiveAsync();
                if (message == null) continue;

                var bytes = message.GetBytes();
                if (bytes == null) continue;

                var text = Encoding.UTF8.GetString(bytes);
                switch (text)
                {
                    case "apridosatore":
                        await apriDosatore(client);
                        break;
                    case "scattafoto":
                        await scattaFoto(client);
                        break;
                    default:
                        Console.WriteLine("Scrivi bene!");
                        break;
                }
                await client.CompleteAsync(message);
            }
        }

        private static async Task scattaFoto(DeviceClient client)
        {
            image = "https://esamecatalinstorage.file.core.windows.net/ciotola/ciotola.jpg";
            var coll = new TwinCollection();
            coll["image"] = image;
            await client.UpdateReportedPropertiesAsync(coll);
        }


        private static async Task apriDosatore(DeviceClient client)
        {
            dosatore++;
            var coll = new TwinCollection();
            coll["dosatore"] = dosatore;
            await client.UpdateReportedPropertiesAsync(coll);
        }

    }
}
