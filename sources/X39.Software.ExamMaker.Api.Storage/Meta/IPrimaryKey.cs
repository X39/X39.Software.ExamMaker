namespace X39.Software.ExamMaker.Api.Storage.Meta;

public interface IPrimaryKey<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
}
