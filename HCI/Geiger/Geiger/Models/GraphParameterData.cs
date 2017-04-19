using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geiger.Models
{
    public class GraphParameterData
    {
        [Display(Name = "Frequency Start")]
        [Range(0, 1e8, ErrorMessage = "Frequency start range must be between {1} and {2} hz.")]
        public double FrequencyStartHz { get; set; }

        [Display(Name = "Frequency End")]
        [Range(100, 10e9, ErrorMessage = "Frequency end range must be between {1} and {2} hz.")]
        public double FrequencyEndHz { get; set; }

        public GraphParameters Parameters1 { get; set; }
        public GraphParameters Parameters2 { get; set; }
    }
}