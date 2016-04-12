using bstrkr.core;

namespace bstrkr.providers
{
    public interface ILiveDataProviderFactory
    {
        ILiveDataProvider GetCurrentProvider();

        ILiveDataProvider CreateProvider(Area area);
    }
}