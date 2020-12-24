using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.UndoRedoOperations
{
    public class AddStrokeOperation : IUndoRedoOperation
    {
        public InkStroke AddedStroke { get; }

        public AddStrokeOperation(InkStroke inkStroke)
        {
            var strokeBuilder = new InkStrokeBuilder();
            strokeBuilder.SetDefaultDrawingAttributes(inkStroke.DrawingAttributes);
            System.Numerics.Matrix3x2 matr = inkStroke.PointTransform;
            IReadOnlyList<InkPoint> inkPoints = inkStroke.GetInkPoints();
            AddedStroke = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
            AddedStroke.StrokeStartedTime = inkStroke.StrokeStartedTime;
        }

        public void Undo(InkCanvas inkCanvas)
        {
            foreach (InkStroke isk in inkCanvas.InkPresenter.StrokeContainer.GetStrokes())
            {
                if (isk.StrokeStartedTime == AddedStroke.StrokeStartedTime)
                {
                    isk.Selected = true;
                }
            }
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }

        public void Redo(InkCanvas inkCanvas)
        {
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(AddedStroke);
        }

        public UndoRedoOperation GetUndoRedoOperation() => UndoRedoOperation.AddStroke;
    }
}
