using Newtonsoft.Json;

namespace Firefly.Helpers
{
    public class ObjectCopier
    {
        public static T ShallowCopy<T>(T thing)
        {
            // Make something swiftier when necessary
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(thing,
                new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
        }
    }
}