using NADAAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.ViewModels.EndPointsViewModel
{
    public class EndPointsViewModel
    {

        [Display(Name = "EndPoint")]
        public List<EndPoint> EndPoints { get; set; }

        [Display(Name = "EndPointId")]
        public Guid EndPointId { get; set; }

        [Display(Name = "EndPointUri")]
        public string EndPointUri { get; set; }
    }
}
