using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

public class EstadoModuloController : Controller
{
    public IActionResult Pendiente(string nombre)
    {
        ViewData["Modulo"] = string.IsNullOrWhiteSpace(nombre)
            ? "Módulo"
            : nombre;

        return View("~/Views/Shared/ModuloPendiente.cshtml");
    }
}