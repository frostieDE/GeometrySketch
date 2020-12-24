using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.UndoRedoOperations
{
    public class EraseStrokeOperation : IUndoRedoOperation
    {
        public InkStroke ErasedStroke { get; }

        public EraseStrokeOperation(InkStroke inkStroke)
        {
            var stroke = inkStroke;

            var strokeBuilder = new InkStrokeBuilder();
            strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
            System.Numerics.Matrix3x2 matr = stroke.PointTransform;
            IReadOnlyList<InkPoint> inkPoints = stroke.GetInkPoints();
            ErasedStroke = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
            ErasedStroke.StrokeStartedTime = stroke.StrokeStartedTime;
        }

        public void Undo(InkCanvas inkCanvas)
        {
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(ErasedStroke);
        }

        public void Redo(InkCanvas inkCanvas)
        {
            foreach (InkStroke isk in inkCanvas.InkPresenter.StrokeContainer.GetStrokes())
            {
                if (isk.StrokeStartedTime == ErasedStroke.StrokeStartedTime)
                {
                    isk.Selected = true;
                }
            }
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }

        public UndoRedoOperation GetUndoRedoOperation() => UndoRedoOperation.EraseStroke;


    }
}
