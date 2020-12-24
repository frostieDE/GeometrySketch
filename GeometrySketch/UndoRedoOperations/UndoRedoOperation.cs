using Windows.UI.Xaml.Controls;

namespace GeometrySketch.UndoRedoOperations
{
    public enum UndoRedoOperation
    {
        AddStroke,
        EraseStroke,
        EraseByPoint,
        DeleteAll,
    }

    public interface IUndoRedoOperation
    {
        void Undo(InkCanvas inkCanvas);

        void Redo(InkCanvas inkCanvas);

        UndoRedoOperation GetUndoRedoOperation();
    }
}
