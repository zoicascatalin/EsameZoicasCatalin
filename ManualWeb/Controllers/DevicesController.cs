using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManualWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ManualWeb.Controllers
{
    public class DevicesController : Controller
    {
        public class obj
        {
            public string img { get; set; }
        }
        private IConfiguration _configuration;

        public DevicesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var rm = RegistryManager.CreateFromConnectionString(_configuration["IoTHubConnectionString"]);

            var devicesQuery = rm.CreateQuery("SELECT DeviceId FROM devices");
            var devices = await devicesQuery.GetNextAsJsonAsync();

            var list = new List<DeviceListDto>();

            foreach (var device in devices)
            {
                var json = JsonConvert.DeserializeObject<JObject>(device);
                var dto = new DeviceListDto
                {
                    DeviceId = json.Value<string>("DeviceId")
                };
                list.Add(dto);
            }

            return View(list);
        }

        public async Task<IActionResult> Details(string id)
        {
            var dto = new ManualDTO();
            dto.DeviceId = id;

            var rm = RegistryManager.CreateFromConnectionString(_configuration["IoTHubConnectionString"]);

            return View(dto);
        }

        public async Task<JsonResult> doFoto(string id)
        {
            var rm = RegistryManager.CreateFromConnectionString(_configuration["IoTHubConnectionString"]);
            var bytes = Encoding.UTF8.GetBytes("scattafoto");
            var message = new Microsoft.Azure.Devices.Message(bytes);
            var deviceId = _configuration["deviceId"];
            var authenticationMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, _configuration["deviceKey"]);
            var transportType = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
            if (!string.IsNullOrWhiteSpace(_configuration["transportType"]))
            {
                transportType = (Microsoft.Azure.Devices.Client.TransportType)Enum.Parse(typeof(Microsoft.Azure.Devices.Client.TransportType), _configuration["transportType"], true);
            }
            var client = DeviceClient.Create(_configuration["hostName"], authenticationMethod, transportType);
            var serviceClient =
                ServiceClient.CreateFromConnectionString(
                    _configuration["IoTHubConnectionString"]);
            var x = new obj();
            try
            {
                await serviceClient.SendAsync(id, message);
                var twin = await client.GetTwinAsync();
                while (true)
                {
                    var msg = await client.ReceiveAsync();
                    if (msg == null) continue;

                    var bts = msg.GetBytes();
                    if (bts == null) continue;

                    var text = Encoding.UTF8.GetString(bts);
                    x.img = text;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Scrivi: scattafoto");
            }
            return Json(x);
        }

        public async Task addDose(string id)
        {
            var rm = RegistryManager.CreateFromConnectionString(_configuration["IoTHubConnectionString"]);
            var bytes = Encoding.UTF8.GetBytes("apridosatore");
            var message = new Microsoft.Azure.Devices.Message(bytes);
            var serviceClient =
                ServiceClient.CreateFromConnectionString(
                    _configuration["IoTHubConnectionString"]);
            try
            {
                await serviceClient.SendAsync(id, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Scrivi: apridosatore");
            }
        }
    }
}
