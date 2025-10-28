namespace X39.Software.ExamMaker.Api.Storage.Meta;

public interface IRefersToMany<TEntity>
{
    ICollection<TEntity>? Entities { get; set; }
}
