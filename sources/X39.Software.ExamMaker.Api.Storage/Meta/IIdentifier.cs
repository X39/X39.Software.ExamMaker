namespace X39.Software.ExamMaker.Api.Storage.Meta;

public interface IIdentifier<TIdentifier>
{
    TIdentifier Identifier { get; set; }
}
