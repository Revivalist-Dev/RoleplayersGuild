using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using System;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.ViewComponents
{
    public class SiteFooterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var viewModel = new SiteFooterViewModel
            {
                CurrentYear = DateTime.Now.Year
            };
            return View(viewModel);
        }
    }
}