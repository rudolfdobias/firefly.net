using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Firefly.Extensions
{
    public static class EntityFramework
    {
        public static async Task EnsureLoaded(this NavigationEntry me)
        {
            if (!me.IsLoaded)
            {
                await me.LoadAsync();
            }
        }
    }
}