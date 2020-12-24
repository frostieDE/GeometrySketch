using GeometrySketch.Base;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.UndoRedoOperations
{
    public class UndoRedoBase : Observable
    {
        private List<IUndoRedoOperation> undoneOperations;
        public List<IUndoRedoOperation> UndoneOperations { get { return undoneOperations; } set { undoneOperations = value; OnPropertyChanged(); } }

        private List<IUndoRedoOperation> redoneOperations;
        public List<IUndoRedoOperation> RedoneOperations { get { return redoneOperations; } set { redoneOperations = value; OnPropertyChanged(); } }

        public void AddOperationToUndoneOperations(IUndoRedoOperation undoRedoOperation)
        {
            UndoneOperations.Add(undoRedoOperation);
            OnPropertyChanged(nameof(CanUndo));
        }

        public void AddOperationToRedoneOperations(IUndoRedoOperation undoRedoOperation)
        {
            RedoneOperations.Add(undoRedoOperation);
            OnPropertyChanged(nameof(CanRedo));
        }

        public void RemoveLastUndoneOperation()
        {
            UndoneOperations.Remove(UndoneOperations.Last());
            OnPropertyChanged(nameof(CanUndo));
        }

        public void RemoveLastRedoneOperation()
        {
            RedoneOperations.Remove(RedoneOperations.Last());
            OnPropertyChanged(nameof(CanRedo));
        }

        public bool CanUndo { get { if (UndoneOperations.Count > 0) { return true; } else { return false; } } }

        public bool CanRedo { get { if (RedoneOperations.Count > 0) { return true; } else { return false; } } }

        public void Undo(InkCanvas inkCanvas)
        {
            if (UndoneOperations.Count > 0)
            {
                UndoneOperations.Last().Undo(inkCanvas);

                if (UndoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.AddStroke)
                {
                    AddStrokeOperation aso = UndoneOperations.Last() as AddStrokeOperation;
                    AddStrokeOperation ason = new AddStrokeOperation(aso.AddedStroke);
                    AddOperationToRedoneOperations(ason);
                }
                else if (UndoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.EraseStroke)
                {
                    EraseStrokeOperation eso = UndoneOperations.Last() as EraseStrokeOperation;
                    EraseStrokeOperation eson = new EraseStrokeOperation(eso.ErasedStroke);
                    AddOperationToRedoneOperations(eson);
                }
                else if (UndoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.DeleteAll)
                {
                    DeleteAllOperation dao = UndoneOperations.Last() as DeleteAllOperation;
                    DeleteAllOperation daon = new DeleteAllOperation(dao.DeletedStrokes);
                    AddOperationToRedoneOperations(daon);
                }
                else if (UndoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.EraseByPoint)
                {
                    EraseByPointOperation ebpo = UndoneOperations.Last() as EraseByPointOperation;
                    EraseByPointOperation ebpon = new EraseByPointOperation(ebpo.StrokesBefore, ebpo.StrokesAfter);
                    AddOperationToRedoneOperations(ebpon);
                }
                else
                {

                }
                RemoveLastUndoneOperation();
            }
        }

        public void Redo(InkCanvas inkCanvas)
        {
            if (RedoneOperations.Count > 0)
            {
                RedoneOperations.Last().Redo(inkCanvas);

                if (RedoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.AddStroke)
                {
                    AddStrokeOperation aso = RedoneOperations.Last() as AddStrokeOperation;
                    AddStrokeOperation ason = new AddStrokeOperation(aso.AddedStroke);
                    AddOperationToUndoneOperations(ason);
                }
                else if (RedoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.EraseStroke)
                {
                    EraseStrokeOperation eso = RedoneOperations.Last() as EraseStrokeOperation;
                    EraseStrokeOperation eson = new EraseStrokeOperation(eso.ErasedStroke);
                    AddOperationToUndoneOperations(eson);
                }
                else if (RedoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.DeleteAll)
                {
                    DeleteAllOperation dao = RedoneOperations.Last() as DeleteAllOperation;
                    DeleteAllOperation daon = new DeleteAllOperation(dao.DeletedStrokes);
                    AddOperationToUndoneOperations(daon);
                }
                else if (RedoneOperations.Last().GetUndoRedoOperation() == UndoRedoOperation.EraseByPoint)
                {
                    EraseByPointOperation ebpo = RedoneOperations.Last() as EraseByPointOperation;
                    EraseByPointOperation ebpon = new EraseByPointOperation(ebpo.StrokesBefore, ebpo.StrokesAfter);
                    AddOperationToUndoneOperations(ebpon);
                }
                else
                {

                }
                RemoveLastRedoneOperation();
            }
        }

        public UndoRedoBase()
        {
            UndoneOperations = new List<IUndoRedoOperation>();
            RedoneOperations = new List<IUndoRedoOperation>();
        }
    }
}
