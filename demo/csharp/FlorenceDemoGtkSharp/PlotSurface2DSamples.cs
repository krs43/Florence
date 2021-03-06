﻿/*
 * Florence - A charting library for .NET
 * 
 * PlotSurface2DSamples.cs
 * Copyright (C) 2003-2006 Matt Howlett and others.
 * Copyright (C) 2003-2013 Hywel Thomas
 * Copyright (C) 2013 Scott Stephens
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of Florence nor the names of its contributors may
 *    be used to endorse or promote products derived from this software without
 *    specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Data;
using System.IO;
using System.Reflection;

using Gtk;
using Florence;
using Florence.GtkSharp;
using DemoLib.PlotSurface2DDemo;

namespace FlorenceDemoGtkSharp
{
    /// <summary>
    /// The main demo window.
    /// </summary>
    public class PlotSurface2DDemo : Gtk.Window
    {
        private int currentPlot = 0;
        private int id = 1;

        private Type[] sampleTypes;
        private Type currentType;
        private IDemo currentSample;
        private InteractivePlotSurface2D plotSurface;

        private Gtk.Table layout;
        private PlotWidget plotWidget;
        private Gtk.TextView infoBox;
        private Gtk.Frame infoFrame;

        private Gtk.Button quitButton;
        private Gtk.Button nextPlotButton;
        private Gtk.Button prevPlotButton;
        private Gtk.Button printButton;
        private PrintDocument printDocument;

        private Gtk.Label exampleNumberLabel;

        /// <summary>
        /// Initializes a new instance of the NPlotDemo.PlotSurface2DDemo class.
        /// </summary>
        public PlotSurface2DDemo()
            : base("NPlot Gtk.InteractivePlotSurface2D Demo")
        {
            // Initialise Gtk# form
            InitializeComponent();

            // Define array of PlotSamples classes
            sampleTypes = new Type[]
			{
				typeof (PlotWave),
				typeof (PlotDataSet),
				typeof (PlotMockup),
				typeof (PlotImage),
				typeof (PlotQE),
				typeof (PlotMarkers),
				typeof (PlotLogAxis),
				typeof (PlotLogLog),
				typeof (PlotParticles), 
				typeof (PlotWavelet), 
				typeof (PlotSincFunction), 
				typeof (PlotGaussian),
				typeof (PlotLabelAxis),
				typeof (PlotCircular),
				typeof (PlotCandle),
				typeof (PlotABC)
			};

            plotSurface = new InteractivePlotSurface2D();
            plotWidget.InteractivePlotSurface2D = plotSurface;

            // set up printer
            printDocument = new PrintDocument();
            printDocument.PrintPage += new PrintPageEventHandler(pd_PrintPage);

            // draw the first plot sample
            currentPlot = 0;
            
            ShowSample(currentPlot);
        }

        #region Gtk# form initialisation

        private void InitializeComponent()
        {
            quitButton = new Gtk.Button();
            nextPlotButton = new Gtk.Button();
            prevPlotButton = new Gtk.Button();
            printButton = new Gtk.Button();
            exampleNumberLabel = new Gtk.Label();

            // Create the two display panes for the samples
            plotWidget = new PlotWidget();
            infoBox = new Gtk.TextView();

            quitButton.Name = "quitButton";
            //quitButton.TabIndex = 14;
            quitButton.Label = "Close";
            quitButton.Clicked += new System.EventHandler(quitButton_Click);

            nextPlotButton.Name = "nextPlotButton";
            //nextPlotButton.TabIndex = 17;
            nextPlotButton.Label = "Next";
            nextPlotButton.Clicked += new System.EventHandler(nextPlotButton_Click);

            printButton.Name = "printButton";
            //printButton.TabIndex = 9;
            printButton.Label = "Print";
            printButton.Clicked += new System.EventHandler(printButton_Click);

            prevPlotButton.Name = "prevPlotButton";
            //prevPlotButton.TabIndex = 15;
            prevPlotButton.Label = "Prev";
            prevPlotButton.Clicked += new System.EventHandler(prevPlotButton_Click);

            exampleNumberLabel.Name = "exampleNumberLabel";

            infoBox.Name = "infoBox";
            //infoBox.TabIndex = 18;

            SetSizeRequest(632, 520);
            //
            // Define 11x8 table on which to lay out the plot, test buttons, etc
            //
            layout = new Gtk.Table(11, 8, true);
            layout.BorderWidth = 4;
            Add(layout);

            infoFrame = new Frame();
            infoFrame.ShadowType = Gtk.ShadowType.In;
            infoFrame.Add(infoBox);

            AttachOptions opt = AttachOptions.Expand | AttachOptions.Fill;
            uint xpad = 2, ypad = 10;

            layout.Attach(infoFrame, 0, 8, 9, 11);
            layout.Attach(plotWidget, 0, 8, 0, 8);
            layout.Attach(quitButton, 3, 4, 8, 9, opt, opt, xpad, ypad);
            layout.Attach(printButton, 2, 3, 8, 9, opt, opt, xpad, ypad);
            layout.Attach(prevPlotButton, 0, 1, 8, 9, opt, opt, xpad, ypad);
            layout.Attach(exampleNumberLabel, 4, 5, 8, 9);
            layout.Attach(nextPlotButton, 1, 2, 8, 9, opt, opt, xpad, ypad);
            Name = "PlotSurface Samples";

        }
        #endregion

        /// <summary>
        /// Creates and shows samplePlot [index]
        /// </summary>
        private void ShowSample(int index)
        {
            if (currentSample != null)
                currentSample.Cleanup();

            currentPlot = index;
            currentType = sampleTypes[index];
            currentSample = (IDemo)Activator.CreateInstance(currentType);
            currentSample.CreatePlot(plotSurface);

            infoBox.Buffer.Text = String.Join("\n", currentSample.Description);	// update info Text

            int id = currentPlot + 1;
            exampleNumberLabel.Text = "Plot " + id.ToString("0") + "/" + sampleTypes.Length.ToString("0");

            ShowAll();
        }

        // The PrintPage event is raised for each page to be printed.
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            // Needs checking for Gtk# implementation - no-op for now
            // plotSurface.Draw( ev.Graphics, ev.MarginBounds );
            // ev.HasMorePages = false;
        }

        /// <summary>
        /// callback for quit button click
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void quitButton_Click(object sender, System.EventArgs e)
        {
            Destroy();
        }

        /// <summary>
        /// callback for next button click
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void nextPlotButton_Click(object sender, System.EventArgs e)
        {
            ShowSample( (currentPlot+1) % sampleTypes.Length);
        }

        /// <summary>
        /// Callback for prev button click.
        /// </summary>
        private void prevPlotButton_Click(object sender, System.EventArgs e)
        {
            ShowSample( ((currentPlot-1) + 16) % sampleTypes.Length);
        }

        /// <summary>
        /// callback for print button click
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void printButton_Click(object sender, System.EventArgs e)
        {
            /*
            PrintDialog dlg = new PrintDialog();
            dlg.Document = printDocument;
            if (dlg.ShowDialog() == DialogResult.OK) 
            {
                try
                {
                    printDocument.Print();
                }
                catch
                {
                    Console.WriteLine( "problem printing.\n" );
                }
            }
            */
        }


    } // class Gtk.PlotSurface2DSamples

} // namespace NPlotDemo
