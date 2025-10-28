namespace X39.Software.ExamMaker.Api.Storage.Meta;

public interface IRefersToOneOptional<TEntity, TPrimaryKey> where TEntity : IPrimaryKey<TPrimaryKey> where TPrimaryKey : struct
{
    TEntity? Entity { get; set; }
    TPrimaryKey? EntityId { get; set; }
}
