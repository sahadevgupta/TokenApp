namespace TokenApp.Configurations.Mapper.Converters
{
    public interface IConverter<in TSource, out TDestination>
    {
        TDestination Convert(TSource source);
    }
}
