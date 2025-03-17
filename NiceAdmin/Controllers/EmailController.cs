using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Services;
using System.Threading.Tasks;

namespace NiceAdmin.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> SendEmail()
        { 

            await _emailService.SendEmailAsync("ritesh.lakhani1507@gmail.com","Normal Check","HII Message Comes From Ritesh");

            return View();
        }
    }
}
