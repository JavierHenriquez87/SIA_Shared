using Microsoft.AspNetCore.Mvc;
using SIA.Helpers;
using SIA.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using SIA.Context;
using Microsoft.EntityFrameworkCore;

namespace SIA.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;

        public DashBoardController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            this._config = config;
        }

        // GET: Dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}