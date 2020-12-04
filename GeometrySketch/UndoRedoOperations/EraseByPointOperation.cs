using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.UndoRedoOperations
{
    public class EraseByPointOperation : IUndoRedoOperation
    {        
        public List<InkStroke> StrokesBefore { get; }
        
        public List<InkStroke> StrokesAfter { get; } 

        public EraseByPointOperation(List<InkStroke> strokesBefore, List<InkStroke> strokesAfter)
        {
            StrokesBefore = new List<InkStroke>();
            StrokesBefore = strokesBefore;
            StrokesAfter = new List<InkStroke>();
            StrokesAfter = strokesAfter;
        } 
        
        public void Undo(InkCanvas inkCanvas)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            foreach (InkStroke isk in StrokesBefore)
            {                
                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(isk.DrawingAttributes);
                System.Numerics.Matrix3x2 matr = isk.PointTransform;
                IReadOnlyList<InkPoint> inkPoints = isk.GetInkPoints();
                InkStroke inkStroke = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
                inkStroke.StrokeStartedTime = isk.StrokeStartedTime;

                inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            }
        }
        public void Redo(InkCanvas inkCanvas)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            foreach (InkStroke isk in StrokesAfter)
            {                
                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(isk.DrawingAttributes);
                System.Numerics.Matrix3x2 matr = isk.PointTransform;
                IReadOnlyList<InkPoint> inkPoints = isk.GetInkPoints();
                InkStroke inkStroke = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
                inkStroke.StrokeStartedTime = isk.StrokeStartedTime;

                inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            }
        }

        public UndoRedoOperation GetUndoRedoOperation() => UndoRedoOperation.EraseByPoint;        
    }
}
