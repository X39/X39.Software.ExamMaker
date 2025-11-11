using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace X39.Software.ExamMaker.Api.DataTransferObjects;

public record struct UpdateValue<T>(T Value)
{
    public static implicit operator T(UpdateValue<T> value) => value.Value;
    public static implicit operator T?(UpdateValue<T>? value) => value != null ? value.Value.Value : default;
}
