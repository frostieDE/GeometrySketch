using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.UndoRedoOperations
{
    public class DeleteAllOperation : IUndoRedoOperation
    {        
        public List<InkStroke> DeletedStrokes { get; }        

        public DeleteAllOperation(IReadOnlyList<InkStroke> inkStrokes)
        {
            DeletedStrokes = new List<InkStroke>();
            foreach (InkStroke isk in inkStrokes)
            {
                DeletedStrokes.Add(isk);
            }
        }

        public void Undo(InkCanvas inkCanvas)
        {
            foreach (InkStroke isk in DeletedStrokes)
            {
                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(isk.DrawingAttributes);
                System.Numerics.Matrix3x2 matr = isk.PointTransform;
                IReadOnlyList<InkPoint> inkPoints = isk.GetInkPoints();

                inkCanvas.InkPresenter.StrokeContainer.AddStroke(strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr, isk.StrokeStartedTime, isk.StrokeDuration));
            }            
        }
        
        public void Redo(InkCanvas inkCanvas)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
        }

        public UndoRedoOperation GetUndoRedoOperation() => UndoRedoOperation.DeleteAll;        
    }
}
