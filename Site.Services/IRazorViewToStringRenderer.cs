using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}