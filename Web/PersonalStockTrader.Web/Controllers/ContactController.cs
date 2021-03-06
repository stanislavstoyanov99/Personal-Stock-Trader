﻿namespace PersonalStockTrader.Web.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using PersonalStockTrader.Common;
    using PersonalStockTrader.Services.Data;
    using PersonalStockTrader.Services.Messaging;
    using PersonalStockTrader.Web.ViewModels.Contact;

    public class ContactController : BaseController
    {
        private readonly IContactFormService contactFormService;
        private readonly IEmailSender emailSender;

        public ContactController(IContactFormService contactFormService, IEmailSender emailSender)
        {
            this.contactFormService = contactFormService;
            this.emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ContactFormViewModel input)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(input);
            }

            var result = await this.contactFormService.AddAsync(input);

            if (result)
            {
                await this.emailSender.SendEmailAsync(
                    input.Email,
                    input.Name,
                    GlobalConstants.SystemEmail,
                    input.Subject ?? GlobalConstants.ConstSubject,
                    input.Content);

                this.TempData["InfoMessage"] = "Thank you for your email!";
            }

            return this.Redirect("/Home/Index");
        }
    }
}
