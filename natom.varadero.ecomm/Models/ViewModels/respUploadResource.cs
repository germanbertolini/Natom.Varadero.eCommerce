using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models.ViewModels
{
    public class respUploadResource
    {
        [JsonProperty("id")]
        public long? Id { get; set; }
        [JsonProperty("resource_url")]
        public string ResourceUrl { get; set; }
    }
}