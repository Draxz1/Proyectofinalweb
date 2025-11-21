namespace megadeliciasapi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailDestino, string asunto, string mensajeHtml);
    }
}