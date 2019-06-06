using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using OxyPlot;
using OxyPlot.Series;

namespace CleanAnalysis
{
    public class StableAbstractionsPlotter
    {

        public void Draw(Dictionary<Project, Metrics> metricsDictionary)
        {
            var plotModel = new PlotModel();
            var scatterSeries = new ScatterSeries();
            plotModel.Series.Add(scatterSeries);

            foreach(var project in metricsDictionary.Keys)
            {
                var metrics = metricsDictionary[project];

                scatterSeries.Points.Add(new ScatterPoint(metrics.Stability.Coefficient, metrics.Abstractness.Coefficient, tag: project.Name));
            }

            using (var stream = File.Create($"StableAbstractions - {DateTime.Now.ToString("yyyyMMddTHHmm")}.pdf"))
            {
                var pdfExporter = new PdfExporter { Width = 800, Height = 800 };
                pdfExporter.Export(plotModel, stream);
            }
        }

    }
}
