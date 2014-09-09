﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models;
using UserInterface.Views;
using System.Reflection;
using Models.Core;
using System.Data;
using System.IO;
using UserInterface.Interfaces;
using UserInterface.EventArguments;

namespace UserInterface.Presenters
{
    class ReportPresenter : IPresenter
    {
        private Report Report;
        private IReportView View;
        private ExplorerPresenter ExplorerPresenter;
        private DataStore DataStore;

        /// <summary>
        /// Attach the model (report) and the view (IReportView)
        /// </summary>
        public void Attach(object Model, object View, ExplorerPresenter explorerPresenter)
        {
            this.Report = Model as Report;
            this.ExplorerPresenter = explorerPresenter;
            this.View = View as IReportView;

            this.View.VariableList.Lines = Report.VariableNames;
            this.View.EventList.Lines = Report.EventNames;
            this.View.VariableList.ContextItemsNeeded += OnNeedVariableNames;
            this.View.EventList.ContextItemsNeeded += OnNeedEventNames;
            this.View.VariableList.TextHasChangedByUser += OnVariableNamesChanged;
            this.View.EventList.TextHasChangedByUser += OnEventNamesChanged;
            ExplorerPresenter.CommandHistory.ModelChanged += CommandHistory_ModelChanged;

            Simulation simulation = Report.ParentOfType(typeof(Simulation)) as Simulation;
            DataStore = new DataStore(Report);

            PopulateDataGrid();
        }

        /// <summary>
        /// Detach the model from the view.
        /// </summary>
        public void Detach()
        {
            this.View.VariableList.ContextItemsNeeded -= OnNeedVariableNames;
            this.View.EventList.ContextItemsNeeded -= OnNeedEventNames;
            this.View.VariableList.TextHasChangedByUser -= OnVariableNamesChanged;
            this.View.EventList.TextHasChangedByUser -= OnEventNamesChanged;
            ExplorerPresenter.CommandHistory.ModelChanged -= CommandHistory_ModelChanged;
            DataStore.Disconnect();
        }

        /// <summary>
        /// The view is asking for variable names.
        /// </summary>
        void OnNeedVariableNames(object Sender, NeedContextItemsArgs e)
        {
            if (e.ObjectName == "")
                e.ObjectName = ".";
            object o = Report.Get(e.ObjectName);

            if (o != null)
            {
                foreach (IVariable Property in ModelFunctions.FieldsAndProperties(o, BindingFlags.Instance | BindingFlags.Public))
                {
                    e.Items.Add(Property.Name);
                }
                e.Items.Sort();
            }
        }

        /// <summary>
        /// The view is asking for event names.
        /// </summary>
        void OnNeedEventNames(object Sender, NeedContextItemsArgs e)
        {
            object o = Report.Get(e.ObjectName);

            if (o != null)
            {
                foreach (EventInfo Event in o.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public))
                    e.Items.Add(Event.Name);
            }
        }

        /// <summary>
        /// The variable names have changed in the view.
        /// </summary>
        void OnVariableNamesChanged(object sender, EventArgs e)
        {
            ExplorerPresenter.CommandHistory.ModelChanged -= new CommandHistory.ModelChangedDelegate(CommandHistory_ModelChanged);
            ExplorerPresenter.CommandHistory.Add(new Commands.ChangeProperty(Report, "VariableNames", View.VariableList.Lines));
            ExplorerPresenter.CommandHistory.ModelChanged += new CommandHistory.ModelChangedDelegate(CommandHistory_ModelChanged);
        }

        /// <summary>
        /// The event names have changed in the view.
        /// </summary>
        void OnEventNamesChanged(object sender, EventArgs e)
        {
            ExplorerPresenter.CommandHistory.ModelChanged -= new CommandHistory.ModelChangedDelegate(CommandHistory_ModelChanged);
            ExplorerPresenter.CommandHistory.Add(new Commands.ChangeProperty(Report, "EventNames", View.EventList.Lines));
            ExplorerPresenter.CommandHistory.ModelChanged += new CommandHistory.ModelChangedDelegate(CommandHistory_ModelChanged);
        }

        /// <summary>
        /// The model has changed so update our view.
        /// </summary>
        void CommandHistory_ModelChanged(object changedModel)
        {
            if (changedModel == Report)
            {
                View.VariableList.Lines = Report.VariableNames;
                View.EventList.Lines = Report.EventNames;
            }
        }

        /// <summary>
        /// Populate the data grid.
        /// </summary>
        private void PopulateDataGrid()
        {
            Simulation simulation = Report.ParentOfType(typeof(Simulation)) as Simulation;
            
            View.DataGrid.DataSource = DataStore.GetData(simulation.Name, Report.Name);

            if (View.DataGrid.DataSource != null)
            {
                // Make all numeric columns have a format of N3
                foreach (DataColumn col in View.DataGrid.DataSource.Columns)
                {
                    IGridColumn gridColumn = this.View.DataGrid.GetColumn(col.Ordinal);
                    gridColumn.LeftAlignment = false;
                    if (col.DataType == typeof(double))
                        gridColumn.Format = "N3";
                }
            }
        }
    }
}