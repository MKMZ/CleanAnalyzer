using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CleanAnalysis
{
    public class StableAbstractionsPlotter
    {

        public void Draw(ImmutableDictionary<Project, Metrics> metricsDictionary, string solutionName, double MainSequenceDistanceAllowance = 0.7)
        {
            var plotModel = new PlotModel();
            var scatterSeries = new ScatterSeries {
                Tag = "{Tag}",
                TextColor = OxyColor.FromRgb(0, 0, 255)
            };
            var mainSequenceSeries = new FunctionSeries(x => x, 0, 1, 0.00001, "The Main Sequence")
            {
                Color = OxyColor.FromRgb(0, 0, 0)
            };
            var upperBoundary = new FunctionSeries(x => x + MainSequenceDistanceAllowance, 0, 1 - MainSequenceDistanceAllowance, 0.00001)
            {
                Color = OxyColor.FromRgb(255, 0, 0)
            };
            var lowerBoundary = new FunctionSeries(x => x - MainSequenceDistanceAllowance, MainSequenceDistanceAllowance, 1, 0.00001)
            {
                Color = OxyColor.FromRgb(255, 0, 0)
            };

            plotModel.Series.Add(scatterSeries);
            plotModel.Series.Add(mainSequenceSeries);
            plotModel.Series.Add(upperBoundary);
            plotModel.Series.Add(lowerBoundary);

            plotModel.Annotations.Add(new TextAnnotation
            {
                Text = "Zone Of Useless Boundary",
                TextPosition = new DataPoint(0.1, 0.9)
            });
            plotModel.Annotations.Add(new TextAnnotation
            {
                Text = "Zone Of Pain Boundary",
                TextPosition = new DataPoint(0.9, 0.1)
            });

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
