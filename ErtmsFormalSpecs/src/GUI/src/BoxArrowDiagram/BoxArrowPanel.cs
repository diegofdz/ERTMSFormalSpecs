// ------------------------------------------------------------------------------
// -- Copyright ERTMS Solutions
// -- Licensed under the EUPL V.1.1
// -- http://joinup.ec.europa.eu/software/page/eupl/licence-eupl
// --
// -- This file is part of ERTMSFormalSpec software and documentation
// --
// --  ERTMSFormalSpec is free software: you can redistribute it and/or modify
// --  it under the terms of the EUPL General Public License, v.1.1
// --
// -- ERTMSFormalSpec is distributed in the hope that it will be useful,
// -- but WITHOUT ANY WARRANTY; without even the implied warranty of
// -- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// --
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DataDictionary.Constants;
using DataDictionary.Types;

namespace GUI.BoxArrowDiagram
{
    public abstract partial class BoxArrowPanel<BoxModel, ArrowModel> : Panel
        where BoxModel : class, DataDictionary.IGraphicalDisplay
        where ArrowModel : class, DataDictionary.IGraphicalArrow<BoxModel>
    {
        private System.Windows.Forms.ToolStripMenuItem refreshMenuItem;

        private void InitializeStartMenu()
        {
            // 
            // Refresh
            // 
            refreshMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            refreshMenuItem.Name = "refreshMenuItem";
            refreshMenuItem.Size = new System.Drawing.Size(161, 22);
            refreshMenuItem.Text = "Refresh";
            refreshMenuItem.Click += new System.EventHandler(refreshBoxMenuItem_Click);

            contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                refreshMenuItem});
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BoxArrowPanel()
        {
            InitializeComponent();
            InitializeStartMenu();

            MouseDown += new MouseEventHandler(BoxArrowPanel_MouseDown);
            MouseMove += new MouseEventHandler(BoxArrowPanel_MouseMove);
            MouseUp += new MouseEventHandler(BoxArrowPanel_MouseUp);
        }

        /// <summary>
        /// Refreshes the panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshBoxMenuItem_Click(object sender, EventArgs e)
        {
            RefreshControl();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container"></param>
        public BoxArrowPanel(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            MouseDown += new MouseEventHandler(BoxArrowPanel_MouseDown);
            MouseMove += new MouseEventHandler(BoxArrowPanel_MouseMove);
        }

        /// <summary>
        /// The selected object
        /// </summary>
        public object Selected { get; set; }

        /// <summary>
        /// Method used to create a box
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public abstract BoxControl<BoxModel, ArrowModel> createBox(BoxModel model);

        /// <summary>
        /// Method used to create an arrow
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public abstract ArrowControl<BoxModel, ArrowModel> createArrow(ArrowModel model);

        /// <summary>
        /// The arrow that is currently being changed
        /// </summary>
        private ArrowControl<BoxModel, ArrowModel> changingArrow = null;

        /// <summary>
        /// The action that is applied on the arrow
        /// </summary>
        private enum ChangeAction { None, InitialBox, TargetBox };
        private ChangeAction chaningArrowAction = ChangeAction.None;

        void BoxArrowPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point clickPoint = new Point(e.X, e.Y);
                foreach (ArrowControl<BoxModel, ArrowModel> arrow in arrows.Values)
                {
                    if (around(arrow.StartLocation, clickPoint))
                    {
                        changingArrow = arrow;
                        changingArrow.Parent = this;   // I do not know why...
                        chaningArrowAction = ChangeAction.InitialBox;
                        break;
                    }
                    if (around(arrow.TargetLocation, clickPoint))
                    {
                        changingArrow = arrow;
                        changingArrow.Parent = this;   // I do not know why...
                        chaningArrowAction = ChangeAction.TargetBox;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the move event, which, in case of an arrow is selected to be modified, 
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BoxArrowPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (changingArrow != null && chaningArrowAction != ChangeAction.None)
            {
                foreach (BoxControl<BoxModel, ArrowModel> box in boxes.Values)
                {
                    if ((e.X > box.Location.X && e.X < box.Location.X + box.Width) &&
                        (e.Y > box.Location.Y && e.Y < box.Location.Y + box.Height))
                    {
                        switch (chaningArrowAction)
                        {
                            case ChangeAction.InitialBox:
                                if (changingArrow.Model.Source != box.Model)
                                {
                                    changingArrow.SetInitialBox(box.Model);
                                }
                                break;
                            case ChangeAction.TargetBox:
                                if (changingArrow.Model.Target != box.Model)
                                {
                                    if (changingArrow.Model.Source != null)
                                    {
                                        changingArrow.SetTargetBox(box.Model);
                                    }
                                }
                                break;
                        }
                        break;
                    }
                }
            }
        }

        void BoxArrowPanel_MouseUp(object sender, MouseEventArgs e)
        {
            changingArrow = null;
            chaningArrowAction = ChangeAction.None;
        }

        /// <summary>
        /// The maximum delta when considering if two points are near one from the other
        /// </summary>
        private const int MAX_DELTA = 5;

        /// <summary>
        /// Indicates whether two points are near one from the other
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private bool around(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) < MAX_DELTA && Math.Abs(p1.Y - p2.Y) < MAX_DELTA;
        }

        /// <summary>
        /// Handles the add box event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBoxHandler(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the add arrow event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddArrowHandler(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Provides access to the enclosing MDI window
        /// </summary>
        public MainWindow MDIWindow
        {
            get
            {
                MainWindow retVal = null;

                Control current = this;
                while (current != null && retVal == null)
                {
                    retVal = current as MainWindow;
                    current = current.Parent;
                }

                return retVal;
            }
        }

        /// <summary>
        /// The dictionary used to keep the relation between boxe controls and their model
        /// </summary>
        private Dictionary<BoxModel, BoxControl<BoxModel, ArrowModel>> boxes = new Dictionary<BoxModel, BoxControl<BoxModel, ArrowModel>>();

        /// <summary>
        /// Provides the box control which corresponds to the model provided
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BoxControl<BoxModel, ArrowModel> getBoxControl(BoxModel model)
        {
            BoxControl<BoxModel, ArrowModel> retVal = null;

            if (model != null)
            {
                if (boxes.ContainsKey(model))
                {
                    retVal = boxes[model];
                }
            }

            return retVal;
        }

        /// <summary>
        /// The dictionary used to keep the relation between arrows and their model
        /// </summary>
        private Dictionary<ArrowModel, ArrowControl<BoxModel, ArrowModel>> arrows = new Dictionary<ArrowModel, ArrowControl<BoxModel, ArrowModel>>();

        /// <summary>
        /// Provides the arrow control which corresponds to the model provided
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ArrowControl<BoxModel, ArrowModel> getArrowControl(ArrowModel model)
        {
            ArrowControl<BoxModel, ArrowModel> retVal = null;

            if (arrows.ContainsKey(model))
            {
                retVal = arrows[model];
            }

            return retVal;
        }

        /// <summary>
        /// Provides the arrow control which corresponds to the rule
        /// </summary>
        /// <param name="referencedModel"></param>
        /// <returns></returns>
        public ArrowControl<BoxModel, ArrowModel> getArrowControl(DataDictionary.ModelElement referencedModel)
        {
            ArrowControl<BoxModel, ArrowModel> retVal = null;

            foreach (ArrowControl<BoxModel, ArrowModel> control in arrows.Values)
            {
                if (control.Model.ReferencedModel == referencedModel)
                {
                    retVal = control;
                    break;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Indicates whether the layout should be suspended
        /// </summary>
        bool refreshingControl = false;

        /// <summary>
        /// Refreshes the layout, if it is not suspended
        /// </summary>
        public override void Refresh()
        {
            if (!refreshingControl)
            {
                base.Refresh();
            }
        }

        /// <summary>
        /// Provides the boxes that need be displayed
        /// </summary>
        /// <returns></returns>
        public abstract List<BoxModel> getBoxes();

        /// <summary>
        /// Provides the arrows that need be displayed
        /// </summary>
        /// <returns></returns>
        public abstract List<ArrowModel> getArrows();

        /// <summary>
        /// Refreshes the control according to the state machine
        /// </summary>
        public void RefreshControl()
        {
            try
            {
                refreshingControl = true;
                SuspendLayout();

                foreach (BoxControl<BoxModel, ArrowModel> control in boxes.Values)
                {
                    control.Parent = null;
                }
                boxes.Clear();

                foreach (ArrowControl<BoxModel, ArrowModel> control in arrows.Values)
                {
                    control.Parent = null;
                }
                arrows.Clear();

                foreach (BoxModel model in getBoxes())
                {
                    BoxControl<BoxModel, ArrowModel> boxControl = createBox(model);
                    boxControl.Parent = this;
                    boxControl.RefreshControl();
                    boxes[model] = boxControl;
                }
                foreach (ArrowModel model in getArrows())
                {
                    if (model.Source != null && !model.Source.Hidden && model.Target != null && !model.Target.Hidden)
                    {
                        ArrowControl<BoxModel, ArrowModel> arrowControl = createArrow(model);
                        arrowControl.Parent = this;
                        arrows[model] = arrowControl;
                    }
                }
                UpdateArrowPosition();
            }
            finally
            {
                refreshingControl = false;
                ResumeLayout(true);
            }

            Refresh();
        }

        /// <summary>
        /// Handles the rectangles that are already allocated in the diagram
        /// </summary>
        private class BoxAllocation
        {
            /// <summary>
            /// The allocated rectangles
            /// </summary>
            List<Rectangle> AllocatedBoxes = new List<Rectangle>();

            /// <summary>
            /// Constructor
            /// </summary>
            public BoxAllocation()
            {
            }

            /// <summary>
            /// Finds a rectangle which intersects with the current rectangle
            /// </summary>
            /// <param name="rectangle"></param>
            /// <returns></returns>
            public Rectangle Intersects(Rectangle rectangle)
            {
                Rectangle retVal = Rectangle.Empty;

                foreach (Rectangle current in AllocatedBoxes)
                {
                    if (current.IntersectsWith(rectangle))
                    {
                        retVal = current;
                        break;
                    }
                }

                return retVal;
            }

            /// <summary>
            /// Allocates a new rectangle 
            /// </summary>
            /// <param name="rectangle"></param>
            public void Allocate(Rectangle rectangle)
            {
                AllocatedBoxes.Add(rectangle);
            }
        }

        /// <summary>
        /// The allocated boxes
        /// </summary>
        private BoxAllocation AllocatedBoxes;

        /// <summary>
        /// Provides a distance between two points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private int distance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        /// <summary>
        /// Updates the arrows position to ensure that no overlap exists
        ///   - on the arrows
        ///   - on their text
        /// </summary>
        public void UpdateArrowPosition()
        {
            ComputeArrowPosition();
            ComputeArrowTextPosition();
        }

        /// <summary>
        /// The size of the shift between arrows to be used when overlap occurs (more or less horizontally)
        /// </summary>
        static int HORIZONTAL_SHIFT_SIZE = 40;

        /// <summary>
        /// The size of the shift between arrows to be used when overlap occurs (more or less horizontally)
        /// </summary>
        static int VERTICAL_SHIFT_SIZE = 20;

        /// <summary>
        /// Ensures that two arrowss do not overlap by computing an offset between the arrows
        /// </summary>
        private void ComputeArrowPosition()
        {
            List<ArrowControl<BoxModel, ArrowModel>> workingSet = new List<ArrowControl<BoxModel, ArrowModel>>();
            workingSet.AddRange(arrows.Values);

            while (workingSet.Count > 1)
            {
                ArrowControl<BoxModel, ArrowModel> t1 = workingSet[0];
                workingSet.Remove(t1);

                // Compute the set of arrows overlapping with t1
                List<ArrowControl<BoxModel, ArrowModel>> overlap = new List<ArrowControl<BoxModel, ArrowModel>>();
                overlap.Add(t1);
                foreach (ArrowControl<BoxModel, ArrowModel> t in workingSet)
                {
                    if (t.Model.Source == t1.Model.Source &&
                        t.Model.Target == t1.Model.Target)
                    {
                        overlap.Add(t);
                    }
                    else if ((t.Model.Source == t1.Model.Target &&
                        t.Model.Target == t1.Model.Source))
                    {
                        overlap.Add(t);
                    }
                }

                // Remove all arrows of this overlap class from the working set
                foreach (ArrowControl<BoxModel, ArrowModel> t in overlap)
                {
                    workingSet.Remove(t);
                }

                // Shift arrows of this overlap set if they are overlapping (that is, if the set size > 1)
                if (overlap.Count > 1)
                {
                    Point shift;        // the shift to be applied to the current arrow
                    Point offset;       // the offset to apply on all arrows of this overlap set

                    double angle = overlap[0].Angle;
                    if ((angle > Math.PI / 4 && angle < 3 * Math.PI / 4) ||
                        (angle < -Math.PI / 4 && angle > -3 * Math.PI / 4))
                    {
                        // Horizontal shift
                        shift = new Point(-(overlap.Count - 1) * HORIZONTAL_SHIFT_SIZE / 2, 0);
                        offset = new Point(HORIZONTAL_SHIFT_SIZE, 0);
                    }
                    else
                    {
                        // Vertical shift
                        shift = new Point(0, -(overlap.Count - 1) * VERTICAL_SHIFT_SIZE / 2);
                        offset = new Point(0, VERTICAL_SHIFT_SIZE);
                    }

                    int i = 0;
                    foreach (ArrowControl<BoxModel, ArrowModel> arrow in overlap)
                    {
                        arrow.Offset = shift;
                        shift.Offset(offset);

                        if (arrow.TargetBoxControl == null)
                        {
                            arrow.EndOffset = new Point(0, VERTICAL_SHIFT_SIZE * i / 2);
                        }
                        i = i + 1;
                    }
                }
            }
        }

        /// <summary>
        /// Computes the position of the arrow texts, following the arrow, to avoid text overlap
        /// </summary>
        private void ComputeArrowTextPosition()
        {
            AllocatedBoxes = new BoxAllocation();

            // Allocate all boxes as non available
            foreach (BoxControl<BoxModel, ArrowModel> box in boxes.Values)
            {
                Rectangle rectangle = box.DisplayRectangle;
                rectangle.Offset(box.Location);
                AllocatedBoxes.Allocate(rectangle);
            }

            foreach (ArrowControl<BoxModel, ArrowModel> arrow in arrows.Values)
            {
                Point center = arrow.getCenter();
                Point upSlide = Slide(arrow, center, ArrowControl<BoxModel, ArrowModel>.SlideDirection.Up);
                Point downSlide = Slide(arrow, center, ArrowControl<BoxModel, ArrowModel>.SlideDirection.Down);

                Rectangle boundingBox;
                if (distance(center, upSlide) <= distance(center, downSlide))
                {
                    boundingBox = arrow.getTextBoundingBox(upSlide);
                }
                else
                {
                    boundingBox = arrow.getTextBoundingBox(downSlide);
                }

                arrow.Location = new Point(boundingBox.X, boundingBox.Y);
                AllocatedBoxes.Allocate(boundingBox);
            }
        }

        /// <summary>
        /// Tries to slide the arrow up following the arrow to avoid any collision
        /// with the already allocated bounding boxes
        /// </summary>
        /// <param name="arrow"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        private Point Slide(ArrowControl<BoxModel, ArrowModel> arrow, Point center, ArrowControl<BoxModel, ArrowModel>.SlideDirection direction)
        {
            Point retVal = center;
            Rectangle colliding = AllocatedBoxes.Intersects(arrow.getTextBoundingBox(retVal));

            while (colliding != Rectangle.Empty)
            {
                retVal = arrow.Slide(retVal, colliding, direction);
                colliding = AllocatedBoxes.Intersects(arrow.getTextBoundingBox(retVal));
            }

            return retVal;
        }

        private Point currentPosition = new Point(1, 1);
        private static int X_OFFSET = BoxControl<BoxModel, ArrowModel>.DEFAULT_SIZE.Width + 10;
        private static int Y_OFFSET = BoxControl<BoxModel, ArrowModel>.DEFAULT_SIZE.Height + 10;

        /// <summary>
        /// Provides the next available position in the box-arrow diagram
        /// </summary>
        /// <returns></returns>
        public Point GetNextPosition()
        {
            Point retVal = new Point(currentPosition.X, currentPosition.Y);

            currentPosition.Offset(X_OFFSET, 0);
            if (currentPosition.X > Size.Width - BoxControl<BoxModel, ArrowModel>.DEFAULT_SIZE.Width)
            {
                currentPosition = new Point(1, currentPosition.Y + Y_OFFSET);
            }

            return retVal;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            SuspendLayout();
            foreach (BoxControl<BoxModel, ArrowModel> control in boxes.Values)
            {
                control.PaintInBoxArrowPanel(e);
            }

            foreach (ArrowControl<BoxModel, ArrowModel> control in arrows.Values)
            {
                control.PaintInBoxArrowPanel(e);
            }
            ResumeLayout(true);
        }

        public void ControlHasMoved()
        {
            UpdateArrowPosition();
        }

        /// <summary>
        /// Provides the enclosing form
        /// </summary>
        protected BoxArrowWindow<BoxModel, ArrowModel> EnclosingWindow
        {
            get { return GUIUtils.EnclosingFinder<BoxArrowWindow<BoxModel, ArrowModel>>.find(this); }
        }

        /// <summary>
        /// Selects a model element
        /// </summary>
        /// <param name="model"></param>
        public void Select(object model)
        {
            if (EnclosingWindow != null)
            {
                EnclosingWindow.Select(model);
            }
            else
            {
                if (model is BoxControl<BoxModel, ArrowModel>)
                {
                    BoxControl<BoxModel, ArrowModel> control = model as BoxControl<BoxModel, ArrowModel>;
                    MDIWindow.Select(control.Model);
                }
                else if (model is ArrowControl<BoxModel, ArrowModel>)
                {
                    ArrowControl<BoxModel, ArrowModel> control = model as ArrowControl<BoxModel, ArrowModel>;
                    MDIWindow.Select(control.Model.ReferencedModel);
                }
            }

            Refresh();
        }

        /// <summary>
        /// Indicates whether the control is selected
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        internal bool isSelected(Control control)
        {
            return EnclosingWindow.isSelected(control);
        }
    }
}
