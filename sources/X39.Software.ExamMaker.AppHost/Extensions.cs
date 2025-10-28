namespace X39.Software.ExamMaker.AppHost;

public static class Extensions
{
    public static IResourceBuilder<TDestination> WithAwaitedReference<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResourceWithConnectionString> source,
        string? connectionName = null,
        bool optional = false
    )
        where TDestination : IResourceWithEnvironment, IResourceWithWaitSupport
        => builder.WithReference(source, connectionName, optional)
            .WaitFor(source);
}
