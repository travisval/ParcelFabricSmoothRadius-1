﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;

namespace SmoothRadiusAddin
{

    /// <summary>
    /// A construction tool for ArcMap Editor, using shape constructors
    /// </summary>
    public partial class SmoothRadiusTool : ESRI.ArcGIS.Desktop.AddIns.Tool, IShapeConstructorTool, ISketchTool
    {
        private IEditor3 m_editor;
        private IEditEvents_Event m_editEvents;
        private IEditEvents5_Event m_editEvents5;
        private IEditSketch3 m_edSketch;
        private IShapeConstructor m_csc;

        private ICadastralEditor m_cadEd; 
        private ICadastralFabric m_cadFab; 
        private IFeatureClass m_fabricLines;
        private IFeatureClass m_fabricPoints; 



        public SmoothRadiusTool()
        {
            // Get the editor
            m_editor = ArcMap.Editor as IEditor3;
            m_editEvents = m_editor as IEditEvents_Event;
            m_editEvents5 = m_editor as IEditEvents5_Event;

            OnUpdate();
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null && m_editor != null && m_editor.EditState != esriEditState.esriStateNotEditing;
        }

        protected override void OnActivate()
        {
            m_cadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension"); 
            m_cadFab = m_cadEd.CadastralFabric; 
 
            if (m_cadFab == null) 
            { 
                MessageBox.Show("No target fabric or edit session found. Please add a fabric to the map, start editing, and try again."); 
                return; 
            }
            m_fabricLines = (IFeatureClass)m_cadFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
            m_fabricPoints = (IFeatureClass)m_cadFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
            
            m_edSketch = m_editor as IEditSketch3;
            m_edSketch.GeometryType = esriGeometryType.esriGeometryPolyline;
            m_csc = new TraceConstructorClass();
            // Activate a shape constructor based on the current sketch geometry
            //if (m_edSketch.GeometryType == esriGeometryType.esriGeometryPoint | m_edSketch.GeometryType == esriGeometryType.esriGeometryMultipoint)
            //    m_csc = new PointConstructorClass();
            //else
            //   m_csc = new StraightConstructorClass();

            m_csc.Initialize(m_editor);
            m_edSketch.ShapeConstructor = m_csc;
            m_csc.Activate();

            // Setup events
            m_editEvents.OnSketchModified += OnSketchModified;
            m_editEvents5.OnShapeConstructorChanged += OnShapeConstructorChanged;
            m_editEvents.OnSketchFinished += OnSketchFinished;
        }

        protected override bool OnDeactivate()
        {
            m_editEvents.OnSketchModified -= OnSketchModified;
            m_editEvents5.OnShapeConstructorChanged -= OnShapeConstructorChanged;
            m_editEvents.OnSketchFinished -= OnSketchFinished;
            return true;
        }

        protected override void OnDoubleClick()
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                // Finish part
                ISketchOperation pso = new SketchOperation();
                pso.MenuString_2 = "Finish Sketch Part";
                pso.Start(m_editor);
                m_edSketch.FinishSketchPart();
                pso.Finish(null);
            }
            else
                m_edSketch.FinishSketch();
        }

        private void OnSketchModified()
        {
            m_csc.SketchModified();
        }

        private void OnShapeConstructorChanged()
        {
            // Activate a new constructor
            m_csc.Deactivate();
            m_csc = null;
            m_csc = m_edSketch.ShapeConstructor;
            if (m_csc != null)
                m_csc.Activate();
        }

        private void OnSketchFinished()
        {
            try
            {
                IGeometry queryShape = ((ITopologicalOperator)m_edSketch.Geometry).Buffer(ArcMap.Document.SearchTolerance);

                int indxRadius = m_fabricLines.Fields.FindField("RADIUS");
                int indxCenterpointID = m_fabricLines.Fields.FindField("CENTERPOINTID");
                int indxParcelID = m_fabricLines.Fields.FindField("PARCELID");
                
                //Query Lines
                ISpatialFilter filter = new SpatialFilter()
                {
                    Geometry = queryShape,
                    GeometryField = m_fabricLines.ShapeFieldName,
                    SpatialRel = esriSpatialRelEnum.esriSpatialRelContains
                };
                IFeatureCursor cursor = m_fabricLines.Search(filter, true);
                IFeature feature = null;
                List<Line> curves = new List<Line>();
                double radius;
                int parcel, centerpoint;
                while ((feature = cursor.NextFeature()) != null)
                {
                    bool isRadius = feature.SafeRead(indxRadius, out radius);
                    bool isCenterpoint = feature.SafeRead(indxCenterpointID, out centerpoint);
                    bool isParcel = feature.SafeRead(indxParcelID, out parcel);
                                        
                    if (!isParcel)
                        throw new Exception("A null pracel identifier has been detected");
                    if (isRadius ^ isCenterpoint)
                        throw new Exception("A feature should have radius and centerpoint information set, or should not have either of the values set.");

                    curves.Add((isRadius) ?
                        new Line(feature.OID, isRadius, radius, centerpoint, parcel, feature.ShapeCopy) :
                        new StraightLine(feature.OID, parcel, feature.ShapeCopy));
                }
                Marshal.ReleaseComObject(cursor);

                if (curves.Count() > 0)
                {
                    SmoothContext context = new SmoothContext(m_cadFab, m_editor, curves);
                    System.Windows.Controls.ScrollViewer scrollViewer = new System.Windows.Controls.ScrollViewer()
                    {
                        HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
                    };
                    SmoothControl control = new SmoothControl() { DataContext = context };
                    scrollViewer.Content = control;

                    System.Windows.Window dialog = new System.Windows.Window()
                    {
                        SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
                        Content = scrollViewer
                    };
                    control.OKClicked += (sender,e) => { 
                            if (context.WarningCount > 0)
                            {
                                if (System.Windows.Forms.DialogResult.Yes != MessageBox.Show(
                                    String.Format("There are {0} warnings present in the selected segments.  Are you sure you want to apply the updates?", context.WarningCount), "Warnings Present", MessageBoxButtons.YesNo))
                                {
                                    return;
                                }
                            }

                            if (Math.Abs(context.Value - context.Median) > (context.Median * 0.05))
                            {
                                if (System.Windows.Forms.DialogResult.Yes != MessageBox.Show(
                                    "The current value is more than 5% different than the current median value.  Are you sure you want to apply the updates?", "Value Difference", MessageBoxButtons.YesNo))
                                {
                                    return;
                                }
                            }
                        context.Update();
                        dialog.Close();
                    };
                    control.CancelClicked += (sender,e)=>{
                        dialog.Close();
                    };
                    dialog.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No features were found");
                }
            }
            catch (Exception exx)
            {
                System.Windows.Forms.MessageBox.Show(exx.Message);
            }
        }


    }

}
