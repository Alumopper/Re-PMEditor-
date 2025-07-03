namespace PMEditor.Operation;

public abstract class BaseOperation
{
    public abstract void Redo();

    public abstract void Undo();

    public abstract string GetInfo();
}