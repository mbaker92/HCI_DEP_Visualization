using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Web.Mvc;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Geiger.Models;

namespace Geiger.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Renders the primary page of the website - this is the default page when the user visits.
        /// </summary>
        /// <returns>Default home page.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// About this project / team.
        /// </summary>
        /// <returns>View displaying information about this project.</returns>
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Collects the graph parameters from the user, validates them, and then forwards the user to the graph page.
        /// </summary>
        /// <param name="data">Graph parameter data.</param>
        /// <returns>View to get graph parameters.</returns>
        public ActionResult GraphParameters(GraphParameterData data)
        {
            // If no data, or both parameter sets are null, give them an empty to fill.
            if (data == null ||
                (data.Parameters1 == null || data.Parameters2 == null))
            {
                // Clear errors for not entering any data
                this.ModelState.Clear();

                // Generate new model to fill data into & return
                var model = new GraphParameterData() { FrequencyStartHz = 1000, FrequencyEndHz = 10000000 };
                return View(model);
            }

            // TODO Validation of graphing parameters, add to ModelState errors if any exist
            List<GraphParameters> validParameters = new List<GraphParameters>();
            // Validate that one set of graph parameters are filled out.
            GraphParameters p = data.Parameters1;
            if (p.CellRadius != null || p.CytoplasmConductivity != null || p.CytoplasmPermittivity != null || p.MediaConductivity != null || p.MediaPermittivity != null || p.MembraneConductivity != null || p.MembranePermittivity != null || p.ShellThickness != null)
            {
                validParameters.Add(data.Parameters1);
            }

            p = data.Parameters2;
            if (p.CellRadius != null || p.CytoplasmConductivity != null || p.CytoplasmPermittivity != null || p.MediaConductivity != null || p.MediaPermittivity != null || p.MembraneConductivity != null || p.MembranePermittivity != null || p.ShellThickness != null)
            {
                validParameters.Add(data.Parameters2);
            }

            if (validParameters.Count == 0)
            {
                ModelState.AddModelError("", "Please fill in at least one set of graph parameters to continue.");
            }

            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var graphData = UpdateGraphData(validParameters, data.FrequencyStartHz, data.FrequencyEndHz);

            TempData["graphData"] = graphData;
            return RedirectToAction("Graph");
        }

        /// <summary>
        /// Shows a graph to the user based on the parameters collected.
        /// </summary>
        /// <param name="graphData">Graph data for display.</param>
        /// <returns>Graph view to user.</returns>
        public ActionResult Graph()
        {
            var graphData = TempData["graphData"];
            return View(graphData);
        }

        /// <summary>
        /// Constructs a GraphData object with a series for each set of parameters provided across the
        /// Frequency range provided with the given number of sample points. These sample points are 
        /// spaced logarithmically from eachother.
        /// </summary>
        private GraphData UpdateGraphData(IEnumerable<GraphParameters> parameters, double minFreq, double maxFreq, int numSamples = 5000)
        {
            var graphData = new GraphData {Chart = new Highcharts("DEP_Chart")};
            graphData.Chart.SetTitle(new Title() {Text = "DEP Separability"});
            graphData.Chart.InitChart(new Chart() {ZoomType = ZoomTypes.X});

            graphData.Chart.SetXAxis(new XAxis()
            {
                Type = AxisTypes.Logarithmic,
                TickInterval = 0.11,
                Title = new XAxisTitle() {Text = "Frequency (Hz)"}
            });

            graphData.Chart.SetYAxis(new YAxis()
            {
                Type = AxisTypes.Linear,
                Title = new YAxisTitle() {Text = "Real part of Fcm"}
            });

            try
            {
                var seriesList = new List<Series>();

                foreach (var curve in parameters)
                {
                    var points = new List<double[]>();
                    foreach (var point in Logspace(minFreq, maxFreq, numSamples))
                    {
                        points.Add(new[] { point, GetValueAtX(point, curve) });
                    }

                    var data = new Data(points.ToArray());
                    var series = new Series
                    {
                        Data = data,
                        Name = curve.CurveName,
                        Type = ChartTypes.Line
                    };

                    seriesList.Add(series);
                }
                

                graphData.Chart.SetSeries(seriesList.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (graphData.Chart == null)
            {
                throw new Exception("Something went wrong in GetGraphData.");
            }

            return graphData;
        }

        /// <summary>
        /// Finds the DEP force at the given frequency, using the given parameters.
        /// These calculations were adapted from Dr. Emil Geiger's MATLAB script.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private double GetValueAtX(double freq, GraphParameters parameters)
        {
            const double epsilon_0 = 8.8542e-12; //permitivity of free space
            var w = 2 * Math.PI * freq;

            var R = parameters.CellRadius ?? 0;
            var d = parameters.ShellThickness ?? 0;

            var epsilon_m = parameters.MediaPermittivity ?? 0;
            var sigma_m = parameters.MediaConductivity ?? 0;

            var epsilon_ps1 = parameters.MembranePermittivity ?? 0;
            var epsilon_pc1 = parameters.CytoplasmPermittivity ?? 0;

            var sigma_ps1 = parameters.MembraneConductivity ?? 0;
            var sigma_pc1 = parameters.CytoplasmConductivity ?? 0;

            var com_eps1 = epsilon_ps1 * epsilon_0 - new Complex(0, 1) * (sigma_ps1 / w); //complex permittivity membrane
            var com_epc1 = epsilon_pc1 * epsilon_0 - new Complex(0, 1) * (sigma_pc1 / w); //complex permittivity cytoplasm
            var com_medium = epsilon_m * epsilon_0 - new Complex(0, 1) * (sigma_m / w); //complex permittivity of medium

            //complex permittivity using single shell model
            var com_ep1 = com_eps1 *
                          (Math.Pow((R + d) / R, 3) + 2 * (com_epc1 - com_eps1) / (com_epc1 + 2 * com_eps1)) /
                            ((Math.Pow((R + d) / R, 3) - (com_epc1 - com_eps1) / (com_epc1 + 2 * com_eps1)));

            //clasius_mosotti factor
            var Fcm1 = (com_ep1 - com_medium) / (com_ep1 + 2 * com_medium);

            var ReFcm1 = Fcm1.Real;

            return ReFcm1;
        }

        /// <summary>
        /// Provides the frequency at which the graph is expected to intersect with the Y-Axis.
        /// This could be used to position the initial graph near the upper crosover frequency.
        /// These calculations were adapted from Dr. Emil Geiger's MATLAB script.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private double GetUpperCrossoverFreqEst(GraphParameters parameters)
        {
            const double epsilon_0 = 8.8542e-12; //permitivity of free space

            //TODO: do GraphParameters have to be nullable? We need all of the below to be not null.
            var epsilon_m = parameters.MediaPermittivity ?? 0;
            var sigma_m = parameters.MediaConductivity ?? 0;

            var epsilon_pc1 = parameters.CytoplasmPermittivity ?? 0;
            var sigma_pc1 = parameters.CytoplasmConductivity ?? 0;

            //calculate the upper crossover frequency estimate
            var Fxupper_p1_est = (1 / (2 * Math.PI)) *
                                 Math.Sqrt((Math.Pow(sigma_pc1, 2) - sigma_pc1 * sigma_m - 2 * Math.Pow(sigma_m, 2)) /
                                           ((2 * Math.Pow(epsilon_m, 2) - epsilon_pc1 * epsilon_m - Math.Pow(epsilon_pc1, 2)) *
                                            Math.Pow(epsilon_0, 2)));

            return Fxupper_p1_est;
        }

        /// <summary>
        /// Returns a list of values in logarithmic space that can be used to iterate logarithmically.
        /// Retrieved from: http://stackoverflow.com/a/16491073
        /// </summary>
        private IEnumerable<double> Logspace(double start, double end, int count)
        {
            double d = (double)count, p = end / start;
            return Enumerable.Range(0, count).Select(i => start * Math.Pow(p, i / d));
        }
    }
}