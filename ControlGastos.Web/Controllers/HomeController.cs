using Microsoft.AspNetCore.Mvc;

namespace ControlGastos.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
