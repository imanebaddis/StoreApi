namespace StoreApi.Infra.Repositories;

public sealed class CategoryNameConflictException : Exception
{
    public CategoryNameConflictException(string name, Exception? innerException = null)
        : base($"Esiste già una categoria con il nome \"{name}\".", innerException) { }
}
