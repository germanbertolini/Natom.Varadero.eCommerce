using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models.ViewModels
{
    public class reqUploadResource
    {
        [JsonProperty("base64")]
        public string Base64 { get; set; }
    }
}