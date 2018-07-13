using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ManualWeb.Models
{
    public class ManualDTO
    {
        public string DeviceId { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public int? dose { get; set; }

    }
}
