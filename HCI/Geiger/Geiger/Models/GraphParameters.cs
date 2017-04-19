using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geiger.Models
{
    public class GraphParameters
    {
        [Display(Name="Curve Name")]
        public string CurveName { get; set; }

        [Display(Name="Media Permittivity")]
        public double? MediaPermittivity { get; set; }
        [Display(Name = "Media Conductivity")]
        public double? MediaConductivity { get; set; }

        [Display(Name = "Membrane Permittivity")]
        public double? MembranePermittivity { get; set; }
        [Display(Name = "Membrane Conductivity")]
        public double? MembraneConductivity { get; set; }

        [Display(Name = "Cytoplasm Permittivity")]
        public double? CytoplasmPermittivity { get; set; }
        [Display(Name = "Cytoplasm Conductivity")]
        public double? CytoplasmConductivity { get; set; }

        [Display(Name="Cell Radius")]
        public double? CellRadius { get; set; }

        [Display(Name = "Shell Thickness")]
        public double? ShellThickness { get; set; }
    }
}