﻿namespace webapi.Services
{
    public interface ISendMailSerVice
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailAsync(List<string> email, string subject, string htmlMessage, List<string> imagePath);
    }
}
