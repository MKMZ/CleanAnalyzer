using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CleanAnalysis
{
    public class StableAbstractionsPlotter
    {

        public void Draw(ImmutableDictionary<Project, Metrics> metricsDictionary, string solutionName)
        {
            var plotModel = new PlotModel();
            var scatterSeries = new ScatterSeries {
                Tag = "{Tag}"
            };
            var mainSequenceSeries = new FunctionSeries(x => x, 0, 1, 0.00001, "The Main Sequence");
            var upperBoundary = new FunctionSeries(x => x + 0.5, 0, 0.5, 0.00001, "Zone Of Useless Boundary");
            var lowerBoundary = new FunctionSeries(x => x - 0.5, 0.5, 1, 0.00001, "Zone Of Pain Boundary");
            
            plotModel.Series.Add(scatterSeries);
            plotModel.Series.Add(mainSequenceSeries);
            plotModel.Series.Add(upperBoundary);
            plotModel.Series.Add(lowerBoundary);

            var xAxis = new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Maximum = 1,
                Minimum = 0,
                Title = "Stability"
            };
            var yAxis = new LinearAxis
            {
                Key = "Y",
                Position = AxisPosition.Left,
                Maximum = 1,
                Minimum = 0,
                Title = "Abstractness"
            };
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            plotModel.Title = $"{solutionName} - {DateTime.UtcNow}";

            foreach (var project in metricsDictionary.Keys)
            {
                var metrics = metricsDictionary[project];

                scatterSeries.Points.Add(new ScatterPoint(metrics.Stability.Coefficient, metrics.Abstractness.Coefficient, tag: project.Name));
            }

            using (var stream = File.Create($"{solutionName} - {DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}.pdf"))
            {
                var pdfExporter = new PdfExporter { Width = 800, Height = 800 };
                pdfExporter.Export(plotModel, stream);
            }
        }

    }
}
